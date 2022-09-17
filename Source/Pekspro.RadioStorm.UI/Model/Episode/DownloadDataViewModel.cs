namespace Pekspro.RadioStorm.UI.Model.Episode;

public sealed partial class DownloadDataViewModel : ObservableObject
{
    #region Constructor

    public DownloadDataViewModel()
        : this(SampleData.DownloadSample(0))
    {

    }

    public DownloadDataViewModel(Download download)
    {
        UpdateFromDownload(download);
    }

    #endregion

    #region Properties

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadedRatio))]
    private ulong _BytesToDownload;
        
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadedRatio))]
    private ulong _BytesDownloaded;

    public double DownloadedRatio
    {
        get
        {
            if (BytesToDownload == BytesDownloaded)
            {
                return 1;
            }

            if (BytesToDownload == 0)
            {
                return 0;
            }

            return (double)BytesDownloaded / BytesToDownload;
        }
    }

    [ObservableProperty]
    private DownloadDataStatus _Status;

    [ObservableProperty]
    private DateTimeOffset _DownloadTime;

    [ObservableProperty]
    private string _DownloadStatusText = string.Empty;

    [ObservableProperty]
    private string _DownloadSizeText = string.Empty;

    #endregion

    #region Methods
    
    public void UpdateFromDownload(Download download)
    {
        Status = download.Status;
        BytesToDownload = download.BytesToDownload;
        BytesDownloaded = download.BytesDownloaded;
        DownloadTime = download.DownloadTime;
        UpdateDownloadText();
    }

    private void UpdateDownloadText()
    {
        if (Status == DownloadDataStatus.Done)
        {
            DownloadStatusText = string.Format(Strings.DownloadStatus_Completed, BytesDownloaded / 1024.0 / 1024);
        }
        else if (Status == DownloadDataStatus.Paused || Status == DownloadDataStatus.Starting)
        {
            DownloadStatusText = Strings.DownloadStatus_Waiting;
        }
        else if (Status == DownloadDataStatus.Downloading)
        {
            if (BytesToDownload == 0)
            {
                DownloadStatusText = Strings.DownloadStatus_Downloading;
            }
            else
            {
                DownloadStatusText = string.Format(Strings.DownloadStatus_Downloading_InProgress, (int)(DownloadedRatio * 100 + 0.5), BytesToDownload / 1024.0 / 1024);
            }
        }
        else
        {
            DownloadStatusText = Strings.DownloadStatus_Error;
        }

        DownloadSizeText = string.Format("{0:F1} MB", BytesDownloaded / 1024.0 / 1024);
    }

    #endregion
}
