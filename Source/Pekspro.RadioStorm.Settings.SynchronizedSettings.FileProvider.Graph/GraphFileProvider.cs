﻿namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.FileProvider.Graph;

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

    private (DateTime CacheTime, string DriveId, DriveItem DriveItem)? DriveCache { get; set; }

    private async Task<(string DriveId, DriveItem AppRoot)> GetDriveItemAsync(GraphServiceClient graphClient)
    {
        if (DriveCache is null || DriveCache.Value.CacheTime < DateTime.UtcNow.AddSeconds(-5))
        {
            string driveId;

            try
            {
                var driveItem = await graphClient.Me.Drive.GetAsync().ConfigureAwait(false);
                driveId = driveItem!.Id!;
            }
            catch (ODataError ex) when (ex.ResponseStatusCode == 403)
            {
                // https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/2624#issuecomment-2532361401
                // Is this a real problem? Is below a solution?

                var result = await graphClient.Me.Drive
                    .WithUrl($"{graphClient.RequestAdapter.BaseUrl}/drive/special/approot")
                    .GetAsync();

                driveId = result!.Id!.Split("!")[0];
            }

            var appRootFolder = await graphClient.Drives[driveId].Special["AppRoot"].GetAsync().ConfigureAwait(false);

            DriveCache = new (DateTime.UtcNow, driveId, appRootFolder!);
        }

        return (DriveCache.Value.DriveId, DriveCache.Value.DriveItem);
    }

    public override async Task<Dictionary<string, FileOverview>> GetFilesAsync(HashSet<string> allowedLowerCaseFilename)
    {
        try
        {
            var client = await GraphHelper.GetClientAsync().ConfigureAwait(false);

            var driveData = await GetDriveItemAsync(client).ConfigureAwait(false);

            var filesInSubFolder = await client
                .Drives[driveData.DriveId]
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
                .Drives[driveData.DriveId]
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
                    .Drives[driveData.DriveId]
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
                .Drives[driveData.DriveId]
                .Items[driveData.AppRoot.Id]
                .ItemWithPath(Path.Combine(SyncPath, fileName))
                .Content
                .PutAsync(stream);

        return (driveItem!.LastModifiedDateTime!.Value, driveItem.Size!.Value);
    }
}
