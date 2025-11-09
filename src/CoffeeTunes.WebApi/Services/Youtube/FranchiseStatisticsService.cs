using CoffeeTunes.Contracts.Franchise;
using CoffeeTunes.WebApi.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CoffeeTunes.WebApi.Services.Youtube;

public class FranchiseStatisticsService(CoffeeTunesDbContext dbContext)
{
    public async Task<FranchiseStatisticsContract> GetFranchiseStatistics(Guid franchiseId, CancellationToken ct)
    {
        var hipstersInFranchises = await dbContext.HipstersInFranchises
            .AsNoTracking()
            .Include(hf => hf.Hipster)
            .Where(f => f.FranchiseId == franchiseId)
            .ToListAsync(cancellationToken: ct);
        var barStats = await dbContext.BarStatistics
            .AsNoTracking()
            .Include(b => b.BarHipsterStatistics)
            .Where(b => b.FranchiseId == franchiseId)
            .ToListAsync(ct);

        var barHipsterStats = barStats.SelectMany(b => b.BarHipsterStatistics)
            .ToLookup(bhs => bhs.HipsterId, bhs => bhs);
        
        var hipsters = new List<FranchiseHipsterStatisticsContract>();
        foreach (var hipsterInFranchise in hipstersInFranchises)
        {
            var hipsterStats = new FranchiseHipsterStatisticsContract
            {
                HipsterId = hipsterInFranchise.HipsterId,
                HipsterName = hipsterInFranchise.Hipster!.Name,
                CorrectGuesses = barHipsterStats.Contains(hipsterInFranchise.HipsterId)
                    ? barHipsterStats[hipsterInFranchise.HipsterId].Sum(bhs => bhs.CorrectGuesses)
                    : 0,
                TotalGuesses = barHipsterStats.Contains(hipsterInFranchise.HipsterId)
                    ? barHipsterStats[hipsterInFranchise.HipsterId].Sum(bhs => bhs.TotalGuesses)
                    : 0,
                IngredientsSubmitted = barHipsterStats.Contains(hipsterInFranchise.HipsterId)
                    ? barHipsterStats[hipsterInFranchise.HipsterId].Sum(bhs => bhs.IngredientsSubmitted)
                    : 0
            };
            hipsters.Add(hipsterStats);
        }
        
        return new FranchiseStatisticsContract
        {
            TotalIngredientsCount = barStats.Sum(b => b.IngredientsBrewedCount),
            BeansCastCount = barStats.Sum(b => b.BeansCastCount),
            CorrectCastsCount = barStats.Sum(b => b.CorrectCastsCount),
            HipsterStatistics = hipsters
        };
    }
}