namespace CoffeeTunes.Contracts.Clubs;

public class ClubContract
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required IEnumerable<string> HipstersInClub { get; set; }
}