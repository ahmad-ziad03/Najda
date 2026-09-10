using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Najda.Models;

public class Donor
{
    public int DonorId { get; set; }

    [Required, MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Phone { get; set; }

    // Entered by the donor — treated as preliminary until a hospital confirms it.
    [Required, MaxLength(3)]
    public string BloodType { get; set; } = string.Empty;

    public bool IsVerified { get; set; } = false;

    // Which hospital confirmed the type, and when.
    public int? VerifiedByHospitalId { get; set; }
    public Hospital? VerifiedByHospital { get; set; }
    public DateTime? VerifiedDate { get; set; }

    [Required, MaxLength(80)]
    public string City { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? Area { get; set; }

    public bool IsAvailable { get; set; } = true;

    // Set by an administrator. A deactivated donor is excluded from matching
    // and cannot sign in. Reversible — reactivating restores the account.
    public bool IsActive { get; set; } = true;

    public DateTime? LastDonationDate { get; set; }

    // Link to the ASP.NET Identity account (AspNetUsers.Id).
    [MaxLength(450)]
    public string? UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
    public ICollection<RequestMatch> Matches { get; set; } = new List<RequestMatch>();

    // --- Computed (not stored) : the 56-day eligibility rule ---
    [NotMapped]
    public DateTime? NextEligibleDate =>
        LastDonationDate.HasValue ? LastDonationDate.Value.AddDays(56) : null;

    [NotMapped]
    public bool IsEligibleNow =>
        LastDonationDate is null || DateTime.Today >= LastDonationDate.Value.AddDays(56);
}
