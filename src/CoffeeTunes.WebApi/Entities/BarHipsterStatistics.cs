using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeTunes.WebApi.Entities;

public class BarHipsterStatistics
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }
    
    public Guid BarStatisticsId { get; set; }
    public Guid BarId { get; set; }
    public Guid HipsterId { get; set; }

    public required int CorrectGuesses { get; set; }
    public required int TotalGuesses { get; set; }
    public required int IngredientsSubmitted { get; set; }

    [ForeignKey(nameof(BarStatisticsId))]
    public BarStatistics? BarStatistics { get; set; }
    
    [ForeignKey(nameof(BarId))]
    public Bar? Bar { get; set; }
    
    [ForeignKey(nameof(HipsterId))]
    public Hipster? Hipster { get; set; }
}