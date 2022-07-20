namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.Base;

public class FileBaseProviderAndFiles
{
    public FileBaseProviderAndFiles(IFileProvider provider, Dictionary<string, FileOverview> files)
    {
        Provider = provider;
        Files = files;
    }

    public IFileProvider Provider { get; set; }

    public Dictionary<string, FileOverview> Files { get; set; }
}
