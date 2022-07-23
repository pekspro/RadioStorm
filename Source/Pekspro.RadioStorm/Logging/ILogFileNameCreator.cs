namespace Pekspro.RadioStorm.Logging;

internal interface ILogFileNameCreator
{
    string CreateLogFilName(string prefix);
}