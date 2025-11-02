using System.IdentityModel.Tokens.Jwt;
using CoffeeTunes.Contracts.Clubs;
using CoffeeTunes.WebApi.Contexts;
using CoffeeTunes.WebApi.Entities;
using CoffeeTunes.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeTunes.WebApi.Endpoints;

public static class ClubEndpoints
{
    private const string _routeBase = "v1/clubs";
    
    public static void RegisterClubEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .RegisterCreateClub()
            .RegisterGetClub()
            .RegisterGetAllClubs();
    }

    private static IEndpointRouteBuilder RegisterCreateClub(this IEndpointRouteBuilder builder)
    {
        builder.MapPost($"/{_routeBase}", CreateClub)
            .WithName("CreateClub")
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> CreateClub(
        [FromBody] CreateClubContract contract,
        [FromServices] CoffeeTunesDbContext dbContext,
        [FromServices] ClubAccessService clubAccessService,
        CancellationToken cancellationToken)
    {
        var hipsterInfo = clubAccessService.GetHipsterInfoFromToken();
        if (hipsterInfo is null)
            return Results.BadRequest("Authentication failed");
        
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
        
        var club = new Club
        {
            Id = Guid.CreateVersion7(),
            Name = contract.Name
        };
        dbContext.Clubs.Add(club);
        
        var hipstersInClub = new HipstersInClub
        {
            ClubId = club.Id,
            HipsterId = hipsterInfo.Value.Id
        };
        dbContext.HipstersInClubs.Add(hipstersInClub);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return Results.CreatedAtRoute(nameof(GetClub), new { id = club.Id });
    }

    private static IEndpointRouteBuilder RegisterGetClub(this IEndpointRouteBuilder builder)
    {
        builder.MapGet($"/{_routeBase}/{{id:guid}}", GetClub)
            .WithName("GetClub")
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> GetClub(
        [FromRoute] Guid id,
        [FromServices] CoffeeTunesDbContext dbContext,
        [FromServices] ClubAccessService clubAccessService,
        CancellationToken cancellationToken)
    {
        await clubAccessService.EnsureAccessToClubAsync(id, cancellationToken);
        
        var club = await dbContext.Clubs
            .Where(c => c.Id == id)
            .Select(c => new ClubContract
            {
                Id = c.Id,
                Name = c.Name,
                HipstersInClub = c.HipstersInClubs!
                    .Select(hic => hic.Hipster!.Name)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return Results.Ok(club);
    }

    private static IEndpointRouteBuilder RegisterGetAllClubs(this IEndpointRouteBuilder builder)
    {
        builder.MapGet($"/{_routeBase}/all", GetAllClubs)
            .WithName("GetAllClubs")
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> GetAllClubs(
        [FromServices] CoffeeTunesDbContext dbContext,
        [FromServices] ClubAccessService clubAccessService,
        CancellationToken cancellationToken)
    {
        var (hipsterId, _) = clubAccessService.GetHipsterInfoFromToken() 
            ?? throw new InvalidOperationException("Authentication failed");
        
        var club = await dbContext.Clubs
            .Where(c => c.HipstersInClubs!.Any(hic => hic.HipsterId == hipsterId))
            .Select(c => new ClubOverviewContract
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(club);
    }
}