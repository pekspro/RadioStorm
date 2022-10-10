namespace Pekspro.RadioStorm.Audio;

public interface IAudioManager
{
    bool IsBuffering { get; }

    bool CanPlay { get; }

    bool CanPause { get; }

    bool CanPlayPause { get; }

    PlayListItem? CurrentItem { get; }

    PlayList? CurrentPlayList { get; }

    TimeSpan Position { get; }

    TimeSpan MediaLength { get; }

    int ProgressValue { get; set; }

    int ProgressMaxValue { get; }

    double Volume { get; set; }
    
    int PlaybackRateIndex { get; set; }

    double PlaybackRate { get; }

    bool IsSleepTimerEnabled { get; }

    DateTime SleepActivationTime { get; }

    TimeSpan TimeLeftToSleepActivation { get; }

    double SleepModeVolumeMultiplier { get; }

    double? BufferRatio { get; set; }

    void Play(PlayListItem item);
    
    void Play(PlayListItem[] items);
    
    void Play(PlayListItem[] items, Guid playListId, int activeIndex);
    
    void Add(PlayListItem item);

    void Add(PlayListItem[] items);

    bool IsItemIdActive(bool isLiveAudio, int itemId);

    bool IsItemIdInPlayList(bool isLiveAudio, int itemId);

    void RemoveItemFromPlaylist(int itemId);

    void RemoveItemsFromPlaylist(IEnumerable<int> itemId);

    void MovePlaylistItem(int fromIndexId, int toIndexId);

    void SetPlaylistPosition(int pos);

    void SetPlaybackPosition(TimeSpan position);

    void Play();

    void Pause();

    void PlayPause();

    void Move(TimeSpan delta);

    bool GoToNext();

    bool GoToPrevious();

    void StartSleepTimer();
    
    void StartSleepTimer(TimeSpan timeLeftToSleepActivation);

    void StopSleepTimer();

    void IncreaseSleepTimer();

    void DecreaseSleepTimer();
    
    void IncreaseSleepTimer(TimeSpan timeLeftToSleepActivationDelta);
}
