namespace CoffeeTunes.Contracts.Beans;

public class CastBeansContract
{
    public required Guid IngredientId { get; set; }
    public required List<CastBeanContract> Beans { get; set; }
}