using System.Collections.Concurrent;
using CoffeeTunes.Contracts;
using CoffeeTunes.Contracts.Beans;
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
public class BarHub(CoffeeTunesDbContext dbContext, FranchiseAccessService accessService, AuthOptions authOptions)
    : Hub<IBarClient>
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

        // show ingredient
        var ingredient = await dbContext.Ingredients
            .AsNoTracking()
            .Include(i => i.Beans)
            .ThenInclude(b => b.CastFrom)
            .Include(i => i.Beans)
            .ThenInclude(b => b.CastTo)
            .Include(i => i.Owners)
            .ThenInclude(o => o.Hipster)
            .Where(i => i.BarId == barId && !i.Used && i.Selected)
            .FirstOrDefaultAsync();

        if (ingredient is null)
            return;
        
        var brewCycleContract = ingredient.ToBrewCycleContract();
        await Clients.Caller.BrewCycleUpdated(brewCycleContract);
        
        // send all hipsters who already cast beans
        var hipstersCastIngredientBeans = await dbContext.HipstersCastIngredientBeans
            .AsNoTracking()
            .Where(b => b.IngredientId == ingredient.Id)
            .ToListAsync();
        
        foreach (var hipstersCastIngredientBean in hipstersCastIngredientBeans)
        {
            await Clients.All.BeanCast(new BeanCastContract
            {
                HipsterId = hipstersCastIngredientBean.HipsterId
            });
        }
        
        // reveal ingredient solution
        if (ingredient.Revealed)
        {
            var revealContract = ingredient.ToBrewCycleRevealContract();
            await Clients.Caller.RevealBrewCycle(revealContract);
        }
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
        if(PresenceTracker.Connections.TryGetValue(Context.ConnectionId, out var hipsterId))
            await Clients.Others.HipsterLeft(hipsterId.hipsterId);
        
        PresenceTracker.OnDisconnected(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}

public class PresenceTracker
{
    // connectionId => groupName, hipsterId
    public ConcurrentDictionary<string, (string groupName, Guid hipsterId)> Connections { get; } = new();
    // groupName => set of hipsterIds
    public ConcurrentDictionary<string, HashSet<Guid>> Trackers { get; } = new();
    
    public void OnConnected(string groupName, Guid hipsterId, string connectionId)
    {
        var hipsters = Trackers.GetOrAdd(groupName, _ => new HashSet<Guid>());
        lock (hipsters)
        {
            hipsters.Add(hipsterId);
        }
        
        Connections[connectionId] = (groupName, hipsterId);
    }
    
    public void OnDisconnected(string connectionId)
    {
        if (!Connections.TryRemove(connectionId, out var info))
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