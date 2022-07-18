namespace Pekspro.RadioStorm.Audio.Message;

public record PlayerButtonStateChanged(bool CanPlay, bool CanPause, bool IsBuffering);
