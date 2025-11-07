using CoffeeTunes.Contracts.BrewCycles;
using CoffeeTunes.WebApi.Entities;

namespace CoffeeTunes.WebApi.Mappers;

public static class BrewCycleRevealContractMapper
{
    public static BrewCycleRevealContract ToBrewCycleRevealContract(this Ingredient ingredient)
    {
        var ownerHash = ingredient.Owners.Select(o => o.HipsterId).ToHashSet();
        return new BrewCycleRevealContract
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
    }
}