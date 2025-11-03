using CoffeeTunes.Contracts.Bars;
using CoffeeTunes.Contracts.Franchise;
using CoffeeTunes.Contracts.Ingredients;
using RestEase;

namespace CoffeeTunes.Contracts;

public interface ICoffeeTunesApi
{
    // Bar endpoints
    [Post("v1/{franchiseId}/bars")]
    Task<HttpResponseMessage> CreateBar([Path] Guid franchiseId, [Body] CreateBarContract contract);

    [Get("v1/{franchiseId}/bars/{id}")]
    Task<BarContract> GetBar([Path] Guid franchiseId, [Path] Guid id);

    [Get("v1/{franchiseId}/bars/all")]
    Task<BarOverviewContract> GetAllBars([Path] Guid franchiseId);

    [Post("v1/{franchiseId}/bars/{id}/ingredient")]
    Task AddIngredient([Path] Guid franchiseId, [Path] Guid id, [Body] AddIngredientContract contract);

    [Put("v1/{franchiseId}/bars/{id}/open")]
    Task OpenBar([Path] Guid franchiseId, [Path] Guid id);

    [Put("v1/{franchiseId}/bars/{id}/close")]
    Task CloseBar([Path] Guid franchiseId, [Path] Guid id);

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