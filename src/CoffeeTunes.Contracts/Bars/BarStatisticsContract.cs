namespace CoffeeTunes.Contracts.Bars;

public class BarStatisticsContract
{
    public required int TotalIngredientsCount { get; set; }
    public required int IngredientsPlayedCount { get; set; }
    public required int BeansCastCount { get; set; }
    public required int CorrectCastsCount { get; set; }
    public required List<BarHipsterStatisticsContract> HipsterStatistics { get; set; }
}