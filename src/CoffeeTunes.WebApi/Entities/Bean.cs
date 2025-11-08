using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeTunes.WebApi.Entities;

public class Bean
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required Guid Id { get; set; }

    public required Guid CastFromId { get; set; }
    public required Guid CastToId { get; set; }
    public required bool IsCorrect { get; set; }
    
    public required Guid IngredientId { get; set; }

    [ForeignKey(nameof(IngredientId))]
    public Ingredient? Ingredient { get; set; }

    [ForeignKey(nameof(CastFromId))]
    public Hipster? CastFrom { get; set; }
    
    [ForeignKey(nameof(CastToId))]
    public Hipster? CastTo { get; set; }
}