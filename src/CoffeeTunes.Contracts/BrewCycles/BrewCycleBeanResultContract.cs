namespace CoffeeTunes.Contracts.BrewCycles;

public class BrewCycleBeanResultContract
{
    /// <summary>
    /// This is the Hipster who cast the beans
    /// </summary>
    public required BrewCycleHipsterContract Hipster { get; set; }

    /// <summary>
    /// These are the beans, the hipster cast
    /// </summary>
    public required List<BrewCycleCastBeanContract> Beans { get; set; }
}