namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.ListenState;

public class ListenStateManager : SharedSettingsListBase<ListenStateItem>, IListenStateManager
{
    const ulong FileHeader = 0x7789512414351a1e;
    const int FileSlotCount = 8;
    const string FileNameFormat = "listenstate_{0}.dat";

    private readonly TimeSpan MaxSaveInteral = TimeSpan.FromSeconds(60);
    private readonly TimeSpan SaveDelay = TimeSpan.FromSeconds(2);
    private DateTime RequestSaveTime = DateTime.MinValue;




    // public event EventHandler ListenStateManagerUpdated;

    // private DispatcherTimer Timer = null;
    private bool[] DirtySlots = new bool[FileSlotCount];

    public IMessenger Messenger { get; }
    public IDateTimeProvider DateTimeProvider { get; }

    public override string GetFileName(int slotId) =>
        string.Format(FileNameFormat, slotId);

    public ListenStateManager
        (
            IMessenger messenger,
            IDateTimeProvider dateTimeProvider,
            ILogger<ListenStateManager> logger, 
            IOptions<StorageLocations> storageLocationOptions
        )
        : base(logger, storageLocationOptions, FileSlotCount)
    {
        Messenger = messenger;
        DateTimeProvider = dateTimeProvider;
    }

    public void Init(bool allowBackgroundSaving)
    {
        if (allowBackgroundSaving)
        {
            //Timer = new DispatcherTimer();
            TimerInterval = SaveDelay.TotalMilliseconds;
            //Timer.Tick += async (s, e) => await SaveDirtySlotsAsync();
        }
    }


    public bool SetFullyListen(int id, bool isFullyListen)
    {
        var c = Items.FirstOrDefault(o => o.Value.Id == id);
        ListenStateItem current = c.Value;

        if (current is null)
        {
            ListenStateItem favorite = new ListenStateItem()
            {
                Id = id,
                IsFullyListen = isFullyListen,
                LastChangedTimestamp = TimestampHelper.NowToInt()
            };

            Items.Add(id, favorite);

            SaveLater(id % FileSlotCount);

            // ListenStateManagerUpdated?.Invoke(this, null);
            Messenger.Send(new ListenStateChangedMessage(id, isFullyListen));

            return true;
        }
        else
        {
            if (isFullyListen != current.IsFullyListen)
            {
                current.IsFullyListen = isFullyListen;
                current.LastChangedTimestamp = TimestampHelper.NowToInt();

                SaveLater(id % FileSlotCount);

                // ListenStateManagerUpdated?.Invoke(this, null);
                Messenger.Send(new ListenStateChangedMessage(id, isFullyListen));

                return true;
            }

            return false;
        }
    }

    public bool SetListenLength(int id, int length)
    {
        var c = Items.FirstOrDefault(o => o.Value.Id == id);
        ListenStateItem current = c.Value;

        if (current is null)
        {
            ListenStateItem favorite = new ListenStateItem()
            {
                Id = id,
                ListenLength = length,
                LastChangedTimestamp = TimestampHelper.NowToInt()
            };

            Items.Add(id, favorite);

            SaveLater(id % FileSlotCount);

            return true;
        }
        else
        {
            if (length != current.ListenLength)
            {
                current.ListenLength = length;
                current.LastChangedTimestamp = TimestampHelper.NowToInt();

                SaveLater(id % FileSlotCount);

                return true;
            }

            return false;
        }
    }

    public bool IsFullyListen(int id)
    {
        if (Items.ContainsKey(id))
        {
            return Items[id].IsFullyListen;
        }

        return false;
    }

    public int GetListenLength(int id)
    {
        if (Items.ContainsKey(id))
        {
            return Items[id].ListenLength;
        }

        return 0;
    }


    private void SaveLater(int slotId)
    {
        DirtySlots[slotId] = true;

        if (RequestSaveTime != DateTime.MinValue)
        {
            if (DateTimeProvider.UtcNow - RequestSaveTime >= MaxSaveInteral)
            {
                TimerInterval = TimeSpan.FromMilliseconds(50).TotalMilliseconds;
            }
        }
        else
        {
            RequestSaveTime = DateTimeProvider.UtcNow;
            TimerInterval = SaveDelay.TotalMilliseconds;
        }

        RestartTimer();
    }

    public override async Task SaveIfDirtyAsync()
    {
        Logger.LogInformation("It's time to save dirty slots.");
        Stopwatch watch = new Stopwatch();
        // Timer?.Stop();
        int saveCount = 0;

        for (int i = 0; i < FileSlotCount; i++)
        {
            if (DirtySlots[i])
            {
                Logger.LogInformation($"Saving dirty slot {i}.");
                saveCount++;
                await SaveAsync(i);

                // _ = SharedSettingsManager.UpdateRemoteFileIfNewerAsync(GetFileName(i));
                Messenger.Send(new LocalSharedFileUpdated(GetFileName(i)));
            }
        }

        if (saveCount > 0)
        {
            Logger.LogInformation($"Saved {saveCount} files in {watch.ElapsedMilliseconds} ms.");
        }

        RequestSaveTime = DateTime.MinValue;
    }

    protected override Task SaveAsync(int slotId, ReverseBinaryWriter writer)
    {
        var data = Items.Where(a => a.Value.Id % FileSlotCount == slotId).Select(a => a.Value).ToList();

        writer.WriteUInt64(FileHeader);
        writer.WriteInt32(data.Count);

        foreach (var item in data)
        {
            writer.WriteInt32(item.Id);
            writer.WriteUInt32(item.LastChangedTimestamp);

            writer.WriteBoolean(item.IsFullyListen);
            writer.WriteInt32(item.ListenLength);
        }

        DirtySlots[slotId] = false;

        return Task.CompletedTask;
    }

    protected override Task<Dictionary<int, ListenStateItem>> ReadAsync(ReverseBinaryReader reader)
    {
        Dictionary<int, ListenStateItem> result = new Dictionary<int, ListenStateItem>();

        ulong header = reader.ReadUInt64();

        if (header != FileHeader)
        {
            throw new Exception("File doesn't seem to be a listen state file.");
        }
        else
        {
            int count = reader.ReadInt32();

            while (count > 0)
            {
                int id = reader.ReadInt32();
                uint timeStamp = reader.ReadUInt32();
                bool isFullyListened = reader.ReadBoolean();
                int listenLength = reader.ReadInt32();

                if (result.ContainsKey(id))
                {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                }
                else
                {
                    result.Add(id, new ListenStateItem()
                    {
                        Id = id,
                        IsFullyListen = isFullyListened,
                        ListenLength = listenLength,
                        LastChangedTimestamp = timeStamp
                    });
                }

                count--;
            }
        }

        return Task.FromResult(result);
    }

    protected override async Task<bool> MergeAsync(Dictionary<int, ListenStateItem> roamingList)
    {
        Logger.LogInformation($"Has {Items.Count} items locally. Got {roamingList.Count} from remote.");

        bool changed = Merge(Items, roamingList);

        if (changed)
        {
            Logger.LogInformation($"Local list was modified during merge. Has now {Items.Count} items.");

            await SaveIfDirtyAsync();

            // ListenStateManagerUpdated?.Invoke(this, null);
            Messenger.Send(new ListenStateChangedMessage(null, false));
        }
        else
        {
            Logger.LogInformation($"No changed in local list after merge.");
        }

        return changed;
    }


    internal void Merge(ListenStateItem backgroundSavedItem)
    {
        var c = Items.FirstOrDefault(o => o.Value.Id == backgroundSavedItem.Id);
        ListenStateItem current = c.Value;

        if (current is null)
        {
            ListenStateItem favorite = backgroundSavedItem;

            Items.Add(backgroundSavedItem.Id, favorite);

            SaveLater(backgroundSavedItem.Id % FileSlotCount);

            return;
        }
        else
        {
            if (backgroundSavedItem.LastChangedTimestamp > current.LastChangedTimestamp)
            {
                current.LastChangedTimestamp = backgroundSavedItem.LastChangedTimestamp;
                current.ListenLength = backgroundSavedItem.ListenLength;

                if (backgroundSavedItem.IsFullyListen)
                {
                    current.IsFullyListen = true;
                }

                SaveLater(current.Id % FileSlotCount);
            }
        }
    }

    private bool Merge(Dictionary<int, ListenStateItem> localList, Dictionary<int, ListenStateItem> roamingList)
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
                    Logger.LogDebug($"Listen state item {roamingValue.Id} updated from roaming settings.");
                    localList[key] = roamingValue;

                    changeDetected = true;
                    DirtySlots[key % FileSlotCount] = true;
                }

                roamingList.Remove(key);
            }
        }

        foreach (var roamingValue in roamingList)
        {
            Logger.LogDebug($"Adding listen state item {roamingValue.Value.Id} added from roaming settings.");
            localList[roamingValue.Key] = roamingValue.Value;

            changeDetected = true;
            DirtySlots[roamingValue.Key % FileSlotCount] = true;
        }

        return changeDetected;
    }
}

