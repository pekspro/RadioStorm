namespace Pekspro.RadioStorm.UI.ViewModel.Logging;

public record LogLine
    (
        string DateTime,
        string Category,
        string Level,
        string Message
    );

