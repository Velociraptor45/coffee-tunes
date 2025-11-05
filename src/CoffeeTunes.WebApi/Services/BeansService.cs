using CoffeeTunes.Contracts.Beans;
using CoffeeTunes.WebApi.Contexts;
using CoffeeTunes.WebApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoffeeTunes.WebApi.Services;

public class BeansService(CoffeeTunesDbContext dbContext, FranchiseAccessService accessService)
{
    public async Task EnsureIngredientIsCurrentlySelected(Guid ingredientId, CancellationToken ct)
    {
        var ingredient = await dbContext.Ingredients.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == ingredientId, ct);

        if (ingredient is not { Selected: true, Used: false })
        {
            throw new InvalidOperationException("The specified ingredient is not currently selected.");
        }
    }

    private async Task<List<HipstersSubmittedIngredient>> GetIngredientOwnersAsync(Guid ingredientId, CancellationToken ct)
    {
        var (hipsterId, _) = accessService.GetHipsterInfoFromToken()
            ?? throw new InvalidOperationException("Hipster information could not be retrieved from the access token.");
        return await dbContext.HipstersSubmittedIngredients.AsNoTracking()
            .Where(b => b.IngredientId == ingredientId && b.HipsterId != hipsterId)
            .ToListAsync(ct);
    }

    public async Task CreateBeansAsync(CastBeansContract contract, CancellationToken ct)
    {
        var (hipsterId, _) = accessService.GetHipsterInfoFromToken()
                             ?? throw new InvalidOperationException("Hipster information could not be retrieved from the access token.");
        var owners = await GetIngredientOwnersAsync(contract.IngredientId, ct);
        
        if(contract.Beans.Count > owners.Count)
            throw new InvalidOperationException("You're not allowed to cast more beans than there are ingredient owners.");
        
        foreach (var castBean in contract.Beans)
        {
            var bean = new Bean
            {
                Id = Guid.CreateVersion7(),
                CastFromId = hipsterId,
                CastToId = castBean.HipsterId,
                IngredientId = contract.IngredientId,
                IsCorrect = owners.Any(o => o.HipsterId == castBean.HipsterId)
            };
            await dbContext.Beans.AddAsync(bean, ct);
        }
        await dbContext.SaveChangesAsync(ct);
    }
}