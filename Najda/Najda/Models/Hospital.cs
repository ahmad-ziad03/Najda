using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Najda.Models;

public class Hospital
{
    public int HospitalId { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    // English name (proper noun) shown when the site is in English.
    [MaxLength(150)]
    public string? NameEn { get; set; }

    [Required, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(50)]
    public string? LicenseNumber { get; set; }

    [Required, MaxLength(80)]
    public string City { get; set; } = string.Empty;

    // English city name shown when the site is in English.
    [MaxLength(80)]
    public string? CityEn { get; set; }

    [MaxLength(80)]
    public string? Area { get; set; }

    // --- Contact person: the platform <-> hospital liaison ---
    [MaxLength(120)]
    public string? ContactName { get; set; }

    [MaxLength(256)]
    public string? ContactEmail { get; set; }

    [MaxLength(30)]
    public string? ContactPhone { get; set; }

    public DateTime? ContactAppointedDate { get; set; }

    // Pending | Approved | Suspended
    [Required, MaxLength(10)]
    public string Status { get; set; } = "Pending";

    public DateTime? ApprovedDate { get; set; }

    // Link to the ASP.NET Identity account (AspNetUsers.Id).
    [MaxLength(450)]
    public string? UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public ICollection<BloodRequest> Requests { get; set; } = new List<BloodRequest>();
}