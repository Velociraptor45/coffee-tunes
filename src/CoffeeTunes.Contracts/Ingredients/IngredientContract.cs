namespace CoffeeTunes.Contracts.Ingredients;

public class IngredientContract
{
    public required Guid Id { get; set; }
    public required string Url { get; set; }
    public required string ThumbnailUrl { get; set; }
    public required string Name { get; set; }
    public required bool Used { get; set; }
}