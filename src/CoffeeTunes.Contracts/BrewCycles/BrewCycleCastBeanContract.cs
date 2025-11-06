namespace CoffeeTunes.Contracts.BrewCycles;

public class BrewCycleCastBeanContract
{
    /// <summary>
    /// This is the Hipster for whom the beans were cast
    /// </summary>
    public required BrewCycleHipsterContract HipsterCastFor { get; set; }

    public required bool WasCorrect { get; set; }
}