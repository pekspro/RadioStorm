namespace Pekspro.RadioStorm.Audio;

public static class AudioUrlHelper
{
    #region Methods
    
    public static string? GetDownloadUrl(string? audioStreamWithMusicUrl, string? audioStreamWithoutMusicUrl, string? audioDownloadUrl)
    {
        if (!string.IsNullOrWhiteSpace(audioDownloadUrl))
        {
            return audioDownloadUrl;
        }

        if (!string.IsNullOrWhiteSpace(audioStreamWithMusicUrl))
        {
            return audioStreamWithMusicUrl;
        }

        if (!string.IsNullOrWhiteSpace(audioStreamWithoutMusicUrl))
        {
            return audioStreamWithoutMusicUrl;
        }

        return null;
    }

    #endregion
}
