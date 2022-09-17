namespace Pekspro.RadioStorm.MAUI.Services;

using Microsoft.Extensions.Logging;

public sealed class AndroidLoggerProvider : ILoggerProvider
{
    public AndroidLoggerProvider()
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        // Category name is often the full sealed class name, like
        // MyApp.ViewModel.MyViewModel
        // This removes the namespace:
        int lastDotPos = categoryName.LastIndexOf('.');
        if (lastDotPos > 0)
        {
            categoryName = categoryName.Substring(lastDotPos + 1);
        }
        
        return new AndroidLogger(categoryName);
    }

    public void Dispose() { }
}

public sealed class AndroidLogger : ILogger
{
    private readonly string Category;

    public IDisposable BeginScope<TState>(TState state) /* Add in .NET7: where TState : notnull */ => null!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public AndroidLogger(string category)
    {
        Category = category;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        string message = formatter(state, exception);

        Java.Lang.Throwable? throwable = null!;

        if (exception is not null)
        {
            throwable = Java.Lang.Throwable.FromException(exception);
        }

        switch (logLevel)
        {
            case LogLevel.Trace:
                Android.Util.Log.Verbose(Category, throwable, message);
                break;

            case LogLevel.Debug:
                Android.Util.Log.Debug(Category, throwable, message);
                break;

            case LogLevel.Information:
                Android.Util.Log.Info(Category, throwable, message);
                break;

            case LogLevel.Warning:
                Android.Util.Log.Warn(Category, throwable, message);
                break;

            case LogLevel.Error:
                Android.Util.Log.Error(Category, throwable, message);
                break;

            case LogLevel.Critical:
                Android.Util.Log.Wtf(Category, throwable, message);
                break;
        }
    }
}
