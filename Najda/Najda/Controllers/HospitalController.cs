using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Najda.Data;
using Najda.Helpers;
using Najda.Models;
using Najda.Services;

namespace Najda.Controllers;

[Authorize(Roles = "Hospital")]
public class HospitalController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _users;
    private readonly IEmailSender _email;

    public HospitalController(ApplicationDbContext db, UserManager<IdentityUser> users, IEmailSender email)
    {
        _db = db;
        _users = users;
        _email = email;
    }

    private void Toast(string message, string type = "success")
    {
        TempData["Toast"] = message;
        TempData["ToastType"] = type;
    }

    // Note: an unused `titleEn` parameter is still accepted so every existing
    // call site (Chrome("key", "arabic", "english")) keeps working unchanged;
    // only the Arabic title is stored/displayed now.
    private void Chrome(string active, string titleAr, string titleEn = "")
    {
        ViewData["Area"] = "hospital";
        ViewData["Active"] = active;
        ViewData["TitleAr"] = titleAr;
    }

    // The Hospital record for the signed-in user.
    private async Task<Hospital?> CurrentHospitalAsync()
    {
        var userId = _users.GetUserId(User);
        return await _db.Hospitals.FirstOrDefaultAsync(h => h.UserId == userId);
    }

    // ==================== DASHBOARD ====================
    public async Task<IActionResult> Dashboard()
    {
        Chrome("dash", "لوحة المستشفى", "Hospital dashboard");

        var hospital = await CurrentHospitalAsync();
        if (hospital is null) return RedirectToAction("Index", "Home");

        var vm = new HospitalDashboardViewModel
        {
            Hospital = hospital,
            TotalRequests = await _db.Requests.CountAsync(r => r.HospitalId == hospital.HospitalId),
            OpenRequests = await _db.Requests.CountAsync(r => r.HospitalId == hospital.HospitalId && r.Status == "Open"),
            UrgentRequests = await _db.Requests.CountAsync(r => r.HospitalId == hospital.HospitalId && r.Priority == "Urgent" && r.Status == "Open"),
            MatchedRequests = await _db.Requests.CountAsync(r => r.HospitalId == hospital.HospitalId && r.Status == "Matched"),
            FulfilledRequests = await _db.Requests.CountAsync(r => r.HospitalId == hospital.HospitalId && r.Status == "Fulfilled"),
            RecentRequests = await _db.Requests
                .Where(r => r.HospitalId == hospital.HospitalId)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync(),
        };

        // available donors in the same city, grouped by blood type
        var supply = await _db.Donors
            .Where(d => d.IsActive && d.IsAvailable && d.City == hospital.City)
            .GroupBy(d => d.BloodType)
            .Select(g => new { Type = g.Key, N = g.Count() })
            .ToListAsync();
        vm.SupplyByType = supply.Select(x => (x.Type, x.N)).ToList();

        return View(vm);
    }

    // ==================== POST REQUEST ====================
    [HttpGet]
    public async Task<IActionResult> PostRequest()
    {
        Chrome("post", "طلب دم جديد", "New request");

        var hospital = await CurrentHospitalAsync();
        if (hospital is null) return RedirectToAction("Index", "Home");

        if (hospital.Status != "Approved")
        {
            Toast(hospital.Status == "Pending"
                ? "Your hospital is pending approval and cannot post requests yet."
                : "This hospital account is suspended and cannot post requests.",
                hospital.Status == "Pending" ? "info" : "error");
            return RedirectToAction(nameof(Dashboard));
        }

        ViewData["HospitalName"] = hospital.Name;
        return View(new RequestFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostRequest(RequestFormViewModel vm)
    {
        Chrome("post", "طلب دم جديد", "New request");

        var hospital = await CurrentHospitalAsync();
        if (hospital is null) return RedirectToAction("Index", "Home");

        if (hospital.Status != "Approved")
        {
            Toast("Only approved hospitals can post requests.", "error");
            return RedirectToAction(nameof(Dashboard));
        }

        if (!ModelState.IsValid)
        {
            ViewData["HospitalName"] = hospital.Name;
            Toast("Please correct the highlighted fields.", "error");
            return View(vm);
        }

        var request = new BloodRequest
        {
            HospitalId = hospital.HospitalId,
            BloodType = vm.BloodType,
            UnitsNeeded = vm.UnitsNeeded,
            UnitsConfirmed = 0,
            Priority = vm.Priority,
            Status = "Open",
            Department = vm.Department,
            NeededBy = vm.NeededBy,
            CreatedAt = DateTime.Now,
        };

        _db.Requests.Add(request);
        await _db.SaveChangesAsync();

        var notified = await NotifyCompatibleDonorsAsync(hospital, request);

        Toast(notified > 0
            ? $"Your request has been posted and {notified} compatible donor(s) were notified by email."
            : "Your request has been posted and is now visible to matching donors.");
        return RedirectToAction(nameof(Dashboard));
    }

    // Compatible + available + eligible (56-day rule) donors for a given needed blood type.
    // Shared by the notification email and the matched-donors screen so the
    // matching rule is defined in exactly one place.
    private async Task<List<Donor>> CompatibleAvailableEligibleDonorsAsync(string neededBloodType)
    {
        var compatibleTypes = BloodCompatibility.CompatibleDonorTypes(neededBloodType);
        var cutoff = DateTime.Today.AddDays(-56);

        return await _db.Donors
            .Where(d => d.IsActive && d.IsAvailable
                        && compatibleTypes.Contains(d.BloodType)
                        && (d.LastDonationDate == null || d.LastDonationDate <= cutoff))
            .ToListAsync();
    }

    // Email every compatible + available + eligible donor about a new request.
    private async Task<int> NotifyCompatibleDonorsAsync(Hospital hospital, BloodRequest request)
    {
        var donors = await CompatibleAvailableEligibleDonorsAsync(request.BloodType);

        if (donors.Count == 0) return 0;

        var loginUrl = $"{Request.Scheme}://{Request.Host}/Account/Login";
        var subject = "طلب دم يحتاج مساعدتك — نجدة";

        foreach (var d in donors)
        {
            var html = $@"
<div dir=""rtl"" style=""font-family:'Segoe UI',Tahoma,Arial,sans-serif;color:#16302C;max-width:560px;text-align:right"">
  <h2 style=""color:#0E4D45;margin-bottom:6px"">هناك حالة تحتاج نجدتك</h2>
  <p>نشر <strong>{hospital.Name}</strong> في {hospital.City} طلباً لفصيلة الدم
     <strong>{request.BloodType}</strong>، وفصيلتك متوافقة.</p>
  <p>إن كنت تستطيع المساعدة، سجّل الدخول واستجب.</p>
  <p><a href=""{loginUrl}"" style=""display:inline-block;background:#0E4D45;color:#ffffff;padding:11px 22px;border-radius:24px;text-decoration:none"">افتح نجدة</a></p>
  <hr style=""border:none;border-top:1px solid #E7E1D5;margin:22px 0"" />
  <p style=""color:#8A9794;font-size:12px;margin-top:18px"">نجدة — منصة التبرع بالدم</p>
</div>";

            await _email.SendAsync(d.Email, d.FullName, subject, html);
        }

        return donors.Count;
    }

    // ==================== MANAGE REQUESTS ====================
    public async Task<IActionResult> Requests(string? status)
    {
        Chrome("requests", "إدارة الطلبات", "Manage requests");

        var hospital = await CurrentHospitalAsync();
        if (hospital is null) return RedirectToAction("Index", "Home");

        var query = _db.Requests.Where(r => r.HospitalId == hospital.HospitalId);
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (status == "Urgent") query = query.Where(r => r.Priority == "Urgent" && r.Status == "Open");
            else query = query.Where(r => r.Status == status);
        }

        ViewData["status"] = status;
        ViewData["ApprovedHospital"] = hospital.Status == "Approved";
        ViewData["RefMap"] = await BuildRefMapAsync(hospital.HospitalId);

        // counts for the tabs
        ViewData["cAll"] = await _db.Requests.CountAsync(r => r.HospitalId == hospital.HospitalId);
        ViewData["cUrgent"] = await _db.Requests.CountAsync(r => r.HospitalId == hospital.HospitalId && r.Priority == "Urgent" && r.Status == "Open");
        ViewData["cOpen"] = await _db.Requests.CountAsync(r => r.HospitalId == hospital.HospitalId && r.Status == "Open");
        ViewData["cMatched"] = await _db.Requests.CountAsync(r => r.HospitalId == hospital.HospitalId && r.Status == "Matched");
        ViewData["cFulfilled"] = await _db.Requests.CountAsync(r => r.HospitalId == hospital.HospitalId && r.Status == "Fulfilled");

        var list = await query
            .Include(r => r.Matches)
            .OrderBy(r => r.Priority == "Urgent" && r.Status == "Open" ? 0 : 1)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(list);
    }

    // ---- edit request (GET) ----
    [HttpGet]
    public async Task<IActionResult> EditRequest(int id)
    {
        Chrome("requests", "تعديل الطلب", "Edit request");

        var request = await OwnedRequestAsync(id);
        if (request is null) { Toast("Request not found.", "error"); return RedirectToAction(nameof(Requests)); }

        if (request.Status == "Fulfilled" || request.Status == "Cancelled")
        {
            Toast("A fulfilled or cancelled request can't be edited.", "error");
            return RedirectToAction(nameof(Requests));
        }

        var vm = new RequestFormViewModel
        {
            RequestId = request.RequestId,
            BloodType = request.BloodType,
            UnitsNeeded = request.UnitsNeeded,
            Priority = request.Priority,
            Department = request.Department,
            NeededBy = request.NeededBy,
        };

        ViewData["HospitalName"] = (await CurrentHospitalAsync())?.Name;
        ViewData["Confirmed"] = request.UnitsConfirmed;
        ViewData["Ref"] = (await BuildRefMapAsync(request.HospitalId)).GetValueOrDefault(request.RequestId, $"REQ-{request.RequestId}");
        return View(vm);
    }

    // ---- edit request (POST) ----
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRequest(RequestFormViewModel vm)
    {
        Chrome("requests", "تعديل الطلب", "Edit request");

        var request = await OwnedRequestAsync(vm.RequestId);
        if (request is null) { Toast("Request not found.", "error"); return RedirectToAction(nameof(Requests)); }

        if (!ModelState.IsValid)
        {
            ViewData["HospitalName"] = (await CurrentHospitalAsync())?.Name;
            ViewData["Confirmed"] = request.UnitsConfirmed;
            Toast("Please correct the highlighted fields.", "error");
            return View(vm);
        }

        // can't set needed units below what's already confirmed
        if (vm.UnitsNeeded < request.UnitsConfirmed)
        {
            ModelState.AddModelError(nameof(vm.UnitsNeeded), $"Units needed can't be less than the {request.UnitsConfirmed} already confirmed.");
            ViewData["HospitalName"] = (await CurrentHospitalAsync())?.Name;
            ViewData["Confirmed"] = request.UnitsConfirmed;
            Toast("Please correct the highlighted fields.", "error");
            return View(vm);
        }

        request.BloodType = vm.BloodType;
        request.UnitsNeeded = vm.UnitsNeeded;
        request.Priority = vm.Priority;
        request.Department = vm.Department;
        request.NeededBy = vm.NeededBy;

        // keep status coherent with the new unit count
        if (request.Status != "Cancelled")
            request.Status = request.UnitsConfirmed >= request.UnitsNeeded ? "Fulfilled"
                           : request.UnitsConfirmed > 0 ? "Matched" : "Open";

        await _db.SaveChangesAsync();

        Toast("The request has been updated.");
        return RedirectToAction(nameof(Requests));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FulfillRequest(int id)
    {
        var request = await OwnedRequestAsync(id);
        if (request is null) { Toast("Request not found.", "error"); return RedirectToAction(nameof(Requests)); }

        request.Status = "Fulfilled";
        await _db.SaveChangesAsync();
        Toast("Request marked as fulfilled.");
        return RedirectToAction(nameof(Requests));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelRequest(int id)
    {
        var request = await OwnedRequestAsync(id);
        if (request is null) { Toast("Request not found.", "error"); return RedirectToAction(nameof(Requests)); }

        request.Status = "Cancelled";
        await _db.SaveChangesAsync();
        Toast("Request cancelled.", "info");
        return RedirectToAction(nameof(Requests));
    }

    // ==================== MATCHED DONORS + VERIFICATION ====================
    public async Task<IActionResult> Matched(int? id)
    {
        Chrome("matched", "المتبرعون المطابقون", "Matched donors");

        var hospital = await CurrentHospitalAsync();
        if (hospital is null) return RedirectToAction("Index", "Home");

        // requests this hospital can still act on
        var openRequests = await _db.Requests
            .Where(r => r.HospitalId == hospital.HospitalId && (r.Status == "Open" || r.Status == "Matched"))
            .OrderBy(r => r.Priority == "Urgent" ? 0 : 1)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync();

        ViewData["OpenRequests"] = openRequests;
        ViewData["HospitalCity"] = hospital.City;
        ViewData["RefMap"] = await BuildRefMapAsync(hospital.HospitalId);

        // pick the request to show donors for
        var request = id.HasValue
            ? openRequests.FirstOrDefault(r => r.RequestId == id.Value)
            : openRequests.FirstOrDefault();

        if (request is null)
        {
            ViewData["Request"] = null;
            ViewData["Matches"] = new List<Donor>();
            return View();
        }

        ViewData["Request"] = request;

        // ---- THE MATCHING QUERY (compatible + available + eligible, nearest first) ----
        var donors = await CompatibleAvailableEligibleDonorsAsync(request.BloodType);

        // donors who actively responded to THIS request
        var responderIds = (await _db.RequestMatches
            .Where(m => m.RequestId == request.RequestId)
            .Select(m => m.DonorId)
            .ToListAsync()).ToHashSet();
        ViewData["ResponderIds"] = responderIds;

        // responders first, then same-city, then verified, then longest-waiting
        var ranked = donors
            .OrderByDescending(d => responderIds.Contains(d.DonorId))
            .ThenByDescending(d => d.City == hospital.City)
            .ThenByDescending(d => d.IsVerified)
            .ThenBy(d => d.LastDonationDate ?? DateTime.MinValue)
            .ToList();

        ViewData["Matches"] = ranked;
        return View();
    }

    // ---- verify a donor after their FIRST donation (unverified donors) ----
    // Records the donation, verifies/corrects the type. Counts a unit only when
    // the confirmed type is compatible with the request.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyDonor(int requestId, int donorId, string confirmedType)
    {
        var hospital = await CurrentHospitalAsync();
        if (hospital is null) return RedirectToAction("Index", "Home");

        var request = await _db.Requests.FirstOrDefaultAsync(r => r.RequestId == requestId && r.HospitalId == hospital.HospitalId);
        var donor = await _db.Donors.FindAsync(donorId);

        if (request is null || donor is null)
        {
            Toast("Request or donor not found.", "error");
            return RedirectToAction(nameof(Matched), new { id = requestId });
        }

        // 56-day rule: never record a donation for a donor still in their window.
        if (!donor.IsEligibleNow)
        {
            Toast($"{donor.FullName} isn't eligible to donate yet (56-day rule). No donation was recorded.", "error");
            return RedirectToAction(nameof(Matched), new { id = requestId });
        }

        var validTypes = new[] { "O+", "O-", "A+", "A-", "B+", "B-", "AB+", "AB-" };
        if (!validTypes.Contains(confirmedType)) confirmedType = donor.BloodType;

        var today = DateTime.Today;

        _db.Donations.Add(new Donation
        {
            DonorId = donor.DonorId,
            HospitalId = hospital.HospitalId,
            RequestId = request.RequestId,
            BloodType = confirmedType,
            DonationDate = today,
        });

        var corrected = donor.BloodType != confirmedType;
        donor.BloodType = confirmedType;
        donor.IsVerified = true;
        donor.VerifiedByHospitalId = hospital.HospitalId;
        donor.VerifiedDate = today;
        donor.LastDonationDate = today;

        var compatible = BloodCompatibility.IsCompatible(request.BloodType, confirmedType);
        if (compatible)
        {
            request.UnitsConfirmed += 1;
            request.Status = request.UnitsConfirmed >= request.UnitsNeeded ? "Fulfilled" : "Matched";
        }

        await _db.SaveChangesAsync();

        if (compatible)
        {
            Toast(corrected
                ? $"{donor.FullName} verified as {confirmedType} (corrected) and one unit counted."
                : $"{donor.FullName} verified as {confirmedType}. One unit counted toward the request.");
        }
        else
        {
            Toast($"{donor.FullName} verified as {confirmedType} — not compatible with this {request.BloodType} request, so no unit was counted.", "error");
        }

        return RedirectToAction(nameof(Matched), new { id = requestId });
    }

    // ---- record a repeat donation (already-verified donors) ----
    // No re-verification: the type is already trusted and (being on this matched
    // list) compatible, so it always counts a unit. Still respects the 56-day rule.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordDonation(int requestId, int donorId)
    {
        var hospital = await CurrentHospitalAsync();
        if (hospital is null) return RedirectToAction("Index", "Home");

        var request = await _db.Requests.FirstOrDefaultAsync(r => r.RequestId == requestId && r.HospitalId == hospital.HospitalId);
        var donor = await _db.Donors.FindAsync(donorId);

        if (request is null || donor is null)
        {
            Toast("Request or donor not found.", "error");
            return RedirectToAction(nameof(Matched), new { id = requestId });
        }

        // guard: verified donors only take this path
        if (!donor.IsVerified)
        {
            Toast("This donor isn't verified yet — verify their type first.", "error");
            return RedirectToAction(nameof(Matched), new { id = requestId });
        }

        // 56-day rule (also enforced by the matched-list filter)
        if (!donor.IsEligibleNow)
        {
            Toast($"{donor.FullName} isn't eligible to donate yet (56-day rule). No donation was recorded.", "error");
            return RedirectToAction(nameof(Matched), new { id = requestId });
        }

        // safety: the donor's verified type must still be compatible with the request
        if (!BloodCompatibility.IsCompatible(request.BloodType, donor.BloodType))
        {
            Toast($"{donor.FullName}'s type ({donor.BloodType}) isn't compatible with this {request.BloodType} request.", "error");
            return RedirectToAction(nameof(Matched), new { id = requestId });
        }

        var today = DateTime.Today;

        _db.Donations.Add(new Donation
        {
            DonorId = donor.DonorId,
            HospitalId = hospital.HospitalId,
            RequestId = request.RequestId,
            BloodType = donor.BloodType,
            DonationDate = today,
        });

        donor.LastDonationDate = today;

        request.UnitsConfirmed += 1;
        request.Status = request.UnitsConfirmed >= request.UnitsNeeded ? "Fulfilled" : "Matched";

        await _db.SaveChangesAsync();

        Toast($"Donation recorded for {donor.FullName}. One unit counted toward the request.");
        return RedirectToAction(nameof(Matched), new { id = requestId });
    }

    // ---- correct an already-verified donor's type (lab found it wrong) ----
    // The correction ALWAYS happens. A donation is recorded and a unit counted
    // only if the corrected type is compatible AND the donor is eligible — so the
    // 56-day clock starts only when a real donation is recorded.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CorrectDonorType(int requestId, int donorId, string confirmedType)
    {
        var hospital = await CurrentHospitalAsync();
        if (hospital is null) return RedirectToAction("Index", "Home");

        var request = await _db.Requests.FirstOrDefaultAsync(r => r.RequestId == requestId && r.HospitalId == hospital.HospitalId);
        var donor = await _db.Donors.FindAsync(donorId);

        if (request is null || donor is null)
        {
            Toast("Request or donor not found.", "error");
            return RedirectToAction(nameof(Matched), new { id = requestId });
        }

        var validTypes = new[] { "O+", "O-", "A+", "A-", "B+", "B-", "AB+", "AB-" };
        if (!validTypes.Contains(confirmedType)) confirmedType = donor.BloodType;

        var today = DateTime.Today;
        var wasCorrected = donor.BloodType != confirmedType;

        // 1) always correct + re-verify the type
        donor.BloodType = confirmedType;
        donor.IsVerified = true;
        donor.VerifiedByHospitalId = hospital.HospitalId;
        donor.VerifiedDate = today;

        // 2) record a donation + count a unit only if compatible AND eligible
        var compatible = BloodCompatibility.IsCompatible(request.BloodType, confirmedType);
        var eligible = donor.IsEligibleNow;

        if (compatible && eligible)
        {
            _db.Donations.Add(new Donation
            {
                DonorId = donor.DonorId,
                HospitalId = hospital.HospitalId,
                RequestId = request.RequestId,
                BloodType = confirmedType,
                DonationDate = today,
            });
            donor.LastDonationDate = today;   // starts the 56-day clock
            request.UnitsConfirmed += 1;
            request.Status = request.UnitsConfirmed >= request.UnitsNeeded ? "Fulfilled" : "Matched";
        }

        await _db.SaveChangesAsync();

        if (compatible && eligible)
        {
            Toast($"{donor.FullName}'s type corrected to {confirmedType} and one unit counted.");
        }
        else if (!compatible)
        {
            Toast($"{donor.FullName}'s type corrected to {confirmedType} — not compatible with this {request.BloodType} request, so no unit was counted.", "error");
        }
        else // compatible but not eligible
        {
            Toast($"{donor.FullName}'s type corrected to {confirmedType}. They aren't eligible to donate yet (56-day rule), so no unit was counted.", "info");
        }

        return RedirectToAction(nameof(Matched), new { id = requestId });
    }

    // A request that belongs to the signed-in hospital (guards ownership).
    private async Task<BloodRequest?> OwnedRequestAsync(int id)
    {
        var hospital = await CurrentHospitalAsync();
        if (hospital is null) return null;
        return await _db.Requests.FirstOrDefaultAsync(r => r.RequestId == id && r.HospitalId == hospital.HospitalId);
    }

    // Friendly per-hospital reference numbers (REQ-01, REQ-02, ...) based on the
    // order the hospital posted its requests — clearer than the raw database id.
    private async Task<Dictionary<int, string>> BuildRefMapAsync(int hospitalId)
    {
        var order = await _db.Requests
            .Where(r => r.HospitalId == hospitalId)
            .OrderBy(r => r.CreatedAt)
            .Select(r => r.RequestId)
            .ToListAsync();

        return order
            .Select((rid, i) => new { rid, label = $"REQ-{(i + 1):00}" })
            .ToDictionary(x => x.rid, x => x.label);
    }
}
