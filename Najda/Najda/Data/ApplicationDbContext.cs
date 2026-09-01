using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Najda.Models;

namespace Najda.Data;
public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Each DbSet becomes a table. The property name is the table name,
    // so "Requests" here matches the [Table("Requests")] on BloodRequest.
    public DbSet<Donor> Donors => Set<Donor>();
    public DbSet<Hospital> Hospitals => Set<Hospital>();
    public DbSet<BloodRequest> Requests => Set<BloodRequest>();
    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<RequestMatch> RequestMatches => Set<RequestMatch>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CouponRedemption> CouponRedemptions => Set<CouponRedemption>();
    public DbSet<Sponsor> Sponsors => Set<Sponsor>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // keep Identity's own configuration

        // A donor's blood type is unique-ish but not a key; make sure emails are unique.
        builder.Entity<Donor>().HasIndex(d => d.Email).IsUnique();
        builder.Entity<Hospital>().HasIndex(h => h.Email).IsUnique();

        // Store money with a fixed precision (avoids EF warnings).
        builder.Entity<CouponRedemption>()
               .Property(r => r.CommissionAmount)
               .HasPrecision(6, 2);

        // Distance in km, one decimal place (e.g. 3.8) — clears the decimal warning.
        builder.Entity<RequestMatch>()
               .Property(m => m.DistanceKm)
               .HasPrecision(5, 1);

        // A donor appears at most once per request.
        builder.Entity<RequestMatch>()
               .HasIndex(m => new { m.RequestId, m.DonorId })
               .IsUnique();

        // ---- Delete behavior ----
        // SQL Server rejects "multiple cascade paths". Several tables point at both
        // Donor and Hospital, so we turn OFF cascade delete on those relationships
        // and let the rows be removed explicitly instead. This prevents the
        // migration error you'd otherwise hit.

        builder.Entity<Donor>()
               .HasOne(d => d.VerifiedByHospital)
               .WithMany()
               .HasForeignKey(d => d.VerifiedByHospitalId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Donation>()
               .HasOne(d => d.Hospital)
               .WithMany()
               .HasForeignKey(d => d.HospitalId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Donation>()
               .HasOne(d => d.Donor)
               .WithMany(d => d.Donations)
               .HasForeignKey(d => d.DonorId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Donation>()
               .HasOne(d => d.Request)
               .WithMany(r => r.Donations)
               .HasForeignKey(d => d.RequestId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<RequestMatch>()
               .HasOne(m => m.Donor)
               .WithMany(d => d.Matches)
               .HasForeignKey(m => m.DonorId)
               .OnDelete(DeleteBehavior.NoAction);
    }
}