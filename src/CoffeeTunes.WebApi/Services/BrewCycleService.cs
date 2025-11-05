using CoffeeTunes.Contracts.BrewCycles;
using CoffeeTunes.WebApi.Contexts;
using CoffeeTunes.WebApi.Mappers;
using Microsoft.EntityFrameworkCore;

namespace CoffeeTunes.WebApi.Services;

public class BrewCycleService(CoffeeTunesDbContext dbContext)
{
    private static readonly Random Random = new();
    
    public async Task<BrewCycleContract> StartNewCycleAsync(Guid barId, CancellationToken ct)
    {
        var count = await dbContext.Ingredients.AsNoTracking()
            .Where(i => i.BarId == barId && !i.Used)
            .CountAsync(ct);
        
        var selectedIndex = Random.Next(0, count);
        
        var ingredient = await dbContext.Ingredients
            .Include(i => i.Owners)
            .Where(i => i.BarId == barId && !i.Used)
            .Skip(selectedIndex)
            .FirstAsync(ct);

        ingredient.Selected = true;

        return ingredient.ToBrewCycleContract();
    }
}