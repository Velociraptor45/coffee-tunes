using CoffeeTunes.Contracts.Bars;
using CoffeeTunes.WebApi.Contexts;
using CoffeeTunes.WebApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoffeeTunes.WebApi.Services;

public class BarService(CoffeeTunesDbContext dbContext)
{
    public async Task<BarContract> GetBarContractAsync(Guid barId, Guid franchiseId, CancellationToken ct)
    {
        var contract = await dbContext.Bars
            .AsNoTracking()
            .Where(b => b.FranchiseId == franchiseId && b.Id == barId)
            .Select(b => new BarContract
            {
                Id = b.Id,
                Topic = b.Topic,
                IsOpen = b.IsOpen,
                HasSupplyLeft = b.HasSupplyLeft,
                MaxIngredientsPerHipster = b.MaxIngredientsPerHipster,
                ContributingHipsters = new List<ContributingHipsterContract>()
            })
            .FirstAsync(ct);

        var contributingHipsters = await dbContext.Ingredients
            .AsNoTracking()
            .SelectMany(i => i.Owners!)
            .Join(
                dbContext.Hipsters,
                o => o.HipsterId,
                h => h.Id,
                (o, h) => new ContributingHipsterContract
                {
                    Id = h.Id,
                    Name = h.Name
                })
            .ToListAsync(ct);
        
        contract.ContributingHipsters = contributingHipsters.DistinctBy(h => h.Id).ToList();
        return contract;
    }
    
    public async Task<Bar> GetBarAsync(Guid barId, Guid franchiseId, CancellationToken ct)
    {
        return await dbContext.Bars
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