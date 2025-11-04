namespace CoffeeTunes.WebApi.Services.Youtube;

public class YouTubeOptions
{
    [ConfigurationKeyName("CT_YT_API_KEY")]
    public string ApiKey { get; set; }
}
