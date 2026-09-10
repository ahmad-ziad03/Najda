using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Najda.Data;
using Najda.Models;

namespace Najda.Controllers;

// Every action here requires the Admin role.
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _users;

    public AdminController(ApplicationDbContext db, UserManager<IdentityUser> users)
    {
        _db = db;
        _users = users;
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
        ViewData["Area"] = "admin";
        ViewData["Active"] = active;
        ViewData["TitleAr"] = titleAr;
    }

    // ==================== OVERVIEW ====================
    public async Task<IActionResult> Dashboard()
    {
        Chrome("dash", "نظرة عامة", "Overview");

        var vm = new AdminDashboardViewModel
        {
            TotalDonors = await _db.Donors.CountAsync(),
            ActiveDonors = await _db.Donors.CountAsync(d => d.IsActive),
            VerifiedDonors = await _db.Donors.CountAsync(d => d.IsVerified),
            AvailableDonors = await _db.Donors.CountAsync(d => d.IsAvailable && d.IsActive),

            ApprovedHospitals = await _db.Hospitals.CountAsync(h => h.Status == "Approved"),
            PendingHospitals = await _db.Hospitals.CountAsync(h => h.Status == "Pending"),
            SuspendedHospitals = await _db.Hospitals.CountAsync(h => h.Status == "Suspended"),

            TotalRequests = await _db.Requests.CountAsync(),
            OpenRequests = await _db.Requests.CountAsync(r => r.Status == "Open"),
            UrgentRequests = await _db.Requests.CountAsync(r => r.Priority == "Urgent" && r.Status == "Open"),
            FulfilledRequests = await _db.Requests.CountAsync(r => r.Status == "Fulfilled"),

            TotalDonations = await _db.Donations.CountAsync(),
            UnitsProvided = await _db.Requests.SumAsync(r => (int?)r.UnitsConfirmed) ?? 0,
        };

        var demand = await _db.Requests
            .GroupBy(r => r.BloodType)
            .Select(g => new { Type = g.Key, N = g.Count() })
            .ToListAsync();
        vm.DemandByType = demand.Select(x => (x.Type, x.N)).ToList();

        var supply = await _db.Donors
            .Where(d => d.IsActive && d.IsAvailable)
            .GroupBy(d => d.BloodType)
            .Select(g => new { Type = g.Key, N = g.Count() })
            .ToListAsync();
        vm.SupplyByType = supply.Select(x => (x.Type, x.N)).ToList();

        vm.PendingList = await _db.Hospitals
            .Where(h => h.Status == "Pending")
            .OrderBy(h => h.CreatedAt)
            .ToListAsync();

        vm.RecentRequests = await _db.Requests
            .Include(r => r.Hospital)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .ToListAsync();

        return View(vm);
    }

    // ==================== DONORS ====================
    public async Task<IActionResult> Donors(string? q, string? bt, string? verified, string? status)
    {
        Chrome("users", "المتبرعون", "Donors");

        var query = _db.Donors.Include(d => d.VerifiedByHospital).AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(d => d.FullName.Contains(q) || d.Email.Contains(q) || d.City.Contains(q));

        if (!string.IsNullOrWhiteSpace(bt))
            query = query.Where(d => d.BloodType == bt);

        if (verified == "1") query = query.Where(d => d.IsVerified);
        else if (verified == "0") query = query.Where(d => !d.IsVerified);

        if (status == "active") query = query.Where(d => d.IsActive);
        else if (status == "inactive") query = query.Where(d => !d.IsActive);

        ViewData["q"] = q;
        ViewData["bt"] = bt;
        ViewData["verified"] = verified;
        ViewData["status"] = status;

        var list = await query.OrderByDescending(d => d.CreatedAt).ToListAsync();
        return View(list);
    }

    // ==================== REQUESTS (global monitor) ====================
    public async Task<IActionResult> Requests(string? q, string? bt, string? status, string? priority)
    {
        Chrome("requests", "الطلبات", "Requests");

        var query = _db.Requests.Include(r => r.Hospital).AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(r =>
                (r.Hospital != null && (r.Hospital.Name.Contains(q) || r.Hospital.City.Contains(q)))
                || (r.Department != null && r.Department.Contains(q)));

        if (!string.IsNullOrWhiteSpace(bt))
            query = query.Where(r => r.BloodType == bt);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.Status == status);

        if (priority == "Urgent")
            query = query.Where(r => r.Priority == "Urgent");

        ViewData["q"] = q;
        ViewData["bt"] = bt;
        ViewData["status"] = status;
        ViewData["priority"] = priority;

        // small headline counts (respecting nothing but the whole system)
        ViewData["cTotal"] = await _db.Requests.CountAsync();
        ViewData["cOpen"] = await _db.Requests.CountAsync(r => r.Status == "Open");
        ViewData["cUrgent"] = await _db.Requests.CountAsync(r => r.Priority == "Urgent" && r.Status == "Open");
        ViewData["cFulfilled"] = await _db.Requests.CountAsync(r => r.Status == "Fulfilled");

        var list = await query
            .OrderBy(r => r.Priority == "Urgent" && r.Status == "Open" ? 0 : 1)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(list);
    }

    // ---- single donor detail page with donation history ----
    public async Task<IActionResult> DonorDetails(int id)
    {
        Chrome("users", "تفاصيل المتبرع", "Donor details");

        var donor = await _db.Donors
            .Include(d => d.VerifiedByHospital)
            .FirstOrDefaultAsync(d => d.DonorId == id);

        if (donor is null) { Toast("Donor not found.", "error"); return RedirectToAction(nameof(Donors)); }

        ViewData["Donations"] = await _db.Donations
            .Include(dn => dn.Hospital)
            .Where(dn => dn.DonorId == id)
            .OrderByDescending(dn => dn.DonationDate)
            .ToListAsync();

        return View(donor);
    }

    // Deactivate <-> reactivate (reversible; history preserved)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleDonorActive(int id)
    {
        var donor = await _db.Donors.FindAsync(id);
        if (donor is null)
        {
            Toast("Donor not found.", "error");
            return RedirectToAction(nameof(Donors));
        }

        donor.IsActive = !donor.IsActive;

        // A deactivated donor is also removed from matching.
        if (!donor.IsActive) donor.IsAvailable = false;

        await _db.SaveChangesAsync();

        Toast(donor.IsActive
                ? $"{donor.FullName}'s account has been reactivated."
                : $"{donor.FullName}'s account has been deactivated.",
              donor.IsActive ? "success" : "info");

        return RedirectToAction(nameof(Donors));
    }

    // Permanent delete — blocked when donation history exists.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDonor(int id)
    {
        var donor = await _db.Donors.FindAsync(id);
        if (donor is null)
        {
            Toast("Donor not found.", "error");
            return RedirectToAction(nameof(Donors));
        }

        var hasDonations = await _db.Donations.AnyAsync(d => d.DonorId == id);
        if (hasDonations)
        {
            Toast("This donor has donation history and cannot be deleted. Deactivate the account instead.", "error");
            return RedirectToAction(nameof(Donors));
        }

        // Remove dependent match rows first.
        var matches = _db.RequestMatches.Where(m => m.DonorId == id);
        _db.RequestMatches.RemoveRange(matches);

        // Remove the linked login account, if any.
        if (!string.IsNullOrEmpty(donor.UserId))
        {
            var user = await _users.FindByIdAsync(donor.UserId);
            if (user is not null) await _users.DeleteAsync(user);
        }

        var name = donor.FullName;
        _db.Donors.Remove(donor);
        await _db.SaveChangesAsync();

        Toast($"{name} has been permanently deleted.");
        return RedirectToAction(nameof(Donors));
    }

    // ==================== HOSPITALS ====================
    public async Task<IActionResult> Hospitals(string? q, string? status)
    {
        Chrome("hospitals", "المستشفيات", "Hospitals");

        var query = _db.Hospitals.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(h => h.Name.Contains(q) || h.City.Contains(q));

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(h => h.Status == status);

        ViewData["q"] = q;
        ViewData["status"] = status;

        var list = await query
            .OrderBy(h => h.Status == "Pending" ? 0 : 1)
            .ThenByDescending(h => h.CreatedAt)
            .ToListAsync();

        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveHospital(int id)
    {
        var h = await _db.Hospitals.FindAsync(id);
        if (h is null) { Toast("Hospital not found.", "error"); return RedirectToAction(nameof(Hospitals)); }

        h.Status = "Approved";
        h.ApprovedDate = DateTime.Today;
        await _db.SaveChangesAsync();

        Toast($"{h.Name} has been approved.");
        return RedirectToAction(nameof(Hospitals));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectHospital(int id)
    {
        var h = await _db.Hospitals.FindAsync(id);
        if (h is null) { Toast("Hospital not found.", "error"); return RedirectToAction(nameof(Hospitals)); }

        var hasRequests = await _db.Requests.AnyAsync(r => r.HospitalId == id);
        if (hasRequests)
        {
            Toast("This hospital already has requests and cannot be rejected. Suspend it instead.", "error");
            return RedirectToAction(nameof(Hospitals));
        }

        if (!string.IsNullOrEmpty(h.UserId))
        {
            var user = await _users.FindByIdAsync(h.UserId);
            if (user is not null) await _users.DeleteAsync(user);
        }

        var name = h.Name;
        _db.Hospitals.Remove(h);
        await _db.SaveChangesAsync();

        Toast($"{name}'s application has been rejected.", "info");
        return RedirectToAction(nameof(Hospitals));
    }

    // Suspend <-> reactivate
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleHospitalSuspend(int id)
    {
        var h = await _db.Hospitals.FindAsync(id);
        if (h is null) { Toast("Hospital not found.", "error"); return RedirectToAction(nameof(Hospitals)); }

        if (h.Status == "Suspended")
        {
            h.Status = "Approved";
            await _db.SaveChangesAsync();
            Toast($"{h.Name} has been reactivated.");
        }
        else
        {
            h.Status = "Suspended";
            await _db.SaveChangesAsync();
            Toast($"{h.Name} has been suspended.", "error");
        }

        return RedirectToAction(nameof(ManageHospital), new { id });
    }

    // ---- single hospital management page ----
    public async Task<IActionResult> ManageHospital(int id)
    {
        Chrome("hospitals", "إدارة مستشفى", "Manage hospital");

        var hospital = await _db.Hospitals.FirstOrDefaultAsync(h => h.HospitalId == id);
        if (hospital is null) { Toast("Hospital not found.", "error"); return RedirectToAction(nameof(Hospitals)); }

        ViewData["Requests"] = await _db.Requests
            .Where(r => r.HospitalId == id)
            .OrderByDescending(r => r.CreatedAt)
            .Take(6)
            .ToListAsync();

        ViewData["TotalRequests"] = await _db.Requests.CountAsync(r => r.HospitalId == id);
        ViewData["ActiveRequests"] = await _db.Requests.CountAsync(r => r.HospitalId == id && r.Status == "Open");
        ViewData["FulfilledRequests"] = await _db.Requests.CountAsync(r => r.HospitalId == id && r.Status == "Fulfilled");

        return View(hospital);
    }

    // ---- edit hospital (GET) ----
    public async Task<IActionResult> EditHospital(int id)
    {
        Chrome("hospitals", "تعديل بيانات المستشفى", "Edit hospital");

        var h = await _db.Hospitals.FindAsync(id);
        if (h is null) { Toast("Hospital not found.", "error"); return RedirectToAction(nameof(Hospitals)); }

        var vm = new HospitalEditViewModel
        {
            HospitalId = h.HospitalId,
            Name = h.Name,
            Email = h.Email,
            Phone = h.Phone,
            LicenseNumber = h.LicenseNumber,
            City = h.City,
            Area = h.Area,
            Status = h.Status,
            ContactName = h.ContactName,
            ContactEmail = h.ContactEmail,
            ContactPhone = h.ContactPhone,
            ContactAppointedDate = h.ContactAppointedDate,
        };

        return View(vm);
    }

    // ---- edit hospital (POST) : receive -> validate -> store -> display back ----
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditHospital(HospitalEditViewModel vm)
    {
        Chrome("hospitals", "تعديل بيانات المستشفى", "Edit hospital");

        if (!ModelState.IsValid)
        {
            Toast("Please correct the highlighted fields.", "error");
            return View(vm);
        }

        var h = await _db.Hospitals.FindAsync(vm.HospitalId);
        if (h is null) { Toast("Hospital not found.", "error"); return RedirectToAction(nameof(Hospitals)); }

        // email must stay unique across hospitals
        var emailTaken = await _db.Hospitals.AnyAsync(x => x.Email == vm.Email && x.HospitalId != vm.HospitalId);
        if (emailTaken)
        {
            ModelState.AddModelError(nameof(vm.Email), "Another hospital already uses this email.");
            Toast("Please correct the highlighted fields.", "error");
            return View(vm);
        }

        h.Name = vm.Name;
        h.Email = vm.Email;
        h.Phone = vm.Phone;
        h.LicenseNumber = vm.LicenseNumber;
        h.City = vm.City;
        h.Area = vm.Area;
        h.ContactName = vm.ContactName;
        h.ContactEmail = vm.ContactEmail;
        h.ContactPhone = vm.ContactPhone;
        h.ContactAppointedDate = vm.ContactAppointedDate;

        // approving through the edit form stamps the approval date
        if (h.Status != "Approved" && vm.Status == "Approved") h.ApprovedDate = DateTime.Today;
        h.Status = vm.Status;

        await _db.SaveChangesAsync();

        Toast($"{h.Name}'s details were saved successfully.");
        return RedirectToAction(nameof(ManageHospital), new { id = h.HospitalId });
    }
}
