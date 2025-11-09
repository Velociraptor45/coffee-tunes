using CoffeeTunes.Contracts.Bars;
using CoffeeTunes.WebApi.Contexts;
using CoffeeTunes.WebApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoffeeTunes.WebApi.Services;

public class BarStatisticsService(CoffeeTunesDbContext dbContext)
{
    public async Task<BarStatisticsContract> GetBarStatisticsAsync(Guid barId, Guid franchiseId, CancellationToken ct)
    {
        var franchise = await dbContext.Franchises
            .AsNoTracking()
            .Include(f => f.HipstersInFranchises)
            .ThenInclude(f => f.Hipster)
            .FirstAsync(f => f.Id == franchiseId, cancellationToken: ct);
        var bar = await dbContext.Bars
            .AsNoTracking()
            .Include(b => b.Ingredients)
            .ThenInclude(i => i.Beans)
            .Include(b => b.Ingredients)
            .ThenInclude(i => i.Owners)
            .FirstOrDefaultAsync(b => b.Id == barId && b.FranchiseId == franchiseId, cancellationToken: ct);
        if (bar is null)
            throw new InvalidOperationException("Bar not found in the specified franchise");
        
        var beans = bar.Ingredients
            .SelectMany(i => i.Beans)
            .ToList();
        
        var hipsterStats = new List<BarHipsterStatisticsContract>();
        foreach (var hipster in franchise.HipstersInFranchises)
        {
            var hipsterBeans = beans
                .Where(b => b.CastFromId == hipster.HipsterId)
                .ToList();
            var hipsterStat = new BarHipsterStatisticsContract
            {
                HipsterId = hipster.HipsterId,
                CorrectGuesses = hipsterBeans.Count(b => b.IsCorrect),
                TotalGuesses = hipsterBeans.Count,
                HipsterName = hipster.Hipster!.Name
            };
            hipsterStats.Add(hipsterStat);
        }

        return new BarStatisticsContract
        {
            BeansCastCount = beans.Count,
            CorrectCastsCount = beans.Count(b => b.IsCorrect),
            IngredientsPlayedCount = bar.Ingredients.Count(i => i.Used),
            TotalIngredientsCount = bar.Ingredients.Count,
            HipsterStatistics = hipsterStats
        };
    }
    
    public async Task CreateFinalBarStatisticsAsync(Guid barId, Guid franchiseId, CancellationToken ct)
    {
        var franchise = await dbContext.Franchises
            .AsNoTracking()
            .Include(f => f.HipstersInFranchises)
            .FirstAsync(f => f.Id == franchiseId, cancellationToken: ct);
        
        var bar = await dbContext.Bars
            .AsNoTracking()
            .Include(b => b.Ingredients)
            .ThenInclude(i => i.Beans)
            .Include(b => b.Ingredients)
            .ThenInclude(i => i.Owners)
            .FirstOrDefaultAsync(b => b.Id == barId && b.FranchiseId == franchiseId, cancellationToken: ct);
        if (bar is null)
            throw new InvalidOperationException("Bar not found in the specified franchise");

        var beans = bar.Ingredients
            .SelectMany(i => i.Beans)
            .ToList();
        
        var hipsterStats = new List<BarHipsterStatistics>();
        foreach (var hipster in franchise.HipstersInFranchises)
        {
            var hipsterBeans = beans
                .Where(b => b.CastFromId == hipster.HipsterId)
                .ToList();
            var hipsterStat = new BarHipsterStatistics
            {
                Id = Guid.CreateVersion7(),
                BarId = bar.Id,
                HipsterId = hipster.HipsterId,
                CorrectGuesses = hipsterBeans.Count(b => b.IsCorrect),
                TotalGuesses = hipsterBeans.Count,
                IngredientsSubmitted = bar.Ingredients.Count(i => i.Owners!.Any(o => o.HipsterId == hipster.HipsterId))
            };
            hipsterStats.Add(hipsterStat);
        }

        var stats = new BarStatistics
        {
            Id = Guid.CreateVersion7(),
            BarId = bar.Id,
            FranchiseId = franchiseId,
            BeansCastCount = beans.Count,
            IngredientsBrewedCount = bar.Ingredients.Count,
            CorrectCastsCount = beans.Count(b => b.IsCorrect),
            TotalHipstersContributedCount = bar.Ingredients
                .SelectMany(i => i.Owners)
                .Select(h => h.HipsterId)
                .Distinct()
                .Count(),
            BarHipsterStatistics = hipsterStats
        };
        await dbContext.BarStatistics.AddAsync(stats, ct);
    }
}