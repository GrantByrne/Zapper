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
    public DbSet<IrCode> IrCodes { get; set; }
    public DbSet<IrCodeSet> IrCodeSets { get; set; }
    public DbSet<ExternalIrCodeCache> ExternalIrCodeCache { get; set; }
    public DbSet<UsbRemote> UsbRemotes { get; set; }
    public DbSet<UsbRemoteButton> UsbRemoteButtons { get; set; }
    public DbSet<UsbRemoteButtonMapping> UsbRemoteButtonMappings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => new { e.IpAddress, e.Port });
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.ConnectionType).HasConversion<string>();
        });

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

        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.SortOrder);
        });

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

        modelBuilder.Entity<IrCode>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Brand, e.Model, e.CommandName });
            entity.Property(e => e.DeviceType).HasConversion<string>();
        });

        modelBuilder.Entity<IrCodeSet>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Brand, e.Model }).IsUnique();
            entity.Property(e => e.DeviceType).HasConversion<string>();
            entity.HasMany(e => e.Codes)
                .WithOne()
                .HasForeignKey("IRCodeSetId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExternalIrCodeCache>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CacheKey).IsUnique();
            entity.HasIndex(e => e.ExpiresAt);
        });

        modelBuilder.Entity<UsbRemote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DeviceId).IsUnique();
            entity.HasIndex(e => new { e.VendorId, e.ProductId, e.SerialNumber }).IsUnique();
        });

        modelBuilder.Entity<UsbRemoteButton>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RemoteId, e.KeyCode }).IsUnique();
            entity.HasOne(e => e.Remote)
                .WithMany(e => e.Buttons)
                .HasForeignKey(e => e.RemoteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UsbRemoteButtonMapping>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ButtonId, e.DeviceId, e.EventType }).IsUnique();
            entity.Property(e => e.EventType).HasConversion<string>();
            entity.HasOne(e => e.Button)
                .WithMany(e => e.Mappings)
                .HasForeignKey(e => e.ButtonId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Device)
                .WithMany()
                .HasForeignKey(e => e.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.DeviceCommand)
                .WithMany()
                .HasForeignKey(e => e.DeviceCommandId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}