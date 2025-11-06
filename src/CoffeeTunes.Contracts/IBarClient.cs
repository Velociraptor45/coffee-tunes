using CoffeeTunes.Contracts.Bars;
using CoffeeTunes.Contracts.Beans;
using CoffeeTunes.Contracts.BrewCycles;
using CoffeeTunes.Contracts.Hipsters;

namespace CoffeeTunes.Contracts;

public interface IBarClient
{
    Task BarUpdated(BarContract barContract);
    Task BrewCycleUpdated(BrewCycleContract brewCycleContract);
    Task BeanCast(BeanCastContract beanCastContract);
    Task HipsterJoined(HipsterJoinedContract hipsterJoinedContract);
    Task HipsterLeft(Guid hipsterId);
}