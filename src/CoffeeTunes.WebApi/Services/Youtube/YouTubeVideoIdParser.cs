using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.WebUtilities;

namespace CoffeeTunes.WebApi.Services.Youtube;

public static partial class YouTubeVideoIdParser
{
    private static readonly Regex VideoIdPattern = VideoIdRegex();

    public static bool TryParse(string? input, [NotNullWhen(true)] out string? videoId)
    {
        videoId = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var candidate = input.Trim();

        if (VideoIdPattern.IsMatch(candidate))
        {
            videoId = candidate;
            return true;
        }

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
        {
            return false;
        }

        candidate = ExtractFromUri(uri);
        if (candidate is null)
        {
            return false;
        }

        candidate = candidate.Trim();
        if (!VideoIdPattern.IsMatch(candidate))
        {
            return false;
        }

        videoId = candidate;
        return true;
    }

    private static string? ExtractFromUri(Uri uri)
    {
        if (!uri.Host.Contains("youtu", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (uri.Host.EndsWith("youtu.be", StringComparison.OrdinalIgnoreCase))
        {
            return uri.AbsolutePath.Trim('/');
        }

        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length >= 2 &&
            (string.Equals(segments[0], "shorts", StringComparison.OrdinalIgnoreCase)
            || string.Equals(segments[0], "live", StringComparison.OrdinalIgnoreCase)
            || string.Equals(segments[0], "embed", StringComparison.OrdinalIgnoreCase)))
        {
            return segments[1];
        }

        var query = QueryHelpers.ParseQuery(uri.Query);
        if (query.TryGetValue("v", out var values))
        {
            return values.FirstOrDefault();
        }

        return null;
    }

    [GeneratedRegex("^[A-Za-z0-9_-]{11}$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex VideoIdRegex();
}
