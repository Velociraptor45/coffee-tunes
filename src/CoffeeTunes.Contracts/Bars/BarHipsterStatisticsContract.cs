namespace CoffeeTunes.Contracts.Bars;

public class BarHipsterStatisticsContract
{
    public required Guid HipsterId { get; set; }
    public required string HipsterName { get; set; }
    public required int TotalGuesses { get; set; }
    public required int CorrectGuesses { get; set; }
}