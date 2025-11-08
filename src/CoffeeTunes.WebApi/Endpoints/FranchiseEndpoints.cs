using CoffeeTunes.Contracts.Franchise;
using CoffeeTunes.WebApi.Contexts;
using CoffeeTunes.WebApi.Entities;
using CoffeeTunes.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeTunes.WebApi.Endpoints;

public static class FranchiseEndpoints
{
    private const string _routeBase = "v1/franchises";
    
    public static void RegisterFranchiseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .RegisterCreateFranchise()
            .RegisterGetFranchise()
            .RegisterGetAllFranchises()
            .RegisterJoinFranchise();
    }

    private static IEndpointRouteBuilder RegisterCreateFranchise(this IEndpointRouteBuilder builder)
    {
        builder.MapPost($"/{_routeBase}", CreateFranchise)
            .WithName(nameof(CreateFranchise))
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> CreateFranchise(
        [FromBody] CreateFranchiseContract contract,
        [FromServices] CoffeeTunesDbContext dbContext,
        [FromServices] FranchiseAccessService franchiseAccessService,
        CancellationToken cancellationToken)
    {
        var hipsterInfo = franchiseAccessService.GetHipsterInfoFromToken();
        if (hipsterInfo is null)
            return Results.BadRequest("Authentication failed");
        
        if(string.IsNullOrWhiteSpace(contract.Name))
            return Results.BadRequest("Franchise name cannot be empty");
        
        var trimmedName = contract.Name.Trim();
        
        var franchiseExists = await dbContext.Franchises
            .AnyAsync(c => c.Name == trimmedName, cancellationToken);
        if (franchiseExists)
            return Results.Conflict("Franchise with the same name already exists");
        
        var existingHipster = dbContext.Hipsters.FirstOrDefault(h => h.Id == hipsterInfo.Value.Id);
        if (existingHipster is null)
        {
            var newHipster = new Hipster
            {
                Id = hipsterInfo.Value.Id,
                Name = hipsterInfo.Value.Name
            };
            dbContext.Hipsters.Add(newHipster);
        }

        var franchiseId = Guid.CreateVersion7();
        var franchise = new Franchise
        {
            Id = franchiseId,
            Name = trimmedName,
            Code = FranchiseCodeService.Generate(franchiseId)
        };
        dbContext.Franchises.Add(franchise);
        
        var hipstersInFranchise = new HipstersInFranchise
        {
            FranchiseId = franchise.Id,
            HipsterId = hipsterInfo.Value.Id
        };
        dbContext.HipstersInFranchises.Add(hipstersInFranchise);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return Results.CreatedAtRoute(nameof(GetFranchise), new { id = franchise.Id });
    }

    private static IEndpointRouteBuilder RegisterGetFranchise(this IEndpointRouteBuilder builder)
    {
        builder.MapGet($"/{_routeBase}/{{id:guid}}", GetFranchise)
            .WithName(nameof(GetFranchise))
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> GetFranchise(
        [FromRoute] Guid id,
        [FromServices] CoffeeTunesDbContext dbContext,
        [FromServices] FranchiseAccessService franchiseAccessService,
        CancellationToken cancellationToken)
    {
        await franchiseAccessService.EnsureAccessToFranchiseAsync(id, cancellationToken);
        
        var franchise = await dbContext.Franchises
            .Where(c => c.Id == id)
            .Select(c => new FranchiseContract
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                HipstersInFranchise = c.HipstersInFranchises!
                    .Select(hic => hic.Hipster!.Name)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return Results.Ok(franchise);
    }

    private static IEndpointRouteBuilder RegisterGetAllFranchises(this IEndpointRouteBuilder builder)
    {
        builder.MapGet($"/{_routeBase}/all", GetAllFranchises)
            .WithName(nameof(GetAllFranchises))
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> GetAllFranchises(
        [FromServices] CoffeeTunesDbContext dbContext,
        [FromServices] FranchiseAccessService franchiseAccessService,
        CancellationToken cancellationToken)
    {
        var (hipsterId, _) = franchiseAccessService.GetHipsterInfoFromToken()
                             ?? throw new InvalidOperationException("Authentication failed");
        
        var franchise = await dbContext.Franchises
            .Where(c => c.HipstersInFranchises!.Any(hic => hic.HipsterId == hipsterId))
            .Select(c => new FranchiseOverviewContract
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(franchise);
    }

    private static IEndpointRouteBuilder RegisterJoinFranchise(this IEndpointRouteBuilder builder)
    {
        builder.MapPut($"/{_routeBase}/join", JoinFranchise)
            .WithName(nameof(JoinFranchise))
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> JoinFranchise(
        [FromBody] JoinFranchiseContract contract,
        [FromServices] CoffeeTunesDbContext dbContext,
        [FromServices] FranchiseAccessService franchiseAccessService,
        CancellationToken cancellationToken)
    {
        var (hipsterId, hipsterName) = franchiseAccessService.GetHipsterInfoFromToken()
            ?? throw new InvalidOperationException("Authentication failed");
        
        var franchise = await dbContext.Franchises
            .Include(c => c.HipstersInFranchises)
            .FirstOrDefaultAsync(c => c.Code == contract.Code, cancellationToken);
        
        if (franchise is null)
            return Results.NotFound("Franchise not found");

        var alreadyInFranchise = franchise.HipstersInFranchises!.Any(hic => hic.HipsterId == hipsterId);
        if (alreadyInFranchise)
            return Results.Ok();
        
        var existingHipster = dbContext.Hipsters.FirstOrDefault(h => h.Id == hipsterId);
        if (existingHipster is null)
        {
            var newHipster = new Hipster
            {
                Id = hipsterId,
                Name = hipsterName
            };
            dbContext.Hipsters.Add(newHipster);
        }
        
        var hipstersInFranchise = new HipstersInFranchise
        {
            FranchiseId = franchise.Id,
            HipsterId = hipsterId
        };
        dbContext.HipstersInFranchises.Add(hipstersInFranchise);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok();
    }
}