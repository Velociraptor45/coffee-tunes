using System.Diagnostics.CodeAnalysis;
using CoffeeTunes.Contracts.Ingredients;
using CoffeeTunes.WebApi.Contexts;
using CoffeeTunes.WebApi.Entities;
using CoffeeTunes.WebApi.Services;
using CoffeeTunes.WebApi.Services.Youtube;
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
            .RegisterRemoveIngredient()
            .RegisterGetUnusedIngredientCount()
            .RegisterGetIngredientContributors()
            .RegisterGetIngredientPlaylist();
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
            .Where(i => i.Owners!.Any(o => o.HipsterId == hipsterId) && i.BarId == bar.Id)
            .Select(i => new IngredientContract
            {
                Id = i.Id,
                Name = i.Name,
                Url = i.Url,
                ThumbnailUrl = i.ThumbnailUrl,
                Used = i.Used,
                Selected = i.Selected
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
        [FromServices] YouTubeMetadataProvider metadataProvider,
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
        
        if (!YouTubeVideoIdParser.TryParse(contract.Url, out var videoId))
        {
            return TypedResults.BadRequest("Unable to extract a YouTube video identifier from the provided URL.");
        }

        var metadata = await metadataProvider.GetMetadataAsync(videoId, cancellationToken);
        if (metadata is null || string.IsNullOrWhiteSpace(metadata.Title) || string.IsNullOrWhiteSpace(metadata.ThumbnailUrl))
            return TypedResults.BadRequest("Unable to retrieve metadata for the provided YouTube video.");
        
        var ingredientCount = await dbContext.Ingredients
            .AsNoTracking()
            .Where(i => i.Owners!.Any(o => o.HipsterId == hipsterId) && i.BarId == bar.Id)
            .CountAsync(cancellationToken);
        if (ingredientCount >= bar.MaxIngredientsPerHipster)
            return Results.BadRequest($"You have reached the maximum number of ingredients ({bar.MaxIngredientsPerHipster}) for this bar.");
        
        var ingredient = await dbContext.Ingredients
            .AsNoTracking()
            .Include(i => i.Owners)
            .Where(i => i.BarId == bar.Id && i.VideoId == videoId)
            .FirstOrDefaultAsync(cancellationToken);

        if (ingredient is null)
        {
            ingredient = new Ingredient
            {
                Id = Guid.CreateVersion7(),
                Name = metadata.Title,
                ThumbnailUrl = metadata.ThumbnailUrl,
                Url = contract.Url,
                VideoId = videoId,
                Used = false,
                Selected = false,
                Revealed = false,
                BarId = bar.Id,
                Owners = []
            };
            dbContext.Ingredients.Add(ingredient);
        }
        
        if(ingredient.Owners!.Any(o => o.HipsterId == hipsterId))
            return Results.Conflict("You have already added this ingredient to the bar.");

        var owner = new HipstersSubmittedIngredient
        {
            HipsterId = hipsterId,
            IngredientId = ingredient.Id
        };
        dbContext.HipstersSubmittedIngredients.Add(owner);
        
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
            .Include(i => i.Owners)
            .Where(i => i.Id == id && i.Owners!.Any(o => o.HipsterId == hipsterId) && i.BarId == bar.Id)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (ingredient is null)
            return Results.NotFound("Ingredient not found.");
        
        if(ingredient.Owners!.Count == 1)
        {
            // Last owner - remove the ingredient
            dbContext.Ingredients.Remove(ingredient);
        }
        
        var owner = ingredient.Owners!.First(o => o.HipsterId == hipsterId);
        dbContext.HipstersSubmittedIngredients.Remove(owner);
        
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static IEndpointRouteBuilder RegisterGetUnusedIngredientCount(this IEndpointRouteBuilder builder)
    {
        builder.MapGet($"/{_routeBase}/unused-count", GetUnusedIngredientCount)
            .WithName(nameof(GetUnusedIngredientCount))
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> GetUnusedIngredientCount(
        [FromRoute] Guid franchiseId,
        [FromRoute] Guid barId,
        [FromServices] CoffeeTunesDbContext dbContext,
        [FromServices] FranchiseAccessService franchiseAccessService,
        CancellationToken cancellationToken)
    {
        await franchiseAccessService.EnsureAccessToFranchiseAsync(franchiseId, cancellationToken);
        
        var count = await dbContext.Ingredients.AsNoTracking()
            .Where(i => i.BarId == barId && !i.Used && !i.Selected)
            .CountAsync(cancellationToken);

        return Results.Ok(count);
    }

    private static IEndpointRouteBuilder RegisterGetIngredientContributors(this IEndpointRouteBuilder builder)
    {
        builder.MapGet($"/{_routeBase}/contributors", GetIngredientContributors)
            .WithName(nameof(GetIngredientContributors))
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> GetIngredientContributors(
        [FromRoute] Guid franchiseId,
        [FromRoute] Guid barId,
        [FromServices] CoffeeTunesDbContext dbContext,
        [FromServices] FranchiseAccessService franchiseAccessService,
        CancellationToken cancellationToken)
    {
        await franchiseAccessService.EnsureAccessToFranchiseAsync(franchiseId, cancellationToken);

        var allHipsters = await dbContext.HipstersInFranchises
            .Include(h => h.Hipster)
            .AsNoTracking()
            .Where(f => f.FranchiseId == franchiseId)
            .ToListAsync(cancellationToken);
        
        var allBarIngredients = await dbContext.Ingredients
            .Include(i => i.Owners)
            .AsNoTracking()
            .Where(i => i.BarId == barId)
            .ToListAsync(cancellationToken);

        var contracts = new List<IngredientContributorContract>();
        foreach (var hipster in allHipsters)
        {
            var alreadyContributed = allBarIngredients
                .Any(i => i.Owners.Any(o => o.HipsterId == hipster.HipsterId));

            var contract = new IngredientContributorContract
            {
                HipsterId = hipster.HipsterId,
                HipsterName = hipster.Hipster!.Name,
                AlreadyContributed = alreadyContributed
            };
            contracts.Add(contract);
        }
        
        return Results.Ok(contracts);
    }

    private static IEndpointRouteBuilder RegisterGetIngredientPlaylist(this IEndpointRouteBuilder builder)
    {
        builder.MapGet($"/{_routeBase}/playlist", GetIngredientPlaylist)
            .WithName(nameof(GetIngredientPlaylist))
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> GetIngredientPlaylist(
        [FromRoute] Guid franchiseId,
        [FromRoute] Guid barId,
        [FromServices] CoffeeTunesDbContext dbContext,
        [FromServices] FranchiseAccessService franchiseAccessService,
        CancellationToken cancellationToken)
    {
        await franchiseAccessService.EnsureAccessToFranchiseAsync(franchiseId, cancellationToken);

        var bar = await dbContext.Bars
            .AsNoTracking()
            .Where(b => b.Id == barId && b.FranchiseId == franchiseId)
            .FirstOrDefaultAsync(cancellationToken);
        if (bar is null)
            return Results.NotFound("Bar not found in the specified franchise.");
        
        if(bar.HasSupplyLeft)
            return Results.BadRequest("The ingredient playlist is only available for bars that have no supply left.");
        
        var allBarIngredients = await dbContext.Ingredients
            .AsNoTracking()
            .Where(i => i.BarId == barId)
            .Select(i => new IngredientPlaylistEntry
            {
                Id = i.Id,
                Name = i.Name,
                Url = i.Url,
                VideoId = i.VideoId,
                ThumbnailUrl = i.ThumbnailUrl
            })
            .ToListAsync(cancellationToken);
        
        return Results.Ok(allBarIngredients);
    }
}