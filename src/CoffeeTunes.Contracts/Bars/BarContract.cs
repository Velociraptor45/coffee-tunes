namespace CoffeeTunes.Contracts.Bars;

public class BarContract
{
    public required Guid Id { get; set; }
    public required string Topic { get; set; }
    public required bool IsOpen { get; set; }
    public required bool IsCurrentlyVisited { get; set; }
}