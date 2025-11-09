namespace CoffeeTunes.Contracts.Ingredients;

public class IngredientContributorContract
{
    public required Guid HipsterId { get; set; }
    public required string HipsterName { get; set; }
    public required bool AlreadyContributed { get; set; }
}