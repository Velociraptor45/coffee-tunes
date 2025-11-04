using Microsoft.Extensions.Options;

namespace CoffeeTunes.WebApi.Services.Youtube;

public sealed record YouTubeVideoMetadata(string? Title, string? ThumbnailUrl);

public sealed class YouTubeMetadataProvider(
    IHttpClientFactory httpClientFactory,
    IOptions<YouTubeOptions> options,
    ILogger<YouTubeMetadataProvider> logger)
{
    public async Task<YouTubeVideoMetadata?> GetMetadataAsync(string videoId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(videoId))
            return null;

        var apiKey = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
            return null;

        try
        {
            using var httpClient = httpClientFactory.CreateClient("YouTubeApi");
            using var response = await httpClient
                .GetAsync($"videos?part=snippet&id={videoId}&key={apiKey}", cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "YouTube metadata lookup failed for video {VideoId} with status {StatusCode}.",
                    videoId,
                    response.StatusCode);
                return null;
            }

            var payload = await response.Content
                .ReadFromJsonAsync<YouTubeVideosResponse>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (payload?.Items is not { Length: > 0 } items)
                return null;

            var snippet = items[0]?.Snippet;
            if (snippet is null)
                return null;

            var title = snippet.Title?.Trim();
            var thumbnailUrl = snippet.Thumbnails is null
                ? null
                : GetPreferredUrl(snippet.Thumbnails);

            return new YouTubeVideoMetadata(title, thumbnailUrl);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to load YouTube metadata for video {VideoId}.", videoId);
            return null;
        }
    }

    private sealed record YouTubeVideosResponse(YouTubeVideoItem?[]? Items);

    private sealed record YouTubeVideoItem(YouTubeSnippet? Snippet);

    private sealed record YouTubeSnippet(string? Title, YouTubeThumbnails? Thumbnails);

    private sealed record YouTubeThumbnails(
        YouTubeThumbnail? Maxres,
        YouTubeThumbnail? Standard,
        YouTubeThumbnail? High,
        YouTubeThumbnail? Medium,
        YouTubeThumbnail? Default);

    private sealed record YouTubeThumbnail(string? Url);

    private static string? GetPreferredUrl(YouTubeThumbnails thumbnails)
    {
        return FirstValid(
            thumbnails.Maxres?.Url,
            thumbnails.Standard?.Url,
            thumbnails.High?.Url,
            thumbnails.Medium?.Url,
            thumbnails.Default?.Url);
    }

    private static string? FirstValid(params string?[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                return candidate;
            }
        }

        return null;
    }
}
