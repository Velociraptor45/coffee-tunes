using System.Diagnostics.CodeAnalysis;
using CoffeeTunes.Contracts.Ingredients;
using CoffeeTunes.WebApi.Contexts;
using CoffeeTunes.WebApi.Entities;
using CoffeeTunes.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeTunes.WebApi.Endpoints;

public static class IngredientEndpoints
{
    [StringSyntax("Route")]
    private const string _routeBase = $"v1/{{franchiseId:guid}}/bars/{{barId:guid}}/ingredients";
    
    public static void RegisterIngredientEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .RegisterGetIngredients()
            .RegisterAddIngredient()
            .RegisterRemoveIngredient();
    }

    private static IEndpointRouteBuilder RegisterGetIngredients(this IEndpointRouteBuilder builder)
    {
        builder.MapGet($"/{_routeBase}", GetIngredients)
            .WithName(nameof(GetIngredients))
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> GetIngredients(
        [FromRoute] Guid franchiseId,
        [FromRoute] Guid barId,
        [FromServices] CoffeeTunesDbContext dbContext,
        [FromServices] FranchiseAccessService franchiseAccessService,
        [FromServices] BarService barService,
        CancellationToken cancellationToken)
    {
        await franchiseAccessService.EnsureAccessToFranchiseAsync(franchiseId, cancellationToken);
        var (hipsterId, _) = franchiseAccessService.GetHipsterInfoFromToken()
                             ?? throw new InvalidOperationException("Authentication failed");

        var bar = await barService.GetBarAsync(barId, franchiseId, cancellationToken);
        
        var ingredients = await dbContext.Ingredients
            .AsNoTracking()
            .Where(i => i.OwnerId == hipsterId && i.BarId == bar.Id)
            .Select(i => new IngredientContract
            {
                Id = i.Id,
                OwnerId = i.OwnerId,
                Name = i.Name,
                Url = i.Url,
                Used = i.Used
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(ingredients);
    }

    private static IEndpointRouteBuilder RegisterAddIngredient(this IEndpointRouteBuilder builder)
    {
        builder.MapPost($"/{_routeBase}", AddIngredient)
            .WithName(nameof(AddIngredient))
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> AddIngredient(
        [FromRoute] Guid franchiseId,
        [FromRoute] Guid barId,
        [FromBody] AddIngredientContract contract,
        [FromServices] CoffeeTunesDbContext dbContext,
        [FromServices] FranchiseAccessService franchiseAccessService,
        CancellationToken cancellationToken)
    {
        await franchiseAccessService.EnsureAccessToFranchiseAsync(franchiseId, cancellationToken);
        var (hipsterId, _) = franchiseAccessService.GetHipsterInfoFromToken()
                             ?? throw new InvalidOperationException("Authentication failed");
        
        var bar = await dbContext.Bars
            .AsNoTracking()
            .Where(b => b.FranchiseId == franchiseId && b.Id == barId)
            .FirstOrDefaultAsync(cancellationToken);
        if (bar is null)
            return Results.NotFound("Bar not found in the specified franchise.");
        if(!bar.HasSupplyLeft)
            return Results.BadRequest("The bar has no supply left - you can't modify ingredients.");
        
        // todo retrieve video info
        
        var ingredient = new Ingredient
        {
            Id = Guid.CreateVersion7(),
            Name = "My Cool YT video",
            Url = contract.Url,
            Used = false,
            OwnerId = hipsterId,
            BarId = bar.Id
        };
        dbContext.Ingredients.Add(ingredient);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static IEndpointRouteBuilder RegisterRemoveIngredient(this IEndpointRouteBuilder builder)
    {
        builder.MapDelete($"/{_routeBase}/{{id:guid}}", RemoveIngredient)
            .WithName(nameof(RemoveIngredient))
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> RemoveIngredient(
        [FromRoute] Guid franchiseId,
        [FromRoute] Guid barId,
        [FromRoute] Guid id,
        [FromServices] CoffeeTunesDbContext dbContext,
        [FromServices] FranchiseAccessService franchiseAccessService,
        [FromServices] BarService barService,
        CancellationToken cancellationToken)
    {
        await franchiseAccessService.EnsureAccessToFranchiseAsync(franchiseId, cancellationToken);
        var (hipsterId, _) = franchiseAccessService.GetHipsterInfoFromToken()
                             ?? throw new InvalidOperationException("Authentication failed");
        
        var bar = await barService.GetInSupplyBar(barId, franchiseId, cancellationToken);
        
        var ingredient = await dbContext.Ingredients
            .Where(i => i.Id == id && i.OwnerId == hipsterId && i.BarId == bar.Id)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (ingredient is null)
            return Results.NotFound("Ingredient not found.");
        
        dbContext.Ingredients.Remove(ingredient);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}