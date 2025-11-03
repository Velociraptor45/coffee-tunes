namespace CoffeeTunes.Contracts.Bars;

public class BarOverviewContract
{
    public required Guid Id { get; set; }
    public required string Topic { get; set; }
    public required bool IsOpen { get; set; }
    public required bool HasSupplyLeft { get; set; }
}