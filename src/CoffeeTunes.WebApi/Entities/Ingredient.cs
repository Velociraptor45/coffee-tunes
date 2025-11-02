using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeTunes.WebApi.Entities;

public class Ingredient
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required Guid Id { get; set; }
    
    public required string Url { get; set; }
    public required string Name { get; set; }

    public required bool Used { get; set; }

    [InverseProperty(nameof(Bean.Ingredient))]
    public ICollection<Bean>? Beans { get; set; }
}