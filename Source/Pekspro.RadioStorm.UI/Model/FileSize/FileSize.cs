namespace Pekspro.RadioStorm.UI.Model.FileSize;

public sealed partial class FileSize : ObservableObject
{
    #region Constructor

    public FileSize()
    {

    }

    public FileSize(ulong sizeInBytes)
    {
        _SizeInBytes = sizeInBytes;
    }

    #endregion

    #region Properties

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SizeFormatted))]
    [NotifyPropertyChangedFor(nameof(SizeInMegaBytes))]
    private ulong _SizeInBytes;

    public double SizeInMegaBytes => SizeInBytes / 1024.0 / 1024;

    public string SizeFormatted => $"{SizeInMegaBytes:F1} MB";

    #endregion
}
