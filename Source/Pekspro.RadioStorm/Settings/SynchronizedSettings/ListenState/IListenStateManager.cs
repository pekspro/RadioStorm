namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.ListenState
{
    public interface IListenStateManager : ISharedSettingsListBase<ListenStateItem>
    {
        int GetListenLength(int id);
        void Init(bool allowBackgroundSaving);
        bool IsFullyListen(int id);
        Task SaveIfDirtyAsync();
        bool SetFullyListen(int id, bool isFullyListen);
        bool SetListenLength(int id, int length);
    }
}