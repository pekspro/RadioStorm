namespace Pekspro.RadioStorm.Settings;

public interface IDownloadSettings
{
    IList<DownloadSetting> GetProgramsWithDownloadSetting();
    DownloadSetting? GetSettings(int programId);
    void Init();
    void UpdateSettings(int programId, int downloadCount);
}