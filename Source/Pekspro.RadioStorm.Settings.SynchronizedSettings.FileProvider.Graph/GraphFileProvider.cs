namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.FileProvider.Graph;

public sealed class GraphFileProvider : FileBaseProvider
{
    protected override string SyncType => "Graph";

    public IGraphHelper GraphHelper { get; }

    public GraphFileProvider(IGraphHelper graphHelper,
        ISettingsService settingsService,
        ILogger<GraphFileProvider> logger,
        IOptions<StorageLocations> storageLocationsOptions
        )
        : base("Graph", settingsService, logger, storageLocationsOptions)
    {
        GraphHelper = graphHelper;
    }

    public override bool IsReady => GraphHelper.State == ProviderState.SignedIn;

    public override bool IsSlow => true;

    private (DateTime CacheTime, Drive Drive, DriveItem DriveItem)? DriveCache { get; set; }

    private async Task<(Drive Drive, DriveItem AppRoot)> GetDriveItemAsync(GraphServiceClient graphClient)
    {
        if (DriveCache is null || DriveCache.Value.CacheTime < DateTime.UtcNow.AddSeconds(-5))
        {
            var driveItem = await graphClient.Me.Drive.GetAsync().ConfigureAwait(false);
            var appRootFolder = await graphClient.Drives[driveItem!.Id].Special["AppRoot"].GetAsync().ConfigureAwait(false);

            DriveCache = new (DateTime.UtcNow, driveItem!, appRootFolder!);
        }

        return (DriveCache.Value.Drive, DriveCache.Value.DriveItem);
    }

    public override async Task<Dictionary<string, FileOverview>> GetFilesAsync(HashSet<string> allowedLowerCaseFilename)
    {
        try
        {
            var client = await GraphHelper.GetClientAsync().ConfigureAwait(false);

            var driveData = await GetDriveItemAsync(client).ConfigureAwait(false);

            var filesInSubFolder = await client
                .Drives[driveData.Drive.Id]
                .Items[driveData.AppRoot.Id]
                .ItemWithPath(SyncPath)
                .Children
                .GetAsync()
                .ConfigureAwait(false);

            // Iterate thru all pages
            var allFiles = new List<DriveItem>();
            var filesIterator = PageIterator<DriveItem, DriveItemCollectionResponse>
                                    .CreatePageIterator
                                    (
                                        client,
                                        filesInSubFolder!,
                                        (file) => { allFiles.Add(file); return true; }
                                    );
            await filesIterator.IterateAsync().ConfigureAwait(false);

            var filesData = allFiles
                        .Where(a => allowedLowerCaseFilename.Contains(a.Name!.ToLower()))
                        .Select(a => new FileOverview
                        (
                            a.Name!.ToLower(),
                            exists: true,
                            checkSum: GetStoredChecksum(a.Name.ToLower()),
                            isModified: IsModified(a.Name.ToLower(), a.LastModifiedDateTime, a.Size),
                            lastModifiedDateTime: a.LastModifiedDateTime,
                            size: a.Size
                        ))
                        .ToDictionary(a => a.FileName.ToLower(), a => a);

            return filesData;
        }
        catch (ODataError ex) when (ex.ResponseStatusCode == (int) System.Net.HttpStatusCode.NotFound)
        {
            return new Dictionary<string, FileOverview>();
        }
        catch (Exception ex2)
        {
            Logger.LogError(ex2, "Failed to get files from Graph");
            
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            throw;
        }
    }

    public override async Task<FileOverview> FetchFileOverviewAsync(string filename)
    {
        var client = await GraphHelper.GetClientAsync().ConfigureAwait(false);

        try
        {
            var driveData = await GetDriveItemAsync(client).ConfigureAwait(false);

            var driveItem = await client
                .Drives[driveData.Drive.Id]
                .Items[driveData.AppRoot.Id]
                .ItemWithPath(Path.Combine(SyncPath, filename))
                .GetAsync()
                .ConfigureAwait(false);

            var fileData = new FileOverview(
                driveItem!.Name!.ToLower(),
                exists: true,
                checkSum: GetStoredChecksum(driveItem.Name.ToLower()),
                isModified: IsModified(driveItem.Name.ToLower(), driveItem.LastModifiedDateTime, driveItem.Size),
                lastModifiedDateTime: driveItem.LastModifiedDateTime,
                size: driveItem.Size
              );

            return fileData;
        }
        catch (ODataError ex) when (ex.ResponseStatusCode == (int)System.Net.HttpStatusCode.NotFound)
        {
            return new FileOverview(filename, exists: false);
        }
    }

    public override async Task<Stream> GetFileContentAsync(string fileName)
    {
        var client = await GraphHelper.GetClientAsync();

        var driveData = await GetDriveItemAsync(client).ConfigureAwait(false);

        var myFileContent = await client
                    .Drives[driveData.Drive.Id]
                    .Items[driveData.AppRoot.Id]
                    .ItemWithPath(Path.Combine(SyncPath, fileName))
                    .Content
                    .GetAsync()
                    .ConfigureAwait(false);

        return myFileContent!;
    }

    protected override async Task<(DateTimeOffset DateModified, long Size)> UploadFileAsync(string fileName, Stream stream)
    {
        var client = await GraphHelper.GetClientAsync();

        var driveData = await GetDriveItemAsync(client).ConfigureAwait(false);

        var driveItem = await client
                .Drives[driveData.Drive.Id]
                .Items[driveData.AppRoot.Id]
                .ItemWithPath(Path.Combine(SyncPath, fileName))
                .Content
                .PutAsync(stream);

        return (driveItem!.LastModifiedDateTime!.Value, driveItem.Size!.Value);
    }
}
