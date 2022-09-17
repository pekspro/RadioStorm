namespace Pekspro.RadioStorm.UI.ViewModel.Base;

[DebuggerDisplay("Header: {Header} Item count: {Count}")]
public sealed class Group<T> : ObservableCollection<T> where T : class
{
    public Group(string header, int priority, IEnumerable<T> items)
    {
        Header = header;
        Priority = priority;

        foreach (var i in items)
        {
            Add(i);
        }
    }

    public string Header { get; }
    
    public int Priority { get; }
}
