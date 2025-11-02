namespace CoffeeTunes.Contracts.Franchise;

public class FranchiseContract
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required IEnumerable<string> HipstersInFranchise { get; set; }
}