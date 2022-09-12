namespace Pekspro.RadioStorm.Audio.Message;

public sealed record MediaPositionChanged(TimeSpan Position, TimeSpan Length);
