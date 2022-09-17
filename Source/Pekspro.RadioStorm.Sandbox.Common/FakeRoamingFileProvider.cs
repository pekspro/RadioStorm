using Microsoft.Extensions.Options;
using Pekspro.RadioStorm.Options;
using Pekspro.RadioStorm.Sandbox.Common.Options;
using Pekspro.RadioStorm.Settings;
using Pekspro.RadioStorm.Settings.SynchronizedSettings.Base;

namespace Pekspro.RadioStorm.Sandbox.Common;

public sealed class FakeRoamingFileProvider2 : FakeRoamingFileProviderBase
{
    protected override string SyncType => "FakeRoaming2";

    public FakeRoamingFileProvider2(ISettingsService settingsService,
        ILogger<FakeRoamingFileProvider2> logger,
        IOptions<StorageLocations> storageLocationsOptions,
        IOptions<FakeStorageLocations> fakeStorageLocationsOptions
        )
        : base(settingsService, logger, storageLocationsOptions, fakeStorageLocationsOptions, "FakeRoaming2", "Fake Roaming2")
    {
    }
}

public sealed class FakeRoamingFileProvider1 : FakeRoamingFileProviderBase
{
    protected override string SyncType => "FakeRoaming1";

    public FakeRoamingFileProvider1(ISettingsService settingsService,
        ILogger<FakeRoamingFileProvider1> logger,
        IOptions<StorageLocations> storageLocationsOptions,
        IOptions<FakeStorageLocations> fakeStorageLocationsOptions
        )
        : base(settingsService, logger, storageLocationsOptions, fakeStorageLocationsOptions, "FakeRoaming1", "Fake Roaming1")
    {
    }
}

public abstract class FakeRoamingFileProviderBase : FileBaseProvider
{
    private readonly string Folder;

    public FakeRoamingFileProviderBase(ISettingsService settingsService,
        ILogger logger,
        IOptions<StorageLocations> storageLocationsOptions,
        IOptions<FakeStorageLocations> fakeStorageLocationsOptions,
        string folderName,
        string name
        )
        : base(name, settingsService, logger, storageLocationsOptions)
    {
        Folder = Path.Combine(fakeStorageLocationsOptions.Value.FakeStorageBaseDirectory ?? storageLocationsOptions.Value.BaseStoragePath, folderName);

        Directory.CreateDirectory(Folder);
    }

    public override async Task<Dictionary<string, FileOverview>> GetFilesAsync(HashSet<string> allowedLowerCaseFilename)
    {
        var list = new Dictionary<string, FileOverview>();

        await Task.Run(() =>
        {
            var files = Directory.GetFiles(Folder);

            foreach (var file in files)
            {
                string lowerCaseName = Path.GetFileName(file.ToLower());
                if (!allowedLowerCaseFilename.Contains(lowerCaseName))
                {
                    continue;
                }

                FileInfo prop = new FileInfo(file);

                FileOverview roamingFileData = new FileOverview
                (
                    lowerCaseName,
                    exists: true,
                    checkSum: GetStoredChecksum(lowerCaseName),
                    isModified: IsModified(lowerCaseName, prop.LastWriteTimeUtc, prop.Length),
                    lastModifiedDateTime: new DateTimeOffset(prop.LastWriteTimeUtc),
                    size: prop.Length
                );

                list.Add(lowerCaseName, roamingFileData);
            }
        });

        return list;
    }

    public override async Task<FileOverview> FetchFileOverviewAsync(string filename)
    {
        try
        {
            var filePath = Path.Combine(Folder, filename);
            string lowerCaseName = filename.ToLower();

            FileInfo prop = null!;

            await Task.Run(() =>
            {
                prop = new FileInfo(filePath);
            });

            FileOverview roamingFileData = new FileOverview
                (
                    lowerCaseName,
                    exists: true,
                    checkSum: GetStoredChecksum(lowerCaseName),
                    isModified: IsModified(lowerCaseName, prop.LastWriteTimeUtc, prop.Length),
                    lastModifiedDateTime: new DateTimeOffset(prop.LastWriteTimeUtc),
                    size: prop.Length
                );

            return roamingFileData;
        }
        catch (FileNotFoundException)
        {
            return new FileOverview(filename, exists: false);
        }
    }

    public override Task<Stream> GetFileContentAsync(string fileName)
    {
        var filePath = Path.Combine(Folder, fileName);

        var stream = File.OpenRead(filePath);

        return Task.FromResult((Stream)stream);
    }

    protected override async Task<(DateTimeOffset DateModified, long Size)> UploadFileAsync(string fileName, Stream stream)
    {
        var filePath = Path.Combine(Folder, fileName);

        using (var outputStream = File.Create(filePath))
        {
            await stream.CopyToAsync(outputStream);
        }

        FileInfo prop = new FileInfo(filePath);
        {
            return (prop.LastWriteTimeUtc, prop.Length);
        }
    }
}
