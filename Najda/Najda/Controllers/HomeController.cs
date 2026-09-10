using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Najda.Data;
using Najda.Models;

namespace Najda.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveNav"] = "home";

        var vm = new HomeViewModel
        {
            TotalDonors = await _db.Donors.CountAsync(),
            AvailableDonors = await _db.Donors.CountAsync(d => d.IsAvailable),
            ApprovedHospitals = await _db.Hospitals.CountAsync(h => h.Status == "Approved"),
            UnitsProvided = await _db.Requests.SumAsync(r => (int?)r.UnitsConfirmed) ?? 0,

            // Live section: up to 3 still-open requests, urgent first, newest first.
            LiveRequests = await _db.Requests
                .Include(r => r.Hospital)
                .Include(r => r.Matches)
                .Where(r => r.Status != "Fulfilled" && r.Status != "Cancelled")
                .OrderBy(r => r.Priority == "Urgent" ? 0 : 1)
                .ThenByDescending(r => r.CreatedAt)
                .Take(3)
                .ToListAsync()
        };

        return View(vm);
    }

    public IActionResult About()
    {
        ViewData["ActiveNav"] = "about";
        return View();
    }

    // Public, read-only list of open requests across all hospitals (urgent first).
    public async Task<IActionResult> UrgentRequests(string? bt, string? city)
    {
        ViewData["ActiveNav"] = "requests";

        var query = _db.Requests
            .Include(r => r.Hospital)
            .Where(r => r.Status == "Open" || r.Status == "Matched");

        if (!string.IsNullOrWhiteSpace(bt))
            query = query.Where(r => r.BloodType == bt);

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(r => r.Hospital != null && r.Hospital.City == city);

        ViewData["bt"] = bt;
        ViewData["city"] = city;
        ViewData["cUrgent"] = await _db.Requests.CountAsync(r => r.Priority == "Urgent" && r.Status == "Open");

        var list = await query
            .OrderBy(r => r.Priority == "Urgent" && r.Status == "Open" ? 0 : 1)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(list);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Custom status pages (404 etc.)
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult StatusCode(int? code = null)
    {
        if (code == 404) return View("NotFound");
        ViewData["Code"] = code;
        return View("NotFound");
    }
}
