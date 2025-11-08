using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeTunes.WebApi.Entities;

public class BarStatistics
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required Guid Id { get; set; }
    
    public required Guid BarId { get; set; }
    public required Guid FranchiseId { get; set; }

    public required int IngredientsBrewedCount { get; set; }
    public required int BeansCastCount { get; set; }
    public required int CorrectCastsCount { get; set; }
    public required int TotalHipstersContributedCount { get; set; }
    
    [ForeignKey(nameof(BarId))]
    public Bar? Bar { get; set; }
    
    [ForeignKey(nameof(FranchiseId))]
    public Franchise? Franchise { get; set; }
    
    [InverseProperty(nameof(Entities.BarHipsterStatistics.BarStatistics))]
    public ICollection<BarHipsterStatistics> BarHipsterStatistics { get; set; } = [];
}