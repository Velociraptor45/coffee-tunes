using System.Collections.Concurrent;
using CoffeeTunes.Contracts;
using CoffeeTunes.WebApi.Contexts;
using CoffeeTunes.WebApi.Mappers;
using CoffeeTunes.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CoffeeTunes.WebApi.Hubs;

[Authorize("User")]
public class BarHub(CoffeeTunesDbContext dbContext, FranchiseAccessService accessService) : Hub<IBarClient>
{
    private readonly PresenceTracker _presenceTracker = new();
    public static string GetGroupName(Guid franchiseId, Guid barId) => $"{franchiseId}:{barId}";
    
    public async Task JoinBar(Guid barId)
    {
        var hipsterId = Guid.Parse(Context.User!.Claims.First(c => c.Type == "sub").Value);
        
        await accessService.EnsureAccessToBarAsync(hipsterId, barId, CancellationToken.None);
        
        var bar = await dbContext.Bars
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == barId);
        
        if (bar is null)
            throw new HubException("Bar not found");
        
        var groupName = GetGroupName(bar.FranchiseId, bar.Id);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _presenceTracker.OnConnected(groupName, hipsterId, Context.ConnectionId);

        if (!bar.IsOpen)
            return;

        var ingredient = await dbContext.Ingredients
            .AsNoTracking()
            .Include(i => i.Owners)
            .Where(i => i.BarId == barId && !i.Used && i.Selected)
            .FirstOrDefaultAsync();

        if (ingredient is null)
            return;
        
        var brewCycleContract = ingredient.ToBrewCycleContract();
        await Clients.Caller.BrewCycleUpdated(brewCycleContract);
    }
    
    public async Task LeaveBar(Guid barId)
    {
        var hipsterId = Guid.Parse(Context.User!.Claims.First(c => c.Type == "sub").Value);
        await accessService.EnsureAccessToBarAsync(hipsterId, barId, CancellationToken.None);
        
        _presenceTracker.OnDisconnected(Context.ConnectionId);
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _presenceTracker.OnDisconnected(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}

public class PresenceTracker
{
    // groupName => set of hipsterIds
    private readonly ConcurrentDictionary<string, HashSet<Guid>> _trackers = new();
    // connectionId => groupName, hipsterId
    private readonly ConcurrentDictionary<string, (string groupName, Guid hipsterId)> _connections = new();
    
    public void OnConnected(string groupName, Guid hipsterId, string connectionId)
    {
        var hipsters = _trackers.GetOrAdd(groupName, _ => new HashSet<Guid>());
        lock (hipsters)
        {
            hipsters.Add(hipsterId);
        }
        
        _connections[connectionId] = (groupName, hipsterId);
    }
    
    public void OnDisconnected(string connectionId)
    {
        if (!_connections.TryRemove(connectionId, out var info))
            return;
        
        var (groupName, hipsterId) = info;
        
        if (!_trackers.TryGetValue(groupName, out var hipsters))
            return;
        
        lock (hipsters)
        {
            hipsters.Remove(hipsterId);
            if (hipsters.Count == 0) 
                _trackers.TryRemove(groupName, out _);
        }
    }
}