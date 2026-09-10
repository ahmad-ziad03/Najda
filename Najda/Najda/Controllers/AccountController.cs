using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Najda.Data;
using Najda.Models;

namespace Najda.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _users;
    private readonly SignInManager<IdentityUser> _signIn;
    private readonly ApplicationDbContext _db;

    public AccountController(UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn, ApplicationDbContext db)
    {
        _users = users;
        _signIn = signIn;
        _db = db;
    }

    // Sends a signed-in user to the dashboard that matches their role.
    private async Task<IActionResult> RedirectToRoleHome(IdentityUser user, string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        if (await _users.IsInRoleAsync(user, "Admin"))
            return RedirectToAction("Dashboard", "Admin");

        if (await _users.IsInRoleAsync(user, "Hospital"))
        {
            // A hospital awaiting approval still gets in, but is told clearly.
            var hospital = _db.Hospitals.FirstOrDefault(h => h.UserId == user.Id);
            if (hospital is not null && hospital.Status == "Pending")
            {
                TempData["Toast"] = "Your hospital account is pending admin approval.";
                TempData["ToastType"] = "info";
            }
            else if (hospital is not null && hospital.Status == "Suspended")
            {
                TempData["Toast"] = "This hospital account is currently suspended.";
                TempData["ToastType"] = "error";
            }
            return RedirectToAction("Dashboard", "Hospital");
        }

        if (await _users.IsInRoleAsync(user, "Donor"))
            return RedirectToAction("Dashboard", "Donor");

        return RedirectToAction("Index", "Home");
    }

    // ---------------- Register ----------------
    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        // role-specific validation
        if (vm.Role == "Donor" && string.IsNullOrWhiteSpace(vm.BloodType))
            ModelState.AddModelError(nameof(vm.BloodType), "Please choose your blood type.");
        if (!vm.AgreeTerms)
            ModelState.AddModelError(nameof(vm.AgreeTerms), "You must accept the terms to continue.");

        if (!ModelState.IsValid)
            return View(vm);

        // an email can only be used once
        if (await _users.FindByEmailAsync(vm.Email) is not null)
        {
            ModelState.AddModelError(nameof(vm.Email), "This email is already registered.");
            return View(vm);
        }

        var user = new IdentityUser { UserName = vm.Email, Email = vm.Email, EmailConfirmed = true, PhoneNumber = vm.Phone };
        var created = await _users.CreateAsync(user, vm.Password);
        if (!created.Succeeded)
        {
            foreach (var e in created.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            return View(vm);
        }

        if (vm.Role == "Hospital")
        {
            await _users.AddToRoleAsync(user, "Hospital");
            _db.Hospitals.Add(new Hospital
            {
                Name = vm.FullName,
                Email = vm.Email,
                Phone = vm.Phone,
                LicenseNumber = vm.LicenseNumber,
                City = vm.City,
                Status = "Pending",          // hospitals await admin approval
                UserId = user.Id
            });
        }
        else
        {
            await _users.AddToRoleAsync(user, "Donor");
            _db.Donors.Add(new Donor
            {
                FullName = vm.FullName,
                Email = vm.Email,
                Phone = vm.Phone,
                BloodType = vm.BloodType!,   // required for donors, validated above
                City = vm.City,
                IsVerified = false,          // preliminary until a hospital confirms
                IsAvailable = true,
                UserId = user.Id
            });
        }

        await _db.SaveChangesAsync();
        await _signIn.SignInAsync(user, isPersistent: false);

        TempData["Toast"] = vm.Role == "Hospital"
            ? "Account created — your hospital is pending approval."
            : "Welcome to Najda! Your account is ready.";
        TempData["ToastType"] = "success";

        return await RedirectToRoleHome(user);
    }

    // ---------------- Login ----------------
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
            return View(vm);

        var result = await _signIn.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            var user = await _users.FindByEmailAsync(vm.Email);

            // A deactivated donor account cannot be used until an admin reactivates it.
            var donor = _db.Donors.FirstOrDefault(d => d.UserId == user!.Id);
            if (donor is not null && !donor.IsActive)
            {
                await _signIn.SignOutAsync();
                ModelState.AddModelError(string.Empty, "This account has been deactivated. Please contact the Najda administrators.");
                return View(vm);
            }

            TempData["Toast"] = "Welcome back!";
            TempData["ToastType"] = "success";
            return await RedirectToRoleHome(user!, returnUrl);
        }

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View(vm);
    }

    // ---------------- Logout ----------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        TempData["Toast"] = "You have been logged out.";
        TempData["ToastType"] = "info";
        return RedirectToAction("Index", "Home");
    }

    // ---------------- Access denied ----------------
    [HttpGet]
    public IActionResult AccessDenied(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // ---------------- Forgot password ----------------
    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(ForgotPasswordViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        // No email service is configured in this project, so we don't actually
        // send a link. We always show the same confirmation (which is also the
        // privacy-safe behavior — it never reveals whether an email exists).
        ViewData["Sent"] = true;
        return View(vm);
    }
}
