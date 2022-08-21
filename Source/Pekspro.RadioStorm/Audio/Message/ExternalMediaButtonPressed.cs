namespace Pekspro.RadioStorm.Audio.Message;

public enum ExternalMediaButton
{
    Play,
    Pause,
    PlayPause,
    Forward,
    Backward,
    Next,
    Previous
}

public record ExternalMediaButtonPressed(ExternalMediaButton Button);
