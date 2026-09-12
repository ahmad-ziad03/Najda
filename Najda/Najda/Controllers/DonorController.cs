using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Najda.Data;
using Najda.Helpers;
using Najda.Models;

namespace Najda.Controllers;

[Authorize(Roles = "Donor")]
public class DonorController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _users;

    public DonorController(ApplicationDbContext db, UserManager<IdentityUser> users)
    {
        _db = db;
        _users = users;
    }

    private void Toast(string message, string type = "success")
    {
        TempData["Toast"] = message;
        TempData["ToastType"] = type;
    }

    // call site (Chrome("key", "arabic", "english")) keeps working unchanged;
    // only the Arabic title is stored/displayed now.
    private void Chrome(string active, string titleAr, string titleEn = "")
    {
        ViewData["Area"] = "donor";
        ViewData["Active"] = active;
        ViewData["TitleAr"] = titleAr;
    }

    private async Task<Donor?> CurrentDonorAsync()
    {
        var userId = _users.GetUserId(User);
        return await _db.Donors.FirstOrDefaultAsync(d => d.UserId == userId);
    }

    // requests this donor is compatible to donate to, still open, nearest first
    private async Task<List<BloodRequest>> MatchingRequestsAsync(Donor donor)
    {
        var neededTypes = BloodCompatibility.NeededTypesFor(donor.BloodType);

        var requests = await _db.Requests
            .Include(r => r.Hospital)
            .Where(r => (r.Status == "Open" || r.Status == "Matched")
                        && neededTypes.Contains(r.BloodType))
            .ToListAsync();

        return requests
            .OrderBy(r => r.Priority == "Urgent" ? 0 : 1)
            .ThenByDescending(r => r.Hospital != null && r.Hospital.City == donor.City)
            .ThenByDescending(r => r.CreatedAt)
            .ToList();
    }

    private async Task<HashSet<int>> RespondedIdsAsync(int donorId)
    {
        var ids = await _db.RequestMatches
            .Where(m => m.DonorId == donorId)
            .Select(m => m.RequestId)
            .ToListAsync();
        return ids.ToHashSet();
    }

    // ==================== DASHBOARD ====================
    public async Task<IActionResult> Dashboard()
    {
        Chrome("dash", "لوحتي", "My dashboard");

        var donor = await CurrentDonorAsync();
        if (donor is null) return RedirectToAction("Index", "Home");

        var matching = await MatchingRequestsAsync(donor);

        var vm = new DonorDashboardViewModel
        {
            Donor = donor,
            TotalDonations = await _db.Donations.CountAsync(d => d.DonorId == donor.DonorId),
            MatchingOpenRequests = matching.Count,
            Preview = matching.Take(3).ToList(),
            RespondedIds = await RespondedIdsAsync(donor.DonorId),
        };

        return View(vm);
    }

    // ==================== MATCHED REQUESTS ====================
    public async Task<IActionResult> Requests()
    {
        Chrome("requests", "الطلبات المطابقة", "Matched requests");

        var donor = await CurrentDonorAsync();
        if (donor is null) return RedirectToAction("Index", "Home");

        ViewData["Donor"] = donor;
        ViewData["RespondedIds"] = await RespondedIdsAsync(donor.DonorId);

        var matching = await MatchingRequestsAsync(donor);
        return View(matching);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Respond(int requestId)
    {
        var donor = await CurrentDonorAsync();
        if (donor is null) return RedirectToAction("Index", "Home");

        var request = await _db.Requests.FindAsync(requestId);
        if (request is null || (request.Status != "Open" && request.Status != "Matched"))
        {
            Toast("This request is no longer accepting responses.", "error");
            return RedirectToAction(nameof(Requests));
        }

        // respond-once: ignore if already responded
        var already = await _db.RequestMatches.AnyAsync(m => m.RequestId == requestId && m.DonorId == donor.DonorId);
        if (!already)
        {
            _db.RequestMatches.Add(new RequestMatch
            {
                RequestId = requestId,
                DonorId = donor.DonorId,
                MatchState = "Responded",
                CreatedAt = DateTime.Now,
            });
            await _db.SaveChangesAsync();
        }

        Toast("Thanks! Your response was sent to the hospital. They'll expect you.");
        return RedirectToAction(nameof(Requests));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(int requestId)
    {
        var donor = await CurrentDonorAsync();
        if (donor is null) return RedirectToAction("Index", "Home");

        var match = await _db.RequestMatches
            .FirstOrDefaultAsync(m => m.RequestId == requestId && m.DonorId == donor.DonorId);

        if (match is not null)
        {
            _db.RequestMatches.Remove(match);
            await _db.SaveChangesAsync();
            Toast("Your response was withdrawn. The slot is open for another donor.", "info");
        }

        return RedirectToAction(nameof(Requests));
    }

    // ==================== DONATION HISTORY ====================
    public async Task<IActionResult> History()
    {
        Chrome("history", "سجل التبرعات", "Donation history");

        var donor = await CurrentDonorAsync();
        if (donor is null) return RedirectToAction("Index", "Home");

        ViewData["Donor"] = donor;

        var donations = await _db.Donations
            .Include(d => d.Hospital)
            .Where(d => d.DonorId == donor.DonorId)
            .OrderByDescending(d => d.DonationDate)
            .ToListAsync();

        return View(donations);
    }

    // ==================== PROFILE ====================
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        Chrome("profile", "ملفي الشخصي", "My profile");

        var donor = await CurrentDonorAsync();
        if (donor is null) return RedirectToAction("Index", "Home");

        var vm = new DonorProfileViewModel
        {
            DonorId = donor.DonorId,
            FullName = donor.FullName,
            Email = donor.Email,
            Phone = donor.Phone,
            City = donor.City,
            Area = donor.Area,
            IsAvailable = donor.IsAvailable,
            BloodType = donor.BloodType,
            IsVerified = donor.IsVerified,
        };

        ViewData["Donations"] = await _db.Donations.CountAsync(d => d.DonorId == donor.DonorId);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(DonorProfileViewModel vm)
    {
        Chrome("profile", "ملفي الشخصي", "My profile");

        var donor = await CurrentDonorAsync();
        if (donor is null) return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid)
        {
            vm.BloodType = donor.BloodType;
            vm.IsVerified = donor.IsVerified;
            ViewData["Donations"] = await _db.Donations.CountAsync(d => d.DonorId == donor.DonorId);
            Toast("Please correct the highlighted fields.", "error");
            return View(vm);
        }

        donor.FullName = vm.FullName;
        donor.Phone = vm.Phone;
        donor.City = vm.City;
        donor.Area = vm.Area;
        donor.IsAvailable = vm.IsAvailable;

        // Blood type can be changed by the donor ONLY while unverified (it's a
        // preliminary self-entered value until a hospital confirms it). Once
        // verified, only a hospital can change it — so we ignore the submitted
        // type here. Enforced server-side, not just in the UI.
        if (!donor.IsVerified)
        {
            var validTypes = new[] { "O+", "O-", "A+", "A-", "B+", "B-", "AB+", "AB-" };
            if (!string.IsNullOrEmpty(vm.BloodType) && validTypes.Contains(vm.BloodType))
                donor.BloodType = vm.BloodType;
        }

        await _db.SaveChangesAsync();

        Toast("Your profile has been updated.");
        return RedirectToAction(nameof(Profile));
    }
}
