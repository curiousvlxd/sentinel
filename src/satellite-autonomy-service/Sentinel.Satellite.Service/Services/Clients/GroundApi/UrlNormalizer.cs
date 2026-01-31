namespace Sentinel.Satellite.Service.Services.Clients.GroundApi;

internal static class UrlNormalizer
{
    public static string NormalizeHttpUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return url;

        if (url.StartsWith("tcp://", StringComparison.OrdinalIgnoreCase)) return string.Concat("http://", url.AsSpan(6));

        return url.StartsWith("tcp:", StringComparison.OrdinalIgnoreCase) ? string.Concat("http:", url.AsSpan(4)) : url;
    }
}
