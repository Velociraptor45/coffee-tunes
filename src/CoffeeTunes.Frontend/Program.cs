using CoffeeTunes.Contracts;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CoffeeTunes.Frontend;
using CoffeeTunes.Frontend.Auth;
using CoffeeTunes.Frontend.Configs;
using CoffeeTunes.Frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestEase.HttpClientFactory;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

AddSecurity(builder);
ConfigureHttpClient(builder);

builder.Services.AddScoped<BarHubService>();

var app = builder.Build();

await app.RunAsync();
return;

static void AddSecurity(WebAssemblyHostBuilder builder)
{
    var authConfig = new AuthConfig();
    builder.Configuration.Bind(authConfig);
    builder.Services.AddSingleton(authConfig);
    builder.Services.AddOidcAuthentication(opt =>
    {
        opt.ProviderOptions.Authority = authConfig.Authority;
        opt.ProviderOptions.MetadataUrl = $"{opt.ProviderOptions.Authority}/.well-known/openid-configuration";
        opt.ProviderOptions.ClientId = authConfig.ClientId;
        opt.ProviderOptions.ResponseType = authConfig.ResponseType;
        foreach (var scope in authConfig.DefaultScopes)
            opt.ProviderOptions.DefaultScopes.Add(scope);

        opt.UserOptions.NameClaim = authConfig.NameClaimIdentifier;
        opt.UserOptions.RoleClaim = authConfig.RoleClaimIdentifier;
        opt.UserOptions.ScopeClaim = authConfig.ScopeClaimIdentifier;
    }).AddAccountClaimsPrincipalFactory<ArrayClaimsPrincipalFactory<RemoteUserAccount>>();

    builder.Services.AddAuthorizationCore(cfg =>
    {
        cfg.AddPolicy("User", new AuthorizationPolicyBuilder()
            .RequireRole(authConfig.UserRoleName)
            .Build());
    });
}

static void ConfigureHttpClient(WebAssemblyHostBuilder builder)
{
    var connectionConfig = new ConnectionConfig();
    builder.Configuration.Bind(connectionConfig);

    builder.Services.AddSingleton(connectionConfig);

    if (string.IsNullOrWhiteSpace(connectionConfig.ApiUri))
        throw new InvalidOperationException("The Api-Url is missing in the configuration");

    builder.Services.AddScoped<CustomAddressAuthorizationMessageHandler>();

    builder.Services.AddRestEaseClient<ICoffeeTunesApi>(
        connectionConfig.ApiUri,
        opt =>
        {
            opt.JsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };
        })
        .AddHttpMessageHandler<CustomAddressAuthorizationMessageHandler>();
}