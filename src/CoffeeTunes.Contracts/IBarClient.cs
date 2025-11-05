using CoffeeTunes.Contracts.BrewCycles;

namespace CoffeeTunes.Contracts;

public interface IBarClient
{
    Task BrewCycleUpdated(BrewCycleContract brewCycleContract);
}