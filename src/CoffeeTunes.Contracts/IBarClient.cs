using CoffeeTunes.Contracts.Bars;
using CoffeeTunes.Contracts.BrewCycles;

namespace CoffeeTunes.Contracts;

public interface IBarClient
{
    Task BarUpdated(BarContract barContract);
    Task BrewCycleUpdated(BrewCycleContract brewCycleContract);
}