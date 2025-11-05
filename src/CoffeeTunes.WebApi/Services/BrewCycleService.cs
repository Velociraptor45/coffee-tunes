using CoffeeTunes.Contracts.BrewCycles;
using CoffeeTunes.WebApi.Contexts;
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
        
        var ingredient = await dbContext.Ingredients.AsNoTracking()
            .Where(i => i.BarId == barId && !i.Used)
            .Skip(selectedIndex)
            .FirstAsync(ct);

        return new BrewCycleContract
        {
            Ingredient = new BrewCycleIngredientContract
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                Url = ingredient.Url,
                ThumbnailUrl = ingredient.ThumbnailUrl
            }
        };
    }
}