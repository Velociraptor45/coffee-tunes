namespace CoffeeTunes.Contracts.BrewCycles;

public class BrewCycleIngredientContract
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Url { get; set; }
    public required string VideoId { get; set; }
    public required string ThumbnailUrl { get; set; }
    public required IEnumerable<Guid> OwnerHipsterIds { get; set; }
}