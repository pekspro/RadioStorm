namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.Base
{
    public interface IFileProvider
    {
        string Name { get; }
        bool IsReady { get; }
        bool IsSlow { get; }

        Task<FileOverview> FetchFileOverviewAsync(string filename);
        string GetCheckSum(Stream instream);
        Task<Stream> GetFileContentAsync(string fileName);
        Task<Dictionary<string, FileOverview>> GetFilesAsync(HashSet<string> allowedLowerCaseFilename);
        Task UpdateFileAsync(string fileName, FileOverview? fileData);
        Task<bool> UpdateFileIfNewerAsync(string fileName);
    }
}