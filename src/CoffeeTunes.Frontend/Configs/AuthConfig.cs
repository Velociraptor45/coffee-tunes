namespace CoffeeTunes.Frontend.Configs;

public sealed class AuthConfig
{
    [ConfigurationKeyName("CT_AUTH_AUTHORITY")]
    public string Authority { get; set; } = string.Empty;

    [ConfigurationKeyName("CT_AUTH_CLIENT_ID")]
    public string ClientId { get; set; } = string.Empty;

    [ConfigurationKeyName("CT_AUTH_DEFAULT_SCOPES")]
    public string[] DefaultScopes { get; set; } = [];

    [ConfigurationKeyName("CT_AUTH_RESPONSE_TYPE")]
    public string ResponseType { get; set; } = "code";

    [ConfigurationKeyName("CT_AUTH_ROLE_NAME_USER")]
    public string UserRoleName { get; init; } = "User";

    [ConfigurationKeyName("CT_AUTH_CLAIM_NAME")]
    public string NameClaimIdentifier { get; init; } = "given_name";

    [ConfigurationKeyName("CT_AUTH_CLAIM_ROLE")]
    public string RoleClaimIdentifier { get; init; } = "role";

    [ConfigurationKeyName("CT_AUTH_CLAIM_SCOPE")]
    public string ScopeClaimIdentifier { get; init; } = "scope";
}