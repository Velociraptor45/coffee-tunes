using System.Diagnostics.CodeAnalysis;
using CoffeeTunes.Contracts;
using CoffeeTunes.Contracts.Bars;
using CoffeeTunes.WebApi.Contexts;
using CoffeeTunes.WebApi.Hubs;
using CoffeeTunes.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CoffeeTunes.WebApi.Endpoints;

public static class BrewCycleEndpoints
{
    [StringSyntax("Route")]
    private const string _routeBase = $"v1/{{franchiseId:guid}}/bars/{{barId:guid}}";
    
    public static void RegisterBrewCycleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .RegisterStartBrewCycle();
    }

    private static IEndpointRouteBuilder RegisterStartBrewCycle(this IEndpointRouteBuilder builder)
    {
        builder.MapPut($"/{_routeBase}/open", StartBrewCycle)
            .WithName(nameof(StartBrewCycle))
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> StartBrewCycle(
        [FromRoute] Guid franchiseId,
        [FromRoute] Guid barId,
        [FromServices] CoffeeTunesDbContext dbContext,
        [FromServices] FranchiseAccessService franchiseAccessService,
        [FromServices] BarService barService,
        [FromServices] IHubContext<BarHub, IBarClient> hubContext,
        [FromServices] BrewCycleService brewCycleService,
        CancellationToken cancellationToken)
    {
        await franchiseAccessService.EnsureAccessToFranchiseAsync(franchiseId, cancellationToken);

        var bar = await barService.GetInSupplyBar(barId, franchiseId, cancellationToken);
        
        if (bar.IsOpen)
            return Results.NoContent();
        
        bar.IsOpen = true;
        await dbContext.SaveChangesAsync(cancellationToken);
        
        var barContract = new BarContract
        {
            Id = bar.Id,
            Topic = bar.Topic,
            IsOpen = bar.IsOpen,
            HasSupplyLeft = bar.HasSupplyLeft,
            MaxIngredientsPerHipster = bar.MaxIngredientsPerHipster
        };
        await hubContext.Clients
            .Group(BarHub.GetGroupName(franchiseId, barId))
            .BarUpdated(barContract);
        
        var brewCycle = await brewCycleService.StartNewCycleAsync(barId, cancellationToken);
        await hubContext.Clients
            .Group(BarHub.GetGroupName(franchiseId, barId))
            .BrewCycleUpdated(brewCycle);

        return Results.NoContent();
    }
}