using System.Diagnostics.CodeAnalysis;
using CoffeeTunes.Contracts;
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
            .RegisterStartBrewCycle()
            .RegisterRevealIngredientResults()
            .RegisterEndBrewCycle()
            .RegisterNextIngredient();
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
        
        var barContract = await barService.GetBarContractAsync(barId, franchiseId, cancellationToken);
        
        var brewCycle = await brewCycleService.StartNewCycleAsync(barId, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        await hubContext.Clients
            .Group(BarHub.GetGroupName(franchiseId, barId))
            .BarUpdated(barContract);
        await hubContext.Clients
            .Group(BarHub.GetGroupName(franchiseId, barId))
            .BrewCycleUpdated(brewCycle);

        return Results.NoContent();
    }

    private static IEndpointRouteBuilder RegisterRevealIngredientResults(this IEndpointRouteBuilder builder)
    {
        builder.MapPut($"/{_routeBase}/reveal", RevealIngredientResults)
            .WithName(nameof(RevealIngredientResults))
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> RevealIngredientResults(
        [FromRoute] Guid franchiseId,
        [FromRoute] Guid barId,
        [FromServices] FranchiseAccessService franchiseAccessService,
        [FromServices] IHubContext<BarHub, IBarClient> hubContext,
        [FromServices] BrewCycleService brewCycleService,
        CancellationToken cancellationToken)
    {
        await franchiseAccessService.EnsureAccessToFranchiseAsync(franchiseId, cancellationToken);

        var revealContract = await brewCycleService.RevealIngredientAsync(barId, franchiseId, cancellationToken);
        
        await hubContext.Clients
            .Group(BarHub.GetGroupName(franchiseId, barId))
            .RevealBrewCycle(revealContract);

        return Results.NoContent();
    }

    private static IEndpointRouteBuilder RegisterEndBrewCycle(this IEndpointRouteBuilder builder)
    {
        builder.MapPut($"/{_routeBase}/close", EndBrewCycle)
            .WithName(nameof(EndBrewCycle))
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> EndBrewCycle(
        [FromRoute] Guid franchiseId,
        [FromRoute] Guid barId,
        [FromServices] FranchiseAccessService franchiseAccessService,
        [FromServices] IHubContext<BarHub, IBarClient> hubContext,
        [FromServices] BrewCycleService brewCycleService,
        CancellationToken cancellationToken)
    {
        await franchiseAccessService.EnsureAccessToFranchiseAsync(franchiseId, cancellationToken);

        var barContract = await brewCycleService.EndCycleAsync(barId, franchiseId, cancellationToken);
        
        await hubContext.Clients
            .Group(BarHub.GetGroupName(franchiseId, barId))
            .BarUpdated(barContract);

        return Results.NoContent();
    }

    private static IEndpointRouteBuilder RegisterNextIngredient(this IEndpointRouteBuilder builder)
    {
        builder.MapPut($"/{_routeBase}/next", NextIngredient)
            .WithName(nameof(NextIngredient))
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> NextIngredient(
        [FromRoute] Guid franchiseId,
        [FromRoute] Guid barId,
        [FromServices] FranchiseAccessService franchiseAccessService,
        [FromServices] IHubContext<BarHub, IBarClient> hubContext,
        [FromServices] BrewCycleService brewCycleService,
        CancellationToken cancellationToken)
    {
        await franchiseAccessService.EnsureAccessToFranchiseAsync(franchiseId, cancellationToken);

        var brewCycleContract = await brewCycleService.SelectNextIngredientAsync(barId, franchiseId, cancellationToken);
        
        await hubContext.Clients
            .Group(BarHub.GetGroupName(franchiseId, barId))
            .BrewCycleUpdated(brewCycleContract);

        return Results.NoContent();
    }
}