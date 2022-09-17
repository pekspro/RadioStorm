namespace Pekspro.RadioStorm.UI.Model.ImageLink;

public sealed class ImageLink : ObservableObject
{
    #region Private properites

    private const string DefaultUrl = null!; // ""; // "ms-appx:///Assets/transparent.png";
    
    private readonly string? PrimaryLinkHighRes;
    private readonly string? PrimaryLinkLowRes;

    private readonly string? SecondaryLinkHighRes;
    private readonly string? SecondaryLinkLowRes;

    #endregion

    #region Constructor

    public ImageLink(string? primaryHighRes, string? primaryLowRes, string? secondaryHighRes = null, string? secondayLowRes = null)
    {
        // Force https, required on Android.
        primaryHighRes = primaryHighRes?.Replace("http:", "https");
        primaryLowRes = primaryLowRes?.Replace("http:", "https");
        secondaryHighRes = secondaryHighRes?.Replace("http:", "https");
        secondayLowRes = secondayLowRes?.Replace("http:", "https");

        //Dirty trick to fix issue: https://groups.google.com/forum/?utm_medium=email&utm_source=footer#!msg/sr-api/Y5Y2mHpvJ9A/7e_wg6joFQAJ
        PrimaryLinkLowRes = !IsLikelyAValidImageUrl(primaryLowRes) ? null : primaryLowRes;
        PrimaryLinkHighRes = PrimaryLinkLowRes ?? (!IsLikelyAValidImageUrl(primaryHighRes) ? null : primaryHighRes);
        SecondaryLinkHighRes = !IsLikelyAValidImageUrl(secondaryHighRes) ? null : secondaryHighRes;
        SecondaryLinkLowRes = SecondaryLinkLowRes ?? (!IsLikelyAValidImageUrl(secondayLowRes) ? null : secondayLowRes);
    }

    public ImageLink(string? primaryHighRes = null)
        : this (primaryHighRes, null, null, null)
    {
    }

    public ImageLink(string? primaryHighRes, string? primaryLowRes = null)
        : this (primaryHighRes, primaryLowRes, null, null)
    {
    }

    public ImageLink(string? primaryHighRes, ImageLink secondarySource)
        : this (primaryHighRes, null, secondarySource)
    {
    }

    public ImageLink(string? primaryHighRes = null, string? primaryLowRes = null, ImageLink? secondarySource = null)
        : this (primaryHighRes, primaryLowRes, secondarySource?.HighResolution, secondarySource?.LowResolution)
    {
    }

    public ImageLink(ImageLink primarySource, ImageLink? secondarySource = null)
        : this (primarySource.HighResolution, primarySource.LowResolution, secondarySource)
    {
    }

    #endregion

    #region Methods

    private static bool IsLikelyAValidImageUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return false;
        }

        if (url.EndsWith("/"))
        {
            return false;
        }

        return true;
    }

    private static string GetSafeUrl(string? s)
    {
        if (Uri.IsWellFormedUriString(s, UriKind.Absolute))
        {
            return s;
        }

        return DefaultUrl;
    }

    #endregion

    #region Properites

    public string HighResolution => 
        GetSafeUrl(PrimaryLinkHighRes ?? PrimaryLinkLowRes ?? SecondaryLinkHighRes ?? SecondaryLinkLowRes);

    public string LowResolution => 
        GetSafeUrl(PrimaryLinkLowRes ?? PrimaryLinkHighRes ?? SecondaryLinkLowRes ?? SecondaryLinkHighRes);

    #endregion
}
