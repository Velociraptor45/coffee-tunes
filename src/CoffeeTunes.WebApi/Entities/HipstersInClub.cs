using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeTunes.WebApi.Entities;

public class HipstersInClub
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required Guid ClubId { get; set; }
    
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required Guid HipsterId { get; set; }
    
    [ForeignKey(nameof(ClubId))]
    public Club? Club { get; set; }
    
    [ForeignKey(nameof(HipsterId))]
    public Hipster? Hipster { get; set; }
}