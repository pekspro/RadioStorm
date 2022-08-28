using static Pekspro.RadioStorm.Settings.SynchronizedSettings.Favorite.FavoriteList;

namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.Favorite;

public abstract class FavoriteList : SharedSettingsListBase<FavoriteItem>, IFavoriteList
{
    [DebuggerDisplay("Id: {Id} Active: {IsActive} LastChanged: {LastChangedTimestamp}")]
    public class FavoriteItem
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public uint LastChangedTimestamp { get; set; }
    }

    const ulong FileHeader = 0x4189512414351a1b;

    // public event TypedEventHandler<FavoriteList, FavoriteListChangeEventArgs> FavoriteListDataChanged;
    // public event EventHandler FavoriteListDataUpdated;



    private bool IsDirty = false;

    public string FileName { get; set; } = string.Empty;

    public override string GetFileName(int slotId) => FileName;

    public IMessenger Messenger { get; set; }

    public IDateTimeProvider DateTimeProvider { get; }

    public FavoriteList
        (
            IMessenger messenger, 
            IDateTimeProvider dateTimeProvider,
            ILogger logger, 
            IOptions<StorageLocations> storageLocationOptions
        )
        : base(logger, storageLocationOptions, 1)
    {
        Messenger = messenger;
        DateTimeProvider = dateTimeProvider;
    }

    public void Init(string filename, bool allowBackgroundSaving, string logName)
    {
        if (allowBackgroundSaving)
        {
            TimerInterval = new TimeSpan(0, 0, 1).TotalMilliseconds;

        }

        FileName = filename;
    }

    public bool SetFavorite(int id, bool active)
    {
        var c = Items.FirstOrDefault(o => o.Value.Id == id);
        FavoriteItem current = c.Value;

        if (current is null)
        {
            FavoriteItem favorite = new FavoriteItem()
            {
                Id = id,
                IsActive = active,
                LastChangedTimestamp = TimestampHelper.NowToInt()
            };

            Items.Add(id, favorite);

            SaveLater();

            LatestChangedTime = DateTimeProvider.UtcNow;

            /*
				FavoriteListDataUpdated?.Invoke(this, null);
            FavoriteListDataChanged?.Invoke(this, new FavoriteListChangeEventArgs()
            {
                Id = id,
                IsAdded = active,
                ChangeSource = changeSource
            });
				*/

            SendChangedMessage(id, active);

            return true;
        }
        else
        {
            if (active != current.IsActive)
            {
                current.IsActive = active;
                current.LastChangedTimestamp = TimestampHelper.NowToInt();

                SaveLater();

                LatestChangedTime = DateTimeProvider.UtcNow;

                /*
					FavoriteListDataUpdated?.Invoke(this, null);
                FavoriteListDataChanged?.Invoke(this, new FavoriteListChangeEventArgs()
                {
                    Id = id,
                    IsAdded = active,
                    ChangeSource = changeSource
                });
					*/

                SendChangedMessage(id, active);

                return true;
            }

            return false;
        }
    }

    protected abstract void SendChangedMessage(int id, bool active);

    protected abstract void SendAllChangedMessage();

    public bool IsFavorite(int id)
    {
        if (Items.ContainsKey(id))
        {
            return Items[id].IsActive;
        }

        return false;
    }

    private void SaveLater()
    {
        IsDirty = true;

        RestartTimer();
    }

    public override async Task SaveIfDirtyAsync()
    {
        if (IsDirty)
        {
            Logger.LogInformation("It's time to save {0}", FileName);
            Stopwatch watch = new Stopwatch();

            await SaveAsync(0);

            Messenger.Send(new LocalSharedFileUpdated(FileName));

            Logger.LogInformation("Saved {0} in {1} ms.", FileName, watch.ElapsedMilliseconds);
        }
    }

    protected override Task SaveAsync(int slotId, ReverseBinaryWriter writer)
    {
        writer.WriteUInt64(FileHeader);

        var data = Items;

        writer.WriteInt32(data.Keys.Count);

        foreach (var item in data)
        {
            writer.WriteInt32(item.Value.Id);
            writer.WriteUInt32(item.Value.LastChangedTimestamp);
            writer.WriteBoolean(item.Value.IsActive);
        }

        IsDirty = false;

        return Task.CompletedTask;
    }

    protected override Task<Dictionary<int, FavoriteItem>> ReadAsync(ReverseBinaryReader reader)
    {
        Dictionary<int, FavoriteItem> result = new Dictionary<int, FavoriteItem>();

        ulong header = reader.ReadUInt64();

        if (header != FileHeader)
        {
            throw new Exception("This doesn't seem to be a favorite file.");
        }

        int count = reader.ReadInt32();

        while (count > 0)
        {
            int id = reader.ReadInt32();
            uint timeStamp = reader.ReadUInt32();
            bool isActive = reader.ReadBoolean();

            if (result.ContainsKey(id))
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
            else
            {
                result.Add(id, new FavoriteItem()
                {
                    Id = id,
                    IsActive = isActive,
                    LastChangedTimestamp = timeStamp
                });
            }

            count--;
        }

        return Task.FromResult(result);
    }

    protected override async Task<bool> MergeAsync(Dictionary<int, FavoriteItem> roamingList)
    {
        Logger.LogInformation($"Has {Items.Count} items locally. Got {roamingList.Count} from remote.");

        bool changed = Merge(Items, roamingList);

        if (changed)
        {
            Logger.LogInformation($"Local list was modified during merge. Has now {Items.Count} items.");

            await SaveAsync(0);

            SendAllChangedMessage();
            // FavoriteListDataUpdated?.Invoke(this, null);
        }
        else
        {
            Logger.LogInformation($"No changes in local list after merge.");
        }

        return changed;
    }

    private bool Merge(Dictionary<int, FavoriteItem> localList, Dictionary<int, FavoriteItem> roamingList)
    {
        bool changeDetected = false;
        var localKeys = localList.Keys.ToList();

        foreach (var key in localKeys)
        {
            //Here is a conflict to resolve
            if (roamingList.ContainsKey(key))
            {
                var localValue = localList[key];
                var roamingValue = roamingList[key];

                //If roaming value is changed after local value, use that one instead.
                if (roamingValue.LastChangedTimestamp > localValue.LastChangedTimestamp)
                {
                    Logger.LogInformation("Favorite item {0} updated from roaming settings.", roamingValue.Id);
                    localList[key] = roamingValue;

                    changeDetected = true;
                }

                roamingList.Remove(key);
            }
        }

        foreach (var roamingValue in roamingList)
        {
            Logger.LogInformation("Adding favorite item {0} from roaming settings.", roamingValue.Value.Id);
            localList[roamingValue.Key] = roamingValue.Value;

            changeDetected = true;
        }

        return changeDetected;
    }
}
