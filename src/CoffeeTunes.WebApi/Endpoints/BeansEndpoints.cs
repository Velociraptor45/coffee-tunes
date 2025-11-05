using System.Diagnostics.CodeAnalysis;
using CoffeeTunes.Contracts.Beans;
using CoffeeTunes.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

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
        CancellationToken cancellationToken)
    {
        await franchiseAccessService.EnsureAccessToFranchiseAsync(franchiseId, cancellationToken);

        await beansService.EnsureIngredientIsCurrentlySelected(contract.IngredientId, cancellationToken);
        await beansService.CreateBeansAsync(contract, cancellationToken);

        return Results.NoContent();
    }
    
}