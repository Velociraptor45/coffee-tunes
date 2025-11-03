using CoffeeTunes.Frontend.Configs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace CoffeeTunes.Frontend.Auth;

public class CustomAddressAuthorizationMessageHandler : AuthorizationMessageHandler
{
    public CustomAddressAuthorizationMessageHandler(IAccessTokenProvider provider, NavigationManager navigation,
        ConnectionConfig config)
        : base(provider, navigation)
    {
        ConfigureHandler([config.ApiUri]);
    }
}