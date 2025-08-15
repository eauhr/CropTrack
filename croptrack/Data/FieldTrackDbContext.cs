using CropTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace CropTrack.Data
{
    public class FieldDbTrackContext : DbContext
    {
        public FieldDbTrackContext(DbContextOptions<FieldDbTrackContext> options)
            : base(options) { }

        public DbSet<Crop> Crops { get; set; }
        public DbSet<FieldCrop> FieldCrops { get; set; }
        public DbSet<MarketPrice> MarketPrices { get; set; }
        public DbSet<Field> Fields { get; set; }
        public DbSet<Farmer> Farmers { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<WeatherLog> WeatherLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=fieldtrack;Integrated Security=True;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FieldCrop>()
                .HasOne(fc => fc.Field)
                .WithMany(f => f.FieldCrops)
                .HasForeignKey(fc => fc.FieldId);

            modelBuilder.Entity<FieldCrop>()
                .HasOne(fc => fc.Crop)
                .WithMany(c => c.FieldCrops)
                .HasForeignKey(fc => fc.CropId);

            modelBuilder.Entity<MarketPrice>()
                .HasOne(mp => mp.Crop)
                .WithMany(c => c.MarketPrices)
                .HasForeignKey(mp => mp.CropId);

            modelBuilder.Entity<Field>()
                .HasOne(f => f.Farmer)
                .WithMany(fm => fm.Fields)
                .HasForeignKey(f => f.FarmerId);

            modelBuilder.Entity<Field>()
                .HasOne(f => f.Region)
                .WithMany(r => r.Fields)
                .HasForeignKey(f => f.RegionId);

            modelBuilder.Entity<WeatherLog>()
                .HasOne(wl => wl.Region)
                .WithMany(r => r.WeatherLogs)
                .HasForeignKey(wl => wl.RegionId);
        }
    }
}
