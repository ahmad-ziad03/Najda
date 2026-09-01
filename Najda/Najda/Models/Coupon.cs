using System.ComponentModel.DataAnnotations;

namespace Najda.Models;

public class Coupon
{
    public int CouponId { get; set; }

    public int PartnerId { get; set; }
    public Partner? Partner { get; set; }

    [Required, MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? DiscountText { get; set; }   // e.g. "15% off"

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<CouponRedemption> Redemptions { get; set; } = new List<CouponRedemption>();
}