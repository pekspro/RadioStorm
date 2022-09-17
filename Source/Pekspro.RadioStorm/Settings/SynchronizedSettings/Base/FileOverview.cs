namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.Base;

public sealed class FileOverview
{
    public FileOverview
        (
            string fileName,
            bool exists = false,
            string checkSum = null!,
            bool isModified = false,
            DateTimeOffset? lastModifiedDateTime = null,
            long? size = null
        )
    {
        FileName = fileName;
        Exists = exists;
        Checksum = checkSum;
        IsModified = isModified;
        LastModifiedDateTime = lastModifiedDateTime;
        Size = size;
    }

    public string FileName { get; }

    public string Checksum { get; set; }

    public bool Exists { get; }

    public bool IsModified { get; }

    public DateTimeOffset? LastModifiedDateTime { get; }

    public long? Size { get; }
}
