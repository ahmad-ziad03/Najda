using System.ComponentModel.DataAnnotations;

namespace Najda.Models;

// One row each time a donor actually redeems a coupon (drives the commission).
public class CouponRedemption
{
    [Key]
    public int RedemptionId { get; set; }

    public int CouponId { get; set; }
    public Coupon? Coupon { get; set; }

    public int DonorId { get; set; }
    public Donor? Donor { get; set; }

    public DateTime RedeemedAt { get; set; } = DateTime.Now;

    public decimal CommissionAmount { get; set; } = 0m;
}