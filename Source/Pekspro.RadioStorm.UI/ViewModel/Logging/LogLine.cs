namespace Pekspro.RadioStorm.UI.ViewModel.Logging;

public sealed record LogLine
    (
        string DateTime,
        string Category,
        string Level,
        string Message
    );

