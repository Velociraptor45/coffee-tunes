using System.Diagnostics.CodeAnalysis;
using CoffeeTunes.Contracts;
using CoffeeTunes.Contracts.Beans;
using CoffeeTunes.WebApi.Hubs;
using CoffeeTunes.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CoffeeTunes.WebApi.Endpoints;

public static class BeansEndpoints
{
    [StringSyntax("Route")]
    private const string _routeBase = $"v1/{{franchiseId:guid}}/bars/{{barId:guid}}/beans";

    public static void RegisterBeansEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .RegisterCastBeans();
    }

    private static IEndpointRouteBuilder RegisterCastBeans(this IEndpointRouteBuilder builder)
    {
        builder.MapPost($"/{_routeBase}", CastBeans)
            .WithName(nameof(CastBeans))
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> CastBeans(
        [FromRoute] Guid franchiseId,
        [FromRoute] Guid barId,
        [FromBody] CastBeansContract contract,
        [FromServices] FranchiseAccessService franchiseAccessService,
        [FromServices] BeansService beansService,
        [FromServices] IHubContext<BarHub, IBarClient> hubContext,
        CancellationToken cancellationToken)
    {
        await franchiseAccessService.EnsureAccessToFranchiseAsync(franchiseId, cancellationToken);
        var (hipsterId, _) = franchiseAccessService.GetHipsterInfoFromToken()
            ?? throw new UnauthorizedAccessException("Hipster information could not be retrieved from token.");
        
        await beansService.EnsureIngredientIsCurrentlySelected(contract.IngredientId, cancellationToken);
        await beansService.CreateBeansAsync(contract, cancellationToken);
        
        await hubContext.Clients
            .Group(BarHub.GetGroupName(franchiseId, barId))
            .BeanCast(new BeanCastContract
            {
                HipsterId = hipsterId
            });

        return Results.NoContent();
    }
    
}