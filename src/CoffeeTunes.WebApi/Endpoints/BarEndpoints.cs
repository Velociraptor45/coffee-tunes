using System.Diagnostics.CodeAnalysis;
using CoffeeTunes.Contracts.Bars;
using CoffeeTunes.Contracts.Ingredients;
using CoffeeTunes.WebApi.Contexts;
using CoffeeTunes.WebApi.Entities;
using CoffeeTunes.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.EntityFrameworkCore;

namespace CoffeeTunes.WebApi.Endpoints;

public static class BarEndpoints
{
    [StringSyntax("Route")]
    private const string _routeBase = $"v1/{{franchiseId:guid}}/bars";
    
    public static void RegisterBarEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .RegisterCreateBar()
            .RegisterGetBar()
            .RegisterGetAllBars()
            .RegisterAddIngredient()
            .RegisterOpenBar()
            .RegisterCloseBar();
    }

    private static IEndpointRouteBuilder RegisterCreateBar(this IEndpointRouteBuilder builder)
    {
        builder.MapPost($"/{_routeBase}", CreateBar)
            .WithName(nameof(CreateBar))
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> CreateBar(
        [FromRoute] Guid franchiseId,
        [FromBody] CreateBarContract contract,
        [FromServices] CoffeeTunesDbContext dbContext,
        [FromServices] FranchiseAccessService franchiseAccessService,
        CancellationToken cancellationToken)
    {
        await franchiseAccessService.EnsureAccessToFranchiseAsync(franchiseId, cancellationToken);
        
        var existingBar = await dbContext.Bars
            .AsNoTracking()
            .AnyAsync(b => b.FranchiseId == franchiseId && b.Topic == contract.Topic, cancellationToken);
        
        if (existingBar)
            return Results.Conflict("A bar with the same topic already exists in this franchise.");
        
        var bar = new Bar
        {
            Id = Guid.CreateVersion7(),
            Topic = contract.Topic,
            IsOpen = false,
            HasSupplyLeft = true,
            FranchiseId = franchiseId,
            MaxIngredientsPerHipster = contract.MaxIngredientsPerHipster
        };
        dbContext.Bars.Add(bar);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return Results.CreatedAtRoute(nameof(GetBar), new { id = bar.Id });
    }

    private static IEndpointRouteBuilder RegisterGetAllBars(this IEndpointRouteBuilder builder)
    {
        builder.MapGet($"/{_routeBase}/all", GetAllBars)
            .WithName(nameof(GetAllBars))
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> GetAllBars(
        [FromRoute] Guid franchiseId,
        [FromServices] CoffeeTunesDbContext dbContext,
        [FromServices] FranchiseAccessService franchiseAccessService,
        CancellationToken cancellationToken)
    {
        await franchiseAccessService.EnsureAccessToFranchiseAsync(franchiseId, cancellationToken);
        
        var bars = await dbContext.Bars
            .AsNoTracking()
            .Where(b => b.FranchiseId == franchiseId)
            .Select(b => new BarOverviewContract
            {
                Id = b.Id,
                Topic = b.Topic,
                IsOpen = b.IsOpen,
                HasSupplyLeft = b.HasSupplyLeft
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(bars);
    }

    private static IEndpointRouteBuilder RegisterGetBar(this IEndpointRouteBuilder builder)
    {
        builder.MapGet($"/{_routeBase}/{{id:guid}}", GetBar)
            .WithName(nameof(GetBar))
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> GetBar(
        [FromRoute] Guid franchiseId,
        [FromRoute] Guid id,
        [FromServices] CoffeeTunesDbContext dbContext,
        [FromServices] FranchiseAccessService franchiseAccessService,
        CancellationToken cancellationToken)
    {
        await franchiseAccessService.EnsureAccessToFranchiseAsync(franchiseId, cancellationToken);
        
        var bar = await dbContext.Bars
            .AsNoTracking()
            .Where(b => b.FranchiseId == franchiseId && b.Id == id)
            .Select(b => new BarContract
            {
                Id = b.Id,
                Topic = b.Topic,
                IsOpen = b.IsOpen,
                HasSupplyLeft = b.HasSupplyLeft
            })
            .FirstOrDefaultAsync(cancellationToken);


        return Results.Ok(bar);
    }

    private static IEndpointRouteBuilder RegisterAddIngredient(this IEndpointRouteBuilder builder)
    {
        builder.MapPost($"/{_routeBase}/{{id:guid}}/ingredient", AddIngredient)
            .WithName(nameof(AddIngredient))
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> AddIngredient(
        [FromRoute] Guid franchiseId,
        [FromRoute] Guid id,
        [FromBody] AddIngredientContract contract,
        [FromServices] CoffeeTunesDbContext dbContext,
        [FromServices] FranchiseAccessService franchiseAccessService,
        CancellationToken cancellationToken)
    {
        await franchiseAccessService.EnsureAccessToFranchiseAsync(franchiseId, cancellationToken);
        var (hipsterId, _) = franchiseAccessService.GetHipsterInfoFromToken()
                             ?? throw new InvalidOperationException("Authentication failed");;
        
        var bar = await dbContext.Bars
            .AsNoTracking()
            .Where(b => b.FranchiseId == franchiseId && b.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
        if (bar is null)
            return Results.NotFound("Bar not found in the specified franchise.");
        if(!bar.HasSupplyLeft)
            return Results.BadRequest("The bar has no supply left - you can't add more ingredients.");
        
        // todo retrieve video info
        
        var ingredient = new Ingredient
        {
            Id = Guid.CreateVersion7(),
            Name = "My Cool YT video",
            Url = contract.Url,
            Used = false,
            OwnerId = hipsterId
        };
        dbContext.Ingredients.Add(ingredient);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static IEndpointRouteBuilder RegisterOpenBar(this IEndpointRouteBuilder builder)
    {
        builder.MapPut($"/{_routeBase}/{{id:guid}}/open", OpenBar)
            .WithName(nameof(OpenBar))
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> OpenBar(
        [FromRoute] Guid franchiseId,
        [FromRoute] Guid id,
        [FromServices] CoffeeTunesDbContext dbContext,
        [FromServices] FranchiseAccessService franchiseAccessService,
        CancellationToken cancellationToken)
    {
        await franchiseAccessService.EnsureAccessToFranchiseAsync(franchiseId, cancellationToken);
        
        var bar = await dbContext.Bars
            .Where(b => b.FranchiseId == franchiseId && b.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
        if (bar is null)
            return Results.NotFound("Bar not found in the specified franchise.");
        if(!bar.HasSupplyLeft)
            return Results.BadRequest("The bar has no supply left - you can't open the bar.");
        
        bar.IsOpen = true;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static IEndpointRouteBuilder RegisterCloseBar(this IEndpointRouteBuilder builder)
    {
        builder.MapPut($"/{_routeBase}/{{id:guid}}/close", CloseBar)
            .WithName(nameof(CloseBar))
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> CloseBar(
        [FromRoute] Guid franchiseId,
        [FromRoute] Guid id,
        [FromServices] CoffeeTunesDbContext dbContext,
        [FromServices] FranchiseAccessService franchiseAccessService,
        CancellationToken cancellationToken)
    {
        await franchiseAccessService.EnsureAccessToFranchiseAsync(franchiseId, cancellationToken);
        
        var bar = await dbContext.Bars
            .Where(b => b.FranchiseId == franchiseId && b.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
        if (bar is null)
            return Results.NotFound("Bar not found in the specified franchise.");
        
        bar.IsOpen = false;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}