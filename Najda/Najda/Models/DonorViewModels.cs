using System.ComponentModel.DataAnnotations;

namespace Najda.Models;

public class DonorDashboardViewModel
{
    public Donor Donor { get; set; } = default!;
    public int TotalDonations { get; set; }
    public int MatchingOpenRequests { get; set; }
    public List<BloodRequest> Preview { get; set; } = new();
    // request ids this donor has already responded to
    public HashSet<int> RespondedIds { get; set; } = new();
}

public class DonorProfileViewModel
{
    public int DonorId { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(120)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email.")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Enter a valid phone number.")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "City is required.")]
    public string City { get; set; } = string.Empty;

    public string? Area { get; set; }

    public bool IsAvailable { get; set; }

    // shown read-only; only a hospital can change a verified type
    public string BloodType { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
}
