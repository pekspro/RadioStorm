namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.FileProvider.Graph;

public class GraphFileProvider : FileBaseProvider
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


#if NO_ANDROID_FIX
#else
    class Response<T>
    {
        [JsonPropertyName("value")]
        public T Value { get; set; } = default!;
    }
#endif

    public override async Task<Dictionary<string, FileOverview>> GetFilesAsync(HashSet<string> allowedLowerCaseFilename)
    {

        try
        {
            var client = await GraphHelper.GetClientAsync().ConfigureAwait(false);

#if NO_ANDROID_FIX
            var childrenPage = await client.Me.Drive.Special.AppRoot.ItemWithPath(SyncPath).Children
                .Request()
                .GetAsync();

            var children = childrenPage.ToList();
            int pagePos = 0;

            while (pagePos <= 10 && childrenPage.NextPageRequest is not null)
            {
                childrenPage = await childrenPage.NextPageRequest.GetAsync();
                children.AddRange(childrenPage.ToList());

                pagePos++;
            }
#else
            var response = await client.Me.Drive.Special.AppRoot.ItemWithPath(SyncPath).Children
                .Request()
                .GetResponseAsync();

            string text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var childrenPage = System.Text.Json.JsonSerializer.Deserialize<Response<List<DriveItem>>>(text)!;

            var children = childrenPage.Value.ToList();

            // Pagination will in weird unlikely cases be needed, but this is skipped for now
            //int pagePos = 0;

            //while (pagePos <= 10 && childrenPage.NextPageRequest is not null)
            //{
            //    childrenPage = await childrenPage.NextPageRequest.GetAsync();
            //    children.AddRange(childrenPage.ToList());

            //    pagePos++;
            //}

#endif

            var filesData = children
                        .Where(a => allowedLowerCaseFilename.Contains(a.Name.ToLower()))
                        .Select(a => new FileOverview
                        (
                            a.Name.ToLower(),
                            exists: true,
                            checkSum: GetStoredChecksum(a.Name.ToLower()),
                            isModified: IsModified(a.Name.ToLower(), a.LastModifiedDateTime, a.Size),
                            lastModifiedDateTime: a.LastModifiedDateTime,
                            size: a.Size
                        ))
                        .ToDictionary(a => a.FileName.ToLower(), a => a);

            return filesData;
        }
        catch (ServiceException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new Dictionary<string, FileOverview>();
        }
        catch(Exception ex2)
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
#if NO_ANDROID_FIX
            var driveItem = await client.Me.Drive.Special.AppRoot.ItemWithPath(Path.Combine(SyncPath, filename))
                .Request()
                .GetAsync()
                .ConfigureAwait(false);
#else
            var response = await client.Me.Drive.Special.AppRoot.ItemWithPath(Path.Combine(SyncPath, filename))
                .Request()
                .GetResponseAsync()
                .ConfigureAwait(false);

            string text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var driveItem = System.Text.Json.JsonSerializer.Deserialize<DriveItem>(text)!;

#endif

            var fileData = new FileOverview(
                driveItem.Name.ToLower(),
                exists: true,
                checkSum: GetStoredChecksum(driveItem.Name.ToLower()),
                isModified: IsModified(driveItem.Name.ToLower(), driveItem.LastModifiedDateTime, driveItem.Size),
                lastModifiedDateTime: driveItem.LastModifiedDateTime,
                size: driveItem.Size
              );

            return fileData;
        }
        catch (ServiceException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new FileOverview(filename, exists: false);
        }
    }

    public override async Task<Stream> GetFileContentAsync(string fileName)
    {
        var client = await GraphHelper.GetClientAsync();

        var driveItem = await client.Me.Drive.Special.AppRoot.ItemWithPath(Path.Combine(SyncPath, fileName)).Content
            .Request()
            .GetAsync()
            .ConfigureAwait(false);

        return driveItem;
    }

    protected override async Task<(DateTimeOffset DateModified, long Size)> UploadFileAsync(string fileName, Stream stream)
    {
        var client = await GraphHelper.GetClientAsync();

#if NO_ANDROID_FIX
        var driveItem = await client.Me.Drive.Special.AppRoot.ItemWithPath(Path.Combine(SyncPath, fileName)).Content
                .Request()
                .PutAsync<DriveItem>(stream);

        return (driveItem.LastModifiedDateTime!.Value, driveItem.Size!.Value);            
#else
        var response = await client.Me.Drive.Special.AppRoot.ItemWithPath(Path.Combine(SyncPath, fileName)).Content
                .Request()
                .PutResponseAsync<DriveItem>(stream);
        
        string text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        
        var driveItem = System.Text.Json.JsonSerializer.Deserialize<DriveItem>(text)!;
            
        return (driveItem.LastModifiedDateTime!.Value, driveItem.Size!.Value);
#endif
    }
}
