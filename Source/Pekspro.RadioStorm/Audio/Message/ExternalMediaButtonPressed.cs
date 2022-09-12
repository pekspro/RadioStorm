namespace Pekspro.RadioStorm.Audio.Message;

public enum ExternalMediaButton
{
    Play,
    Pause,
    PlayPause,
    Forward,
    Rewind,
    Next,
    Previous
}

public sealed record ExternalMediaButtonPressed(ExternalMediaButton Button);
