namespace CoffeeTunes.Contracts.BrewCycles;

public class BrewCycleRevealContract
{
    public required Guid IngredientId { get; set; }
    
    /// <summary>
    /// The hipsters who submitted this ingredient
    /// </summary>
    public required List<BrewCycleHipsterContract> Owners { get; set; }

    /// <summary>
    /// The actual beans that were cast for this ingredient
    /// </summary>
    public required List<BrewCycleBeanResultContract> Results { get; set; }
}