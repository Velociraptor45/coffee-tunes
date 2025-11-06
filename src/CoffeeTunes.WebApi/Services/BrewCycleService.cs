using CoffeeTunes.Contracts.Bars;
using CoffeeTunes.Contracts.BrewCycles;
using CoffeeTunes.WebApi.Contexts;
using CoffeeTunes.WebApi.Entities;
using CoffeeTunes.WebApi.Mappers;
using Microsoft.EntityFrameworkCore;

namespace CoffeeTunes.WebApi.Services;

public class BrewCycleService(CoffeeTunesDbContext dbContext, BarService barService)
{
    private static readonly Random Random = new();

    public async Task<BrewCycleContract> SelectNextIngredientAsync(Guid barId, Guid franchiseId, CancellationToken ct)
    {
        var bar = await dbContext.Bars
            .Where(b => b.Id == barId && b.FranchiseId == franchiseId)
            .FirstOrDefaultAsync(ct);

        if (bar is null)
            throw new InvalidOperationException("Bar not found in the specified franchise.");

        if (!bar.IsOpen)
            throw new InvalidOperationException("Cannot select next ingredient while the bar is closed.");

        var currentIngredient = await dbContext.Ingredients
            .Where(i => i.BarId == barId && i.Selected && !i.Used)
            .SingleAsync(ct);

        currentIngredient.Selected = false;
        currentIngredient.Used = true;

        await dbContext.SaveChangesAsync(ct);

        var ingredient = await SelectNextIngredient(barId, ct);
        await dbContext.SaveChangesAsync(ct);

        return ingredient.ToBrewCycleContract();
    }

    public async Task<BrewCycleContract> StartNewCycleAsync(Guid barId, CancellationToken ct)
    {
        var ingredient = await SelectNextIngredient(barId, ct);

        return ingredient.ToBrewCycleContract();
    }

    private async Task<Ingredient> SelectNextIngredient(Guid barId, CancellationToken ct)
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
        return ingredient;
    }

    public async Task<BarContract> EndCycleAsync(Guid barId, Guid franchiseId, CancellationToken ct)
    {
        var ingredient = await dbContext.Ingredients
            .Where(i => i.BarId == barId && i.Selected && !i.Used)
            .SingleAsync(ct);

        ingredient.Selected = false;
        ingredient.Used = true;

        var bar = await dbContext.Bars
            .Where(b => b.Id == barId && b.FranchiseId == franchiseId)
            .FirstOrDefaultAsync(ct);

        if (bar is null)
            throw new InvalidOperationException("Bar not found in the specified franchise.");

        bar.IsOpen = false;

        await dbContext.SaveChangesAsync(ct);

        return await barService.GetBarContractAsync(barId, franchiseId, ct);
    }

    public async Task<BrewCycleRevealContract> RevealIngredientAsync(Guid barId, Guid franchiseId, CancellationToken ct)
    {
        var bar = await barService.GetInSupplyBar(barId, franchiseId, ct);

        if (!bar.IsOpen)
            throw new InvalidOperationException("Cannot reveal ingredient while the bar is closed.");

        var ingredient = await dbContext.Ingredients
            .AsNoTracking()
            .Include(i => i.Beans)
            .ThenInclude(b => b.CastFrom)
            .Include(i => i.Beans)
            .ThenInclude(b => b.CastTo)
            .Include(i => i.Owners)
            .ThenInclude(o => o.Hipster)
            .Where(i => i.BarId == barId && i.Selected && !i.Used)
            .SingleAsync(ct);

        var ownerHash = ingredient.Owners.Select(o => o.HipsterId).ToHashSet();

        var contract = new BrewCycleRevealContract
        {
            IngredientId = ingredient.Id,
            Owners = ingredient.Owners
                .Select(o => new BrewCycleHipsterContract
                {
                    HipsterId = o.HipsterId,
                    Name = o.Hipster!.Name
                })
                .ToList(),
            Results = ingredient.Beans
                .GroupBy(b => b.CastFromId)
                .Select(bg => new BrewCycleBeanResultContract
                {
                    Hipster = new BrewCycleHipsterContract
                    {
                        HipsterId = bg.First().CastFromId,
                        Name = bg.First().CastFrom!.Name
                    },
                    Beans = bg
                        .Select(b => new BrewCycleCastBeanContract
                        {
                            HipsterCastFor = new BrewCycleHipsterContract
                            {
                                HipsterId = b.CastToId,
                                Name = b.CastTo!.Name
                            },
                            WasCorrect = ownerHash.Contains(b.CastToId)
                        })
                        .ToList()
                })
                .ToList()
        };

        return contract;
    }
}