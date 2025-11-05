using CoffeeTunes.Contracts.BrewCycles;
using CoffeeTunes.WebApi.Entities;

namespace CoffeeTunes.WebApi.Mappers;

public static class BrewCycleContractMapper
{
    public static BrewCycleContract ToBrewCycleContract(this Ingredient ingredient)
    {
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