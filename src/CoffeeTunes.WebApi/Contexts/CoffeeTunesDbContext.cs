using CoffeeTunes.WebApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoffeeTunes.WebApi.Contexts;

public class CoffeeTunesDbContext : DbContext
{
    public CoffeeTunesDbContext(DbContextOptions<CoffeeTunesDbContext> options) : base(options)
    {
    }

    public DbSet<Bar> Bars { get; set; }
    public DbSet<Club> Clubs { get; set; }
    public DbSet<Hipster> Hipsters { get; set; }
    public DbSet<HipstersInClub> HipstersInClubs { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<Bean> Beans { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<HipstersInClub>()
            .HasKey(hic => new { hic.ClubId, hic.HipsterId });
    }
}