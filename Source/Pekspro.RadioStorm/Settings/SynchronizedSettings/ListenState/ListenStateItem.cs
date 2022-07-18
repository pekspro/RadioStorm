namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.ListenState
{
    [DebuggerDisplay("Id: {Id} Length: {ListenLength} LastChanged: {LastChangedTimestamp}")]
    public sealed class ListenStateItem
    {
        public int Id { get; set; }
        public bool IsFullyListen { get; set; }
        public int ListenLength { get; set; }
        public uint LastChangedTimestamp { get; set; }
    }
}
