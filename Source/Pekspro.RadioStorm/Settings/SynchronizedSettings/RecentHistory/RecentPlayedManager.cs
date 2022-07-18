namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.RecentHistory
{
    [DebuggerDisplay("Id: {Id} Episode: {IsEpisode} LastChanged: {Timestamp}")]
    public sealed class RecentPlayedItem : IComparable<RecentPlayedItem>
    {
        public bool IsRemoved { get; set; }
        public int Id { get; set; }
        public bool IsEpisode { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public int CompareTo(RecentPlayedItem? other)
        {
            if (other is null)
            {
                return 0;
            }

            return -Timestamp.CompareTo(other.Timestamp);
        }
    }


    public class RecentPlayedManager : SharedSettingsListBase<RecentPlayedItem>, IRecentPlayedManager
    {
        const ulong FileHeader = 0x139d512414351a1e;
        const string FileName = "recenthistory.dat";
        const int MaxItemCount = 30;

        private readonly TimeSpan MaxSaveInteral = TimeSpan.FromSeconds(60);
        private readonly TimeSpan SaveDelay = TimeSpan.FromSeconds(2);
        private DateTime RequestSaveTime = DateTime.MinValue;

        // public event EventHandler RecentPlayedManagerUpdated;
        // private DispatcherTimer Timer = null;
        private bool IsDirty = false;

        public override string GetFileName(int slotId) => FileName;

        public RecentPlayedManager(IMessenger messenger, ILogger<RecentPlayedManager> logger, 
            IDateTimeProvider dateTimeProvider,
            IOptions<StorageLocations> storageLocationOptions)
            : base(logger, storageLocationOptions, 1)
        {
            Messenger = messenger;
            DateTimeProvider = dateTimeProvider;
        }

        public void Init(bool allowBackgroundSaving)
        {
            if (allowBackgroundSaving)
            {
                TimerInterval = SaveDelay.TotalMilliseconds;

                /* Timer = new DispatcherTimer();
				
				Timer.Tick += async (s, e) => await SaveIfDirtyAsync(); */
            }
        }

        public override DateTime LatestChangedTime
        {
            get
            {
                if (Items.Any())
                    return Items.Values.Max(a => a.Timestamp).LocalDateTime;

                return DateTimeProvider.LocalNow;
            }
        }

        public IMessenger Messenger { get; }
        public IDateTimeProvider DateTimeProvider { get; }

        private void RemoveOldItems()
        {
            RemoveOldItems(Items);
        }

        private void RemoveOldItems(Dictionary<int, RecentPlayedItem> list)
        {
            if (list.Count > MaxItemCount)
            {
                var oldestItems = list.OrderByDescending(a => a.Value.Timestamp).Skip(MaxItemCount);

                foreach (var oldItem in oldestItems)
                {
                    list.Remove(oldItem.Key);
                }
            }
        }

        public bool AddOrUpdate(bool isEpisode, int id)
        {
            return AddOrUpdate(isEpisode, id, DateTimeProvider.OffsetNow);
        }

        public bool AddOrUpdate(bool isEpisode, int id, DateTimeOffset updateTime)
        {
            if (!isEpisode)
            {
                return false;
            }

            int key = GetKey(isEpisode, id);

            if (!Items.TryGetValue(key, out RecentPlayedItem? current))
            {
                RecentPlayedItem newItem = new RecentPlayedItem()
                {
                    Id = id,
                    IsEpisode = isEpisode,
                    IsRemoved = false,
                    Timestamp = updateTime
                };

                Items.Add(key, newItem);
                RemoveOldItems();

                SaveLater();

                Messenger.Send(new RecentListChangedMessage(id, true));

                return true;
            }
            else
            {
                if (current.IsRemoved || current.Timestamp != updateTime)
                {
                    current.IsRemoved = false;
                    current.Timestamp = DateTimeProvider.UtcNow;
                    RemoveOldItems();

                    SaveLater();

                    Messenger.Send(new RecentListChangedMessage(id, true));

                    return true;
                }

                return false;
            }
        }

        private static int GetKey(bool isEpisode, int id)
        {
            return isEpisode ? id : -id;
        }

        public void Clear()
        {
            bool changed = false;
            foreach (var c in Items.Values)
            {
                if (!c.IsRemoved)
                {
                    c.IsRemoved = true;
                    c.Timestamp = DateTimeProvider.OffsetNow;
                    changed = true;
                }
            }

            if (changed)
                SaveLater();
        }

        public void Remove(bool isEpisode, int id)
        {
            int key = GetKey(isEpisode, id);

            if (Items.TryGetValue(key, out var item) && !item.IsRemoved)
            {
                item.IsRemoved = true;
                item.Timestamp = DateTimeProvider.OffsetNow;
                SaveLater();

                Messenger.Send(new RecentListChangedMessage(id, false));
            }
        }

        private void SaveLater()
        {
            IsDirty = true;

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
            if (IsDirty)
            {
                Debug.WriteLine("It's time to save " + FileName);
                Stopwatch watch = new Stopwatch();
                // Timer?.Stop();
                await SaveAsync(0);

                // _ = SharedSettingsManager.UpdateRemoteFileIfNewerAsync(FileName);
                Messenger.Send(new LocalSharedFileUpdated(FileName));

                Debug.WriteLine($"Saved {FileName} in {watch.ElapsedMilliseconds} ms.");
            }
        }

        protected override Task SaveAsync(int slotId, ReverseBinaryWriter writer)
        {
            var data = Items.Values;

            writer.WriteUInt64(FileHeader);
            writer.WriteInt32(data.Count);

            foreach (var item in data)
            {
                writer.WriteInt32(item.Id);
                writer.WriteBoolean(item.IsEpisode);
                writer.WriteBoolean(item.IsRemoved);
                writer.WriteDateTime(item.Timestamp);
            }

            IsDirty = false;
            RequestSaveTime = DateTime.MinValue;

            return Task.CompletedTask;
        }

        protected override Task<Dictionary<int, RecentPlayedItem>> ReadAsync(ReverseBinaryReader reader)
        {
            Dictionary<int, RecentPlayedItem> result = new Dictionary<int, RecentPlayedItem>();

            ulong header = reader.ReadUInt64();

            if (header != FileHeader)
            {
                throw new Exception("File " + FileName + " doesn't seem to be a recent played file");
            }
            else
            {
                int count = reader.ReadInt32();

                while (count > 0)
                {
                    int id = reader.ReadInt32();
                    bool isEpisode = reader.ReadBoolean();
                    bool isRemoved = reader.ReadBoolean();
                    DateTimeOffset timeStamp = reader.ReadDateTime();
                    int key = GetKey(isEpisode, id);

                    if (result.ContainsKey(key))
                    {
                        if (Debugger.IsAttached)
                            Debugger.Break();
                    }
                    else
                    {
                        result.Add(key, new RecentPlayedItem()
                        {
                            Id = id,
                            IsEpisode = isEpisode,
                            IsRemoved = isRemoved,
                            Timestamp = timeStamp
                        });
                    }

                    count--;
                }
            }

            RemoveOldItems(result);

            return Task.FromResult(result);
        }

        protected override async Task<bool> MergeAsync(Dictionary<int, RecentPlayedItem> roamingList)
        {
            Logger.LogInformation($"Has {Items.Count} items locally. Got {roamingList.Count} from remote.");

            bool changed = Merge(Items, roamingList);

            if (changed)
            {
                Logger.LogInformation($"Local list was modified during merge. Has now {Items.Count} items.");

                await SaveAsync(0);

                Messenger.Send(new RecentListChangedMessage(null, true));
            }
            else
            {
                Logger.LogInformation($"No changed in local list after merge.");
            }

            return changed;
        }


        private bool Merge(Dictionary<int, RecentPlayedItem> localList, Dictionary<int, RecentPlayedItem> roamingList)
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
                    if (roamingValue.Timestamp > localValue.Timestamp)
                    {
                        Debug.WriteLine($"Recent played item {roamingValue.Id} updated from roaming settings.");
                        localList[key] = roamingValue;

                        changeDetected = true;
                    }

                    roamingList.Remove(key);
                }
            }

            foreach (var roamingValue in roamingList)
            {
                Debug.WriteLine($"Adding recent played item {roamingValue.Value.Id} added from roaming settings.");
                localList[roamingValue.Key] = roamingValue.Value;

                changeDetected = true;
            }

            RemoveOldItems(localList);

            return changeDetected;
        }
    }
}
