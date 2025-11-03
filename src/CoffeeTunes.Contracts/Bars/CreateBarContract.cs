namespace CoffeeTunes.Contracts.Bars;

public class CreateBarContract
{
    public required string Topic { get; set; }
    public required uint MaxIngredientsPerHipster { get; set; }
}