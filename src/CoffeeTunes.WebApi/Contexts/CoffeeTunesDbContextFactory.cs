using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CoffeeTunes.WebApi.Contexts;

public class CoffeeTunesDbContextFactory: IDesignTimeDbContextFactory<CoffeeTunesDbContext>
{
    public CoffeeTunesDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CoffeeTunesDbContext>();
        optionsBuilder.UseNpgsql();

        return new CoffeeTunesDbContext(optionsBuilder.Options);
    }
}