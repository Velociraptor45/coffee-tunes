namespace CoffeeTunes.Contracts.Franchise;

public class FranchiseHipsterStatisticsContract
{
    public required Guid HipsterId { get; set; }
    public required string HipsterName { get; set; }
    public required int TotalGuesses { get; set; }
    public required int CorrectGuesses { get; set; }
    public required int IngredientsSubmitted { get; set; }
}