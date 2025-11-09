namespace CoffeeTunes.Contracts.Franchise;

public class FranchiseStatisticsContract
{
    public required int TotalIngredientsCount { get; set; }
    public required int BeansCastCount { get; set; }
    public required int CorrectCastsCount { get; set; }
    public required List<FranchiseHipsterStatisticsContract> HipsterStatistics { get; set; }
}