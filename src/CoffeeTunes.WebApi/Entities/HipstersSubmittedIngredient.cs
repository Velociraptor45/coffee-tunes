using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeTunes.WebApi.Entities;

public class HipstersSubmittedIngredient
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required Guid HipsterId { get; set; }
    
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required Guid IngredientId { get; set; }
    
    [ForeignKey(nameof(HipsterId))]
    public Hipster? Hipster { get; set; }
    
    [ForeignKey(nameof(IngredientId))]
    public Ingredient? Ingredient { get; set; }
}