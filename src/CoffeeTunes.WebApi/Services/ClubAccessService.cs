using System.IdentityModel.Tokens.Jwt;
using CoffeeTunes.WebApi.Auth;
using CoffeeTunes.WebApi.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CoffeeTunes.WebApi.Services;

public class ClubAccessService(IHttpContextAccessor httpContextAccessor, JwtSecurityTokenHandler handler, AuthOptions authOptions,
    CoffeeTunesDbContext dbContext)
{
    public async Task EnsureAccessToClubAsync(Guid clubId, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext 
                          ?? throw new InvalidOperationException("No HttpContext available");
        var auth = httpContext.Request.Headers.Authorization.First()!;
        var token = handler.ReadJwtToken(auth[7..]);

        if (!Guid.TryParse(token.Subject, out Guid subject))
        {
            throw new InvalidOperationException("Invalid token");
        }
        
        var result = await dbContext.HipstersInClubs.AsNoTracking()
            .AnyAsync(hic => hic.ClubId == clubId && hic.HipsterId == subject, cancellationToken);

        if (!result)
            throw new InvalidOperationException("Club not found");
    }
    
    public (Guid Id, string Name)? GetHipsterInfoFromToken()
    {
        var httpContext = httpContextAccessor.HttpContext 
                          ?? throw new InvalidOperationException("No HttpContext available");
        var auth = httpContext.Request.Headers.Authorization.First()!;
        var token = handler.ReadJwtToken(auth[7..]);

        if (!Guid.TryParse(token.Subject, out Guid subject))
        {
            return null;
        }
        
        var name = token.Claims.First(c => c.Type == authOptions.NameClaimType).Value;
        return (subject, name);
    }
}