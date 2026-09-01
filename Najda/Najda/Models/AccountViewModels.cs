using System.ComponentModel.DataAnnotations;

namespace Najda.Models;

public class RegisterViewModel
{
    // "Donor" | "Hospital"
    [Required]
    public string Role { get; set; } = "Donor";

    // Donor's full name OR hospital's (Arabic/primary) name.
    [Required(ErrorMessage = "This field is required.")]
    [StringLength(150)]
    [Display(Name = "Name")]
    public string FullName { get; set; } = string.Empty;

    // Hospital only: English name.
    [StringLength(150)]
    public string? HospitalNameEn { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email.")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Enter a valid phone number.")]
    public string? Phone { get; set; }

    // Donor only.
    public string? BloodType { get; set; }

    // Hospital only.
    [StringLength(50)]
    public string? LicenseNumber { get; set; }

    [Required(ErrorMessage = "City is required.")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public bool AgreeTerms { get; set; }
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email.")]
    public string Email { get; set; } = string.Empty;
}