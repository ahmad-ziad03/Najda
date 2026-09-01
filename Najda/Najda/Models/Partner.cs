using System.ComponentModel.DataAnnotations;
using static Azure.Core.HttpHeader;

namespace Najda.Models;

// Commercial partner (restaurant, pharmacy) offering donor rewards.
public class Partner
{
    public int PartnerId { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    // Restaurant | Pharmacy | Other
    [Required, MaxLength(20)]
    public string PartnerType { get; set; } = "Other";

    [MaxLength(80)]
    public string? City { get; set; }

    // Subscription | Commission
    [Required, MaxLength(20)]
    public string BillingModel { get; set; } = "Commission";

    // Active | Paused
    [Required, MaxLength(10)]
    public string Status { get; set; } = "Active";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();
}