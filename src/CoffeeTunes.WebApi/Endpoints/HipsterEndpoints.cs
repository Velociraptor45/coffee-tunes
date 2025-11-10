using System.Diagnostics.CodeAnalysis;
using CoffeeTunes.WebApi.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeTunes.WebApi.Endpoints;

public static class HipsterEndpoints
{
    [StringSyntax("Route")]
    private const string _routeBase = $"v1/hipsters/{{id:guid}}";

    public static void RegisterHipsterEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .RegisterDeleteAccount();
    }

    private static IEndpointRouteBuilder RegisterDeleteAccount(this IEndpointRouteBuilder builder)
    {
        builder.MapDelete($"/{_routeBase}", DeleteAccount)
            .WithName(nameof(DeleteAccount))
            .RequireAuthorization("User");

        return builder;
    }
    
    private static async Task<IResult> DeleteAccount(
        [FromRoute] Guid id,
        [FromServices] CoffeeTunesDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var hipster = await dbContext.Hipsters.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
        
        if (hipster == null)
            return Results.NotFound("Hipster account not found.");
        
        dbContext.Hipsters.Remove(hipster);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return Results.NoContent();
    }
}