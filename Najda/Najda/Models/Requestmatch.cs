using System.ComponentModel.DataAnnotations;

namespace Najda.Models;

// Which donors were matched to a request and how they responded.
// Powers the hospital "matched donors" screen.
public class RequestMatch
{
    [Key]
    public int MatchId { get; set; }

    public int RequestId { get; set; }
    public BloodRequest? Request { get; set; }

    public int DonorId { get; set; }
    public Donor? Donor { get; set; }

    // Matched | Responded | OnWay | Donated | Declined
    [Required, MaxLength(12)]
    public string MatchState { get; set; } = "Matched";

    public decimal? DistanceKm { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}