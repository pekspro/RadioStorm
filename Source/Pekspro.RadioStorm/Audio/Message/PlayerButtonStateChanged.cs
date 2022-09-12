namespace Pekspro.RadioStorm.Audio.Message;

public sealed record PlayerButtonStateChanged(bool CanPlay, bool CanPause, bool IsBuffering);
