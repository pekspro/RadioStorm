namespace Pekspro.RadioStorm.UI.ViewModel.Base;

abstract public partial class ListViewModel<T> : DownloadViewModel where T : class
{
    #region Constructor
    
    public ListViewModel()
        : this(null!, null!)
    {

    }

    public ListViewModel(ILogger logger, IMainThreadRunner mainThreadRunner)
         : base(logger, mainThreadRunner)
    {

    }

    #endregion

    #region Properties

    public SelectionModeHelper SelectionModeHelper { get; } = new SelectionModeHelper();

    [ObservableProperty]
    private ObservableCollection<T>? _Items;

    [ObservableProperty]
    private ObservableCollection<Group<T>>? _GroupedItems = new();
    
    [ObservableProperty]
    private bool _IsSearchEnabled = true;

    #endregion

    #region Methods
    
    protected void ClearLists()
    {
        Items?.Clear();
        GroupedItems?.Clear();
    }

    protected void UpdateList(List<T> newItems)
    {
        if (newItems.Count == 0)
        {
            Items = new ObservableCollection<T>();
            GroupedItems = new ObservableCollection<Group<T>>();

            return;
        }

        newItems.Sort(new Comparison<T>(Compare));
        
        var items = Items;
        var groupedItems = GroupedItems;

        if (items is null || !items.Any() || Math.Abs(newItems.Count - items.Count) > 30)
        {
            items = new ObservableCollection<T>(newItems);

            var groupedData =
            (
                from f in items
                group f by new { Header = GetGroupName(f), Priority = GetGroupPriority(f) } into g
                orderby g.Key.Priority, g.Key.Header
                select new Group<T>(g.Key.Header, g.Key.Priority, g)
            ).ToList();

            groupedItems = new ObservableCollection<Group<T>>(groupedData);
        }
        else
        {
            items = items ?? new ObservableCollection<T>();
            groupedItems = groupedItems ?? new ObservableCollection<Group<T>>();

            // Delete old items not in new list
            var existingItems = items.Select(i => GetId(i)).ToHashSet();
            if (existingItems.Any())
            {
                var newItemsId = newItems.Select(i => GetId(i)).ToHashSet();

                foreach (var exitingItemId in existingItems)
                {
                    if (!newItemsId.Contains(exitingItemId))
                    {
                        Delete(exitingItemId, items, groupedItems);
                    }
                }
            }
        
            // Insert new items
            foreach (var item in newItems)
            {
                if (existingItems.Contains(GetId(item)))
                {
                    continue;
                }

                Insert(item, items, groupedItems);
            }
        }

        Items = items;
        GroupedItems = groupedItems;
    }

    protected bool Contains(T item) => ContainsId(GetId(item));

    protected bool ContainsId(int itemId)
    {
        if (Items is null)
        {
            return false;
        }

        return Items.Any(a => GetId(a) == itemId);
    }

    protected void Insert(T item)
    {
        if (Items is null)
        {
            Items = new ObservableCollection<T>();
        }

        if (GroupedItems is null)
        {
            GroupedItems = new ObservableCollection<Group<T>>();
        }

        if (Contains(item))
        {
            return;
        }

        Insert(item, Items, GroupedItems);
    }

    private void Insert(T item, ObservableCollection<T> items, ObservableCollection<Group<T>> groupedItems)
    {
        Insert(item, items);
        Insert(item, groupedItems);
    }

    private void Insert(T item, ObservableCollection<T> items)
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (Compare(item, items[i]) > 0)
            {
                items.Insert(i + 1, item);
                return;
            }
        }

        items.Insert(0, item);
    }

    private void Insert(T item, ObservableCollection<Group<T>> items)
    {
        string groupName = GetGroupName(item) ?? "?";
        int groupPriority = GetGroupPriority(item);
        
        for (int i = items.Count - 1; i >= 0; i--)
        {
            int groupCompare = CompareGroupName(groupName, groupPriority, items[i].Header, GetGroupPriority(items[i].First()));
            
            if (groupCompare > 0)
            {
                items.Insert(i + 1, new Group<T>(groupName, groupPriority, new List<T>() { item }));
                return;
            }
            else if (groupCompare == 0)
            {
                Insert(item, items[i]);
                return;
            }
        }

        items.Insert(0, new Group<T>(groupName, groupPriority, new List<T>() { item }));

        return;
    }

    protected void DeleteObselete(IEnumerable<int> itemIds)
    {
        if (Items is null || GroupedItems is null)
        {
            return;
        }

        for (int i = Items.Count - 1; i >= 0; i--)
        {
            var existingItemId = GetId(Items[i]);

            if (!itemIds.Any(a => a == existingItemId))
            {
                Delete(existingItemId);
            }
        }
    }

    protected void DeleteObselete(Func<T, bool> check)
    {
        if (Items is null || GroupedItems is null)
        {
            return;
        }

        for (int i = Items.Count - 1; i >= 0; i--)
        {
            if (check(Items[i]))
            {
                Delete(GetId(Items[i]));
            }
        }
    }

    protected void Delete(int itemId)
    {
        if (Items is null || GroupedItems is null)
        {
            return;
        }

        Delete(itemId, Items, GroupedItems);
    }
    
    private void Delete(int itemId, ObservableCollection<T> items, ObservableCollection<Group<T>> groupedItems)
    {
        if (Delete(itemId, items))
        {
            Delete(itemId, groupedItems);
        }
    }
    
    private bool Delete(int itemId, ObservableCollection<T> items)
    {
        for(int i = items.Count - 1; i >= 0; i--)
        {
            if (GetId(items[i]) == itemId)
            {
                items.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    private void Delete(int itemId, ObservableCollection<Group<T>> items)
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (Delete(itemId, items[i]))
            {
                if (!items[i].Any())
                {
                    items.RemoveAt(i);
                }
                
                return;
            }
        }
    }

    protected int CompareGroupName(string a, int priorityA, string b, int priorityB)
    {
        if (priorityA == priorityB)
        {
            return a.CompareTo(b);
        }

        return priorityA.CompareTo(priorityB);
    }

    abstract protected int GetId(T item);
    
    abstract protected int Compare(T a, T b);
    
    virtual protected int GetGroupPriority(T item) => 0;
    
    abstract protected string GetGroupName(T item);

    #endregion
}
