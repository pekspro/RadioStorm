namespace Pekspro.RadioStorm.Settings;

public sealed class DownloadSettings : IDownloadSettings
{
    private const string SettingsName = "AutomaticDownloadSettings";

    public ISettingsService SettingsService { get; }

    private List<DownloadSetting> Settings { get; set; } = new List<DownloadSetting>();

    public DownloadSettings(ISettingsService settingsService)
    {
        SettingsService = settingsService;
    }

    public void Init()
    {
        Read();
    }

    private void Read()
    {
        try
        {
            Settings.Clear();

            string? s = SettingsService.GetSafeValue<string?>(SettingsName, null);

            if (s is not null)
            {
                var d = System.Text.Json.JsonSerializer.Deserialize<List<DownloadSetting>>(s);
                Settings = d ?? Settings;
            }
        }
        catch (Exception)
        {

        }
    }

    private void Save()
    {
        var s = System.Text.Json.JsonSerializer.Serialize(Settings);

        SettingsService.SetValue(SettingsName, s);
    }

    public DownloadSetting? GetSettings(int programId)
    {
        return Settings.FirstOrDefault(a => a.ProgramId == programId);
    }

    public void UpdateSettings(int programId, int downloadCount)
    {
        var current = Settings.FirstOrDefault(a => a.ProgramId == programId);
        if (current is null)
        {
            current = new DownloadSetting();
            Settings.Add(current);
        }

        current.ProgramId = programId;
        current.DownloadCount = downloadCount;

        Save();
    }

    public IList<DownloadSetting> GetProgramsWithDownloadSetting()
    {
        return Settings.Where(a => a.DownloadCount > 0).ToList();
    }
}
