using System.ComponentModel.DataAnnotations;

namespace Najda.Models;

public class Donation
{
    public int DonationId { get; set; }

    public int DonorId { get; set; }
    public Donor? Donor { get; set; }

    public int HospitalId { get; set; }
    public Hospital? Hospital { get; set; }

    // A donation may or may not be tied to a specific request.
    public int? RequestId { get; set; }
    public BloodRequest? Request { get; set; }

    // The lab-confirmed blood type at donation time.
    [Required, MaxLength(3)]
    public string BloodType { get; set; } = string.Empty;

    public DateTime DonationDate { get; set; } = DateTime.Today;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}