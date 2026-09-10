using System.ComponentModel.DataAnnotations;

namespace Najda.Models;

// Statistics shown on the admin overview.
public class AdminDashboardViewModel
{
    public int TotalDonors { get; set; }
    public int ActiveDonors { get; set; }
    public int VerifiedDonors { get; set; }
    public int AvailableDonors { get; set; }

    public int ApprovedHospitals { get; set; }
    public int PendingHospitals { get; set; }
    public int SuspendedHospitals { get; set; }

    public int TotalRequests { get; set; }
    public int OpenRequests { get; set; }
    public int UrgentRequests { get; set; }
    public int FulfilledRequests { get; set; }

    public int TotalDonations { get; set; }
    public int UnitsProvided { get; set; }

    public int VerifiedPercent => TotalDonors == 0 ? 0 : (int)Math.Round(100.0 * VerifiedDonors / TotalDonors);

    // Blood type -> number of requests (demand chart)
    public List<(string BloodType, int Count)> DemandByType { get; set; } = new();

    // Blood type -> number of available donors (supply)
    public List<(string BloodType, int Count)> SupplyByType { get; set; } = new();

    public List<Hospital> PendingList { get; set; } = new();
    public List<BloodRequest> RecentRequests { get; set; } = new();
}

// Edit form for a hospital, including its contact person.
public class HospitalEditViewModel
{
    public int HospitalId { get; set; }

    [Required(ErrorMessage = "Hospital name is required.")]
    [StringLength(150)]
    [Display(Name = "Hospital name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email.")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Enter a valid phone number.")]
    public string? Phone { get; set; }

    [StringLength(50)]
    [Display(Name = "License number")]
    public string? LicenseNumber { get; set; }

    [Required(ErrorMessage = "City is required.")]
    public string City { get; set; } = string.Empty;

    public string? Area { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; } = "Pending";

    // --- contact person (platform <-> hospital liaison) ---
    [StringLength(120)]
    [Display(Name = "Contact name")]
    public string? ContactName { get; set; }

    [EmailAddress(ErrorMessage = "Enter a valid contact email.")]
    [Display(Name = "Contact email")]
    public string? ContactEmail { get; set; }

    [Phone(ErrorMessage = "Enter a valid contact phone.")]
    [Display(Name = "Contact phone")]
    public string? ContactPhone { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Appointment date")]
    public DateTime? ContactAppointedDate { get; set; }
}
