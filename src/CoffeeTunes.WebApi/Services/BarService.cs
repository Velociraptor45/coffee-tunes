using CoffeeTunes.WebApi.Contexts;
using CoffeeTunes.WebApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoffeeTunes.WebApi.Services;

public class BarService(CoffeeTunesDbContext dbContext)
{
    public async Task<Bar> GetBarAsync(Guid barId, Guid franchiseId, CancellationToken ct)
    {
        return await dbContext.Bars
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == barId && b.FranchiseId == franchiseId, cancellationToken: ct)
               ?? throw new InvalidOperationException($"Bar not found in the specified franchise");
    }

    public async Task<Bar> GetInSupplyBar(Guid barId, Guid franchiseId, CancellationToken ct)
    {
        var bar = await GetBarAsync(barId, franchiseId, ct);
        if (!bar.HasSupplyLeft)
            throw new InvalidOperationException("The bar has no supply left.");

        return bar;
    }
}