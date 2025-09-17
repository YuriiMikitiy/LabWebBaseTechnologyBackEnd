using LabWebBaseTechnologyBackEnd.DataAccess.ModulEntity;
using Microsoft.EntityFrameworkCore;

namespace LabWebBaseTechnologyBackEnd.DataAccess;

public class LabWebBaseTechnologyDBContext : DbContext
{
    public LabWebBaseTechnologyDBContext(DbContextOptions<LabWebBaseTechnologyDBContext> option):base(option)
    {

    }
        public DbSet<FlightEntity> Flights { get; set; }
        public DbSet<BookingEntity> Bookings { get; set; }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<PaymentEntity> Payments { get; set; }
        public DbSet<FlightDelayDataEntity> FlightDelayData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Flight -> Bookings (One-to-Many)
            modelBuilder.Entity<BookingEntity>()
                .HasOne(b => b.Flight)
                .WithMany(f => f.Bookings)
                .HasForeignKey(b => b.FlightId)
                .OnDelete(DeleteBehavior.Cascade);

            // Booking -> Payment (One-to-One)
            modelBuilder.Entity<PaymentEntity>()
                .HasOne(p => p.Booking)
                .WithOne(b => b.Payment)
                .HasForeignKey<PaymentEntity>(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Bookings (One-to-Many)
            modelBuilder.Entity<BookingEntity>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            

            // Flight -> FlightDelayData (One-to-Many)
            modelBuilder.Entity<FlightDelayDataEntity>()
                .HasOne(d => d.Flight)
                .WithMany(f => f.DelayData)
                .HasForeignKey(d => d.FlightId)
                .OnDelete(DeleteBehavior.Cascade);

           
        }
}
