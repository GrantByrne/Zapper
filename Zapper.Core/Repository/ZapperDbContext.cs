using System;
using System.IO;
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
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = Path.Join(path, "blogging.db"); 
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }
}