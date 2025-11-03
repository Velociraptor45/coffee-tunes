namespace CoffeeTunes.Frontend.Configs;

public sealed class ConnectionConfig
{
    [ConfigurationKeyName("CT_API_URL")]
    public string ApiUri { get; set; } = string.Empty;
}