namespace CoffeeTunes.Contracts.Ingredients;

public class IngredientPlaylistEntry
{
    public required Guid Id { get; set; }
    public required string Url { get; set; }
    public required string VideoId { get; set; }
    public required string Name { get; set; }
    public required string ThumbnailUrl { get; set; }
}