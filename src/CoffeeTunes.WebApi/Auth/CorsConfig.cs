namespace CoffeeTunes.WebApi.Auth;

internal class CorsConfig
{
    [ConfigurationKeyName("CT_CORS_ORIGIN")]
    public string[] AllowedOrigins { get; set; } = [];
}