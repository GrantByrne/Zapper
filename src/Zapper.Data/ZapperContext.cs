using Microsoft.EntityFrameworkCore;
using Zapper.Core.Models;

namespace Zapper.Data;

public class ZapperContext(DbContextOptions<ZapperContext> options) : DbContext(options)
{
    
    public DbSet<Device> Devices { get; set; }
    public DbSet<DeviceCommand> DeviceCommands { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<ActivityDevice> ActivityDevices { get; set; }
    public DbSet<ActivityStep> ActivitySteps { get; set; }
    public DbSet<IRCode> IRCodes { get; set; }
    public DbSet<IRCodeSet> IRCodeSets { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Device configuration
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => new { e.IpAddress, e.Port });
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.ConnectionType).HasConversion<string>();
        });
        
        // DeviceCommand configuration
        modelBuilder.Entity<DeviceCommand>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DeviceId, e.Name }).IsUnique();
            entity.Property(e => e.Type).HasConversion<string>();
            entity.HasOne(e => e.Device)
                .WithMany(e => e.Commands)
                .HasForeignKey(e => e.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Activity configuration
        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.SortOrder);
        });
        
        // ActivityDevice configuration
        modelBuilder.Entity<ActivityDevice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ActivityId, e.DeviceId }).IsUnique();
            entity.HasOne(e => e.Activity)
                .WithMany(e => e.ActivityDevices)
                .HasForeignKey(e => e.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Device)
                .WithMany(e => e.ActivityDevices)
                .HasForeignKey(e => e.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // ActivityStep configuration
        modelBuilder.Entity<ActivityStep>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ActivityId, e.StepOrder }).IsUnique();
            entity.HasOne(e => e.Activity)
                .WithMany(e => e.Steps)
                .HasForeignKey(e => e.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.DeviceCommand)
                .WithMany(e => e.ActivitySteps)
                .HasForeignKey(e => e.DeviceCommandId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // IRCode configuration
        modelBuilder.Entity<IRCode>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Brand, e.Model, e.CommandName });
            entity.Property(e => e.DeviceType).HasConversion<string>();
        });
        
        // IRCodeSet configuration
        modelBuilder.Entity<IRCodeSet>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Brand, e.Model }).IsUnique();
            entity.Property(e => e.DeviceType).HasConversion<string>();
            entity.HasMany(e => e.Codes)
                .WithOne()
                .HasForeignKey("IRCodeSetId")
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}