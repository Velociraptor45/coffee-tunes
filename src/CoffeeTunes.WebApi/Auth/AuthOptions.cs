namespace CoffeeTunes.WebApi.Auth;

public class AuthOptions
{
    [ConfigurationKeyName("CT_AUTH_AUTHORITY")]
    public string Authority { get; set; } = string.Empty;

    [ConfigurationKeyName("CT_AUTH_AUDIENCE")]
    public string Audience { get; set; } = string.Empty;

    [ConfigurationKeyName("CT_AUTH_VALID_TYPES")]
    public string[] ValidTypes { get; set; } = [];

    [ConfigurationKeyName("CT_AUTH_CLAIM_NAME")]
    public string NameClaimType { get; set; } = "given_name";

    [ConfigurationKeyName("CT_AUTH_CLAIM_ROLE")]
    public string RoleClaimType { get; set; } = "role";

    [ConfigurationKeyName("CT_AUTH_ROLE_NAME_USER")]
    public string UserRoleName { get; set; } = "User";

    public string OidcUrl => $"{Authority}/.well-known/openid-configuration";
}