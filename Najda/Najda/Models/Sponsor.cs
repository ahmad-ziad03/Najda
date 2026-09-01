using System.ComponentModel.DataAnnotations;

namespace Najda.Models;

// Responsible sponsors / ads (pharma, insurance, labs).
public class Sponsor
{
    public int SponsorId { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    // Pharma | Insurance | Lab
    [Required, MaxLength(20)]
    public string Category { get; set; } = "Pharma";

    // Sponsor | Ad
    [Required, MaxLength(20)]
    public string PlacementType { get; set; } = "Sponsor";

    // Active | Paused
    [Required, MaxLength(10)]
    public string Status { get; set; } = "Active";

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}