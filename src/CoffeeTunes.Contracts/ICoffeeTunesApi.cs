using CoffeeTunes.Contracts.Bars;
using CoffeeTunes.Contracts.Beans;
using CoffeeTunes.Contracts.Franchise;
using CoffeeTunes.Contracts.Ingredients;
using RestEase;

namespace CoffeeTunes.Contracts;

public interface ICoffeeTunesApi
{
    // Brewing Cycle endpoints
    [Put("v1/{franchiseId}/bars/{barId}/open")]
    Task StartBrewCycle([Path] Guid franchiseId, [Path] Guid barId);

    [Put("v1/{franchiseId}/bars/{barId}/reveal")]
    Task RevealIngredientResults([Path] Guid franchiseId, [Path] Guid barId);

    [Put("v1/{franchiseId}/bars/{barId}/close")]
    Task EndBrewCycle([Path] Guid franchiseId, [Path] Guid barId);

    [Put("v1/{franchiseId}/bars/{barId}/next")]
    Task NextIngredient([Path] Guid franchiseId, [Path] Guid barId);
    
    // Beans endpoints
    [Post("v1/{franchiseId}/bars/{barId}/beans")]
    Task CastBeans([Path] Guid franchiseId, [Path] Guid barId, [Body] CastBeansContract contract);

    // Bar endpoints
    [Post("v1/{franchiseId}/bars")]
    Task<HttpResponseMessage> CreateBar([Path] Guid franchiseId, [Body] CreateBarContract contract);

    [Get("v1/{franchiseId}/bars/{id}")]
    Task<BarContract> GetBar([Path] Guid franchiseId, [Path] Guid id);

    [Get("v1/{franchiseId}/bars/all")]
    Task<List<BarOverviewContract>> GetAllBars([Path] Guid franchiseId);

    // Ingredient endpoints
    [Get("v1/{franchiseId}/bars/{barId}/ingredients")]
    Task<List<IngredientContract>> GetIngredients([Path] Guid franchiseId, [Path] Guid barId);
    
    [Get("v1/{franchiseId}/bars/{barId}/ingredients/unused-count")]
    Task<int> GetUnusedIngredientCount([Path] Guid franchiseId, [Path] Guid barId);

    [Post("v1/{franchiseId}/bars/{barId}/ingredients")]
    Task AddIngredient([Path] Guid franchiseId, [Path] Guid barId, [Body] AddIngredientContract contract);

    [Delete("v1/{franchiseId}/bars/{barId}/ingredients/{id}")]
    Task RemoveIngredient([Path] Guid franchiseId, [Path] Guid barId, [Path] Guid id);

    // Franchise endpoints
    [Post("v1/franchises")]
    Task<HttpResponseMessage> CreateFranchise([Body] CreateFranchiseContract contract);

    [Get("v1/franchises/{id}")]
    Task<FranchiseContract> GetFranchise([Path] Guid id);

    [Get("v1/franchises/all")]
    Task<List<FranchiseOverviewContract>> GetAllFranchises();

    [Put("v1/franchises/join")]
    Task JoinFranchise([Body] JoinFranchiseContract contract);
}