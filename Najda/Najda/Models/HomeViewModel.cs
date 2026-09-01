namespace Najda.Models;

// Carries the home page's live requests plus real counts for the stat bar,
// so the landing numbers always reflect the actual data (never invented).
public class HomeViewModel
{
    public int TotalDonors { get; set; }
    public int AvailableDonors { get; set; }
    public int ApprovedHospitals { get; set; }
    public int UnitsProvided { get; set; }   // sum of confirmed units across requests
    public List<BloodRequest> LiveRequests { get; set; } = new();
}