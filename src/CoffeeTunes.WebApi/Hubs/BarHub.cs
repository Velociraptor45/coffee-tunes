using System.Collections.Concurrent;
using CoffeeTunes.Contracts;
using CoffeeTunes.Contracts.Hipsters;
using CoffeeTunes.WebApi.Auth;
using CoffeeTunes.WebApi.Contexts;
using CoffeeTunes.WebApi.Mappers;
using CoffeeTunes.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CoffeeTunes.WebApi.Hubs;

[Authorize("User")]
public class BarHub(CoffeeTunesDbContext dbContext, FranchiseAccessService accessService, AuthOptions authOptions) : Hub<IBarClient>
{
    private static readonly PresenceTracker PresenceTracker = new();
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

        await Clients.Others.HipsterJoined(new HipsterJoinedContract
        {
            HipsterId = hipsterId,
            Name = Context.User.Claims.First(c => c.Type == authOptions.NameClaimType).Value
        });
        
        PresenceTracker.OnConnected(groupName, hipsterId, Context.ConnectionId);

        // notify about existing online hipsters
        var onlineHipsters = PresenceTracker.Trackers.GetValueOrDefault(groupName, new HashSet<Guid>());
        var hipsters = await dbContext.Hipsters
            .AsNoTracking()
            .Where(h => onlineHipsters.Contains(h.Id))
            .ToListAsync();
        
        foreach (var hipster in hipsters)
        {
            await Clients.Caller.HipsterJoined(new HipsterJoinedContract
            {
                HipsterId = hipster.Id,
                Name = hipster.Name,
            });
        }
        
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
        
        await Clients.Others.HipsterLeft(hipsterId);
        
        PresenceTracker.OnDisconnected(Context.ConnectionId);
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        PresenceTracker.OnDisconnected(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}

public class PresenceTracker
{
    // connectionId => groupName, hipsterId
    private readonly ConcurrentDictionary<string, (string groupName, Guid hipsterId)> _connections = new();
    // groupName => set of hipsterIds
    public ConcurrentDictionary<string, HashSet<Guid>> Trackers { get; } = new();
    
    public void OnConnected(string groupName, Guid hipsterId, string connectionId)
    {
        var hipsters = Trackers.GetOrAdd(groupName, _ => new HashSet<Guid>());
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
        
        if (!Trackers.TryGetValue(groupName, out var hipsters))
            return;
        
        lock (hipsters)
        {
            hipsters.Remove(hipsterId);
            if (hipsters.Count == 0) 
                Trackers.TryRemove(groupName, out _);
        }
    }
}