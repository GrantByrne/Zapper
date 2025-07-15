using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Zapper.Data;

public class ZapperContextFactory : IDesignTimeDbContextFactory<ZapperContext>
{
    public ZapperContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ZapperContext>();
        optionsBuilder.UseSqlite("Data Source=zapper.db");

        return new ZapperContext(optionsBuilder.Options);
    }
}