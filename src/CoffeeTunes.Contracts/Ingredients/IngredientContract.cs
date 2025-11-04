namespace CoffeeTunes.Contracts.Ingredients;

public class IngredientContract
{
    public required Guid Id { get; set; }
    /// <summary>
    /// The hipster that owns this ingredient
    /// </summary>
    public required Guid OwnerId { get; set; }
    public required string Url { get; set; }
    public required string Name { get; set; }
    public required bool Used { get; set; }
}