using Microsoft.EntityFrameworkCore;
using Zapper.Core.Devices;
using Zapper.Core.Remote;

namespace Zapper.Core.Repository
{
    public class ZapperDbContext : DbContext
    {
        public DbSet<Device> Devices { get; set; }
        
        public DbSet<RemoteButton> RemoteButtons { get; set; }

        private string DbPath { get; }
        
        public ZapperDbContext()
        {
            Database.EnsureCreated();
            
            DbPath = "zapper.db";
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source=zapper.db");
    }
}