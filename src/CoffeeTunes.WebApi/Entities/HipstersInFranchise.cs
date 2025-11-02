using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeTunes.WebApi.Entities;

public class HipstersInFranchise
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required Guid FranchiseId { get; set; }
    
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required Guid HipsterId { get; set; }
    
    [ForeignKey(nameof(FranchiseId))]
    public Franchise? Franchise { get; set; }
    
    [ForeignKey(nameof(HipsterId))]
    public Hipster? Hipster { get; set; }
}