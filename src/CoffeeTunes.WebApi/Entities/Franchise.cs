using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CoffeeTunes.WebApi.Entities;

[Index(nameof(Name), IsUnique = true)]
public class Franchise
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required Guid Id { get; set; }

    public required string Name { get; set; }
    public required string Code { get; set; }

    [InverseProperty(nameof(HipstersInFranchise.Franchise))]
    public ICollection<HipstersInFranchise> HipstersInFranchises { get; set; } = [];
}