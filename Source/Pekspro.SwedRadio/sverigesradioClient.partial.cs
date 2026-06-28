#nullable enable

namespace Pekspro.SwedRadio;

public partial class SrClient
{
    public static string UserAgent { get; set; } = "Pekspro.RadioStorm";

    partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, string url)
    {
        request.Headers.Add("User-Agent", UserAgent);
    }

    partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, System.Text.StringBuilder urlBuilder)
    {
        request.Headers.Add("User-Agent", UserAgent);
    }
}
