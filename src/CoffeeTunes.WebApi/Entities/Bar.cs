using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeTunes.WebApi.Entities;

public class Bar
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required Guid Id { get; set; }

    public required string Topic { get; set; }

    public required bool IsOpen { get; set; }

    public required uint MaxIngredientsPerHipster { get; set; }

    public required bool HasSupplyLeft { get; set; }
    
    public required Guid FranchiseId { get; set; }
    
    [ForeignKey(nameof(FranchiseId))]
    public Franchise? Franchise { get; set; }
    
    [InverseProperty(nameof(Ingredient.Bar))]
    public ICollection<Ingredient> Ingredients { get; set; } = [];
}