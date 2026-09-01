using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Najda.Models;

// Class is BloodRequest (so it doesn't clash with ASP.NET's own Request),
// but it maps to a table called "Requests".
[Table("Requests")]
public class BloodRequest
{
    [Key]
    public int RequestId { get; set; }

    public int HospitalId { get; set; }
    public Hospital? Hospital { get; set; }

    // The blood type the patient needs.
    [Required, MaxLength(3)]
    public string BloodType { get; set; } = string.Empty;

    public int UnitsNeeded { get; set; } = 1;
    public int UnitsConfirmed { get; set; } = 0;

    // Urgent | Normal  (kept separate from Status)
    [Required, MaxLength(10)]
    public string Priority { get; set; } = "Normal";

    // Open | Matched | Fulfilled | Cancelled
    [Required, MaxLength(10)]
    public string Status { get; set; } = "Open";

    [MaxLength(150)]
    public string? Department { get; set; }

    // English reason/department shown when the site is in English.
    [MaxLength(150)]
    public string? DepartmentEn { get; set; }

    public DateTime? NeededBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public ICollection<RequestMatch> Matches { get; set; } = new List<RequestMatch>();
    public ICollection<Donation> Donations { get; set; } = new List<Donation>();

    // --- Computed (not stored) : the badge the UI shows ---
    // "Urgent" only while still open; otherwise the lifecycle status.
    [NotMapped]
    public string DisplayStatus =>
        (Priority == "Urgent" && Status == "Open") ? "Urgent" : Status;
}