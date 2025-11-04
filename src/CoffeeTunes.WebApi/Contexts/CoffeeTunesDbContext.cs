using CoffeeTunes.WebApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoffeeTunes.WebApi.Contexts;

public class CoffeeTunesDbContext : DbContext
{
    public CoffeeTunesDbContext(DbContextOptions<CoffeeTunesDbContext> options) : base(options)
    {
    }

    public DbSet<Bar> Bars { get; set; }
    public DbSet<Franchise> Franchises { get; set; }
    public DbSet<Hipster> Hipsters { get; set; }
    public DbSet<HipstersInFranchise> HipstersInFranchises { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<HipstersSubmittedIngredient> HipstersSubmittedIngredients { get; set; }
    public DbSet<Bean> Beans { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<HipstersInFranchise>()
            .HasKey(hic => new { hic.FranchiseId, hic.HipsterId });
        
        modelBuilder.Entity<HipstersSubmittedIngredient>()
            .HasKey(hsi => new { hsi.HipsterId, hsi.IngredientId });
    }
}