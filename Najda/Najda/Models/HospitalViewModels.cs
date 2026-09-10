using System.ComponentModel.DataAnnotations;

namespace Najda.Models;

public class HospitalDashboardViewModel
{
    public Hospital Hospital { get; set; } = default!;

    public int OpenRequests { get; set; }
    public int UrgentRequests { get; set; }
    public int MatchedRequests { get; set; }
    public int FulfilledRequests { get; set; }
    public int TotalRequests { get; set; }

    // available donors near the hospital, by blood type
    public List<(string BloodType, int Count)> SupplyByType { get; set; } = new();

    public List<BloodRequest> RecentRequests { get; set; } = new();
}

// Used for both posting a new request and editing an existing one.
public class RequestFormViewModel
{
    public int RequestId { get; set; } // 0 = new

    [Required(ErrorMessage = "Please choose the blood type.")]
    [Display(Name = "Blood type")]
    public string BloodType { get; set; } = string.Empty;

    [Range(1, 50, ErrorMessage = "Units must be between 1 and 50.")]
    [Display(Name = "Units needed")]
    public int UnitsNeeded { get; set; } = 1;

    [Required(ErrorMessage = "Please choose a priority.")]
    public string Priority { get; set; } = "Urgent"; // Urgent | Normal

    [Display(Name = "Department / reason")]
    [StringLength(150)]
    public string? Department { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Needed by")]
    public DateTime? NeededBy { get; set; }
}
