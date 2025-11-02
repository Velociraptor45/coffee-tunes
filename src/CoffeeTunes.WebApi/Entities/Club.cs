using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CoffeeTunes.WebApi.Entities;

[Index(nameof(Name), IsUnique = true)]
public class Club
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    [InverseProperty(nameof(HipstersInClub.Club))]
    public ICollection<HipstersInClub>? HipstersInClubs { get; set; }
}