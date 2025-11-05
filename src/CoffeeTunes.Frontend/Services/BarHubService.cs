using CoffeeTunes.Contracts;
using CoffeeTunes.Contracts.Bars;
using CoffeeTunes.Contracts.Beans;
using CoffeeTunes.Contracts.BrewCycles;
using CoffeeTunes.Contracts.Hipsters;
using CoffeeTunes.Frontend.Configs;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.SignalR.Client;

namespace CoffeeTunes.Frontend.Services;

public class BarHubService : IAsyncDisposable
{
    private readonly IAccessTokenProvider _tokenProvider;
    private readonly ConnectionConfig _connectionConfig;
    private HubConnection? _connection;
    
    public event Func<BarContract, Task>? OnBarUpdated;
    public event Func<BrewCycleContract, Task>? OnBrewCycleUpdated;
    public event Func<BeanCastContract, Task>? OnBeanCast;
    public event Func<HipsterJoinedContract, Task>? OnHipsterJoined;
    
    public BarHubService(IAccessTokenProvider tokenProvider, ConnectionConfig connectionConfig)
    {
        _tokenProvider = tokenProvider;
        _connectionConfig = connectionConfig;
    }
    
    public async Task ConnectAsync()
    {
        if (_connection != null)
        {
            // Check if connection is in a disconnected state and attempt to restart
            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync();
            }
            return;
        }
        
        var hubUrl = _connectionConfig.ApiUri.TrimEnd('/') + "/hubs/bar";
        
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = async () =>
                {
                    var tokenResult = await _tokenProvider.RequestAccessToken();
                    if (tokenResult.TryGetToken(out var token))
                    {
                        return token.Value;
                    }
                    return null;
                };
            })
            .WithAutomaticReconnect()
            .Build();
        
        _connection.On<BarContract>(nameof(IBarClient.BarUpdated), async (barContract) =>
        {
            if (OnBarUpdated != null)
                await OnBarUpdated.Invoke(barContract);
        });
        
        _connection.On<BrewCycleContract>(nameof(IBarClient.BrewCycleUpdated), async (brewCycleContract) =>
        {
            if (OnBrewCycleUpdated != null)
                await OnBrewCycleUpdated.Invoke(brewCycleContract);
        });
        
        _connection.On<BeanCastContract>(nameof(IBarClient.BeanCast), async (beanCastContract) =>
        {
            if (OnBeanCast != null)
                await OnBeanCast.Invoke(beanCastContract);
        });
        
        _connection.On<HipsterJoinedContract>(nameof(IBarClient.HipsterJoined), async (hipsterJoinedContract) =>
        {
            if (OnHipsterJoined != null)
                await OnHipsterJoined.Invoke(hipsterJoinedContract);
        });
        
        await _connection.StartAsync();
    }
    
    public async Task JoinBarAsync(Guid barId)
    {
        if (_connection == null)
            throw new InvalidOperationException("Hub connection is not initialized");
        
        // Ensure connection is in Connected state before invoking
        if (_connection.State != HubConnectionState.Connected)
        {
            await _connection.StartAsync();
        }
        
        await _connection.InvokeAsync("JoinBar", barId);
    }
    
    public async Task LeaveBarAsync(Guid barId)
    {
        if (_connection == null)
            return;
        
        // Only invoke if connection is in Connected state
        if (_connection.State != HubConnectionState.Connected)
            return;
        
        await _connection.InvokeAsync("LeaveBar", barId);
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}
