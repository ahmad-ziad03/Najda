using Microsoft.EntityFrameworkCore;
using Najda.Models;

namespace Najda.Data;

// Fills the database with a little demo data the first time the app runs,
// so the home page and dashboards have something to show. Runs only when
// the tables are empty, so it's safe on every startup.
public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (await db.Hospitals.AnyAsync())
            return; // already seeded

        // --- Hospitals ---
        var bashir = new Hospital
        {
            Name = "مستشفى البشير", Email = "info@bashir.jo", City = "عمان",
            Status = "Approved", ApprovedDate = new DateTime(2025, 1, 19),
            ContactName = "د. أحمد سرحان", ContactEmail = "ahmad.sarhan@bashir.jo",
            ContactPhone = "+962 79 555 0132", ContactAppointedDate = new DateTime(2025, 1, 19)
        };
        var kah = new Hospital
        {
            Name = "مستشفى الملك المؤسس", Email = "info@kah.jo", City = "اربد",
            Status = "Approved", ApprovedDate = new DateTime(2025, 2, 2),
            ContactName = "أ. سميرة خالد", ContactEmail = "samira.khaled@kah.jo",
            ContactPhone = "+962 79 555 0210", ContactAppointedDate = new DateTime(2025, 2, 2)
        };
        var hamzah = new Hospital
        {
            Name = "مستشفى الأمير حمزة", Email = "info@hamzah.jo", City = "عمان",
            Status = "Approved", ApprovedDate = new DateTime(2025, 2, 10),
            ContactName = "د. ليث عوّاد", ContactEmail = "laith.awad@hamzah.jo",
            ContactPhone = "+962 79 555 0388", ContactAppointedDate = new DateTime(2025, 2, 10)
        };
        var nbb = new Hospital
        {
            Name = "بنك الدم الوطني", Email = "contact@nbb.jo", City = "عمان",
            Status = "Pending",
            ContactName = "أ. رزان يوسف", ContactEmail = "razan.yousef@nbb.jo",
            ContactPhone = "+962 79 555 0455"
        };
        db.Hospitals.AddRange(bashir, kah, hamzah, nbb);
        await db.SaveChangesAsync();

        // --- Donors (small, logical demo set) ---
        db.Donors.AddRange(
            new Donor { FullName = "سامر الخطيب", Email = "samer@example.com", BloodType = "O+", IsVerified = true, VerifiedByHospitalId = bashir.HospitalId, VerifiedDate = new DateTime(2025, 1, 19), City = "عمان", Area = "الجبيهة", IsAvailable = true, LastDonationDate = new DateTime(2025, 7, 2) },
            new Donor { FullName = "ليلى منصور", Email = "layla@example.com", BloodType = "A-", IsVerified = true, VerifiedByHospitalId = kah.HospitalId, VerifiedDate = new DateTime(2025, 3, 5), City = "اربد", IsAvailable = true, LastDonationDate = new DateTime(2025, 5, 20) },
            new Donor { FullName = "خالد الرشيد", Email = "khaled@example.com", BloodType = "B+", IsVerified = false, City = "الزرقاء", IsAvailable = true },
            new Donor { FullName = "رنا عبدالله", Email = "rana@example.com", BloodType = "O-", IsVerified = true, VerifiedByHospitalId = bashir.HospitalId, VerifiedDate = new DateTime(2024, 11, 11), City = "عمان", IsAvailable = true, LastDonationDate = new DateTime(2025, 4, 10) },
            new Donor { FullName = "عمر السالم", Email = "omar@example.com", BloodType = "O+", IsVerified = true, VerifiedByHospitalId = kah.HospitalId, VerifiedDate = new DateTime(2025, 2, 14), City = "المفرق", IsAvailable = true, LastDonationDate = new DateTime(2025, 6, 15) },
            new Donor { FullName = "يوسف حدّاد", Email = "yousef@example.com", BloodType = "AB+", IsVerified = false, City = "العقبة", IsAvailable = false }
        );

        // --- Requests (varied unit counts; confirmed <= needed; bilingual reason) ---
        db.Requests.AddRange(
            new BloodRequest { HospitalId = bashir.HospitalId, BloodType = "O-", UnitsNeeded = 3, UnitsConfirmed = 1, Priority = "Urgent", Status = "Open", Department = "حالة طوارئ — قسم العناية", CreatedAt = new DateTime(2025, 8, 27, 8, 20, 0) },
            new BloodRequest { HospitalId = hamzah.HospitalId, BloodType = "O+", UnitsNeeded = 2, UnitsConfirmed = 0, Priority = "Urgent", Status = "Open", Department = "عملية جراحية عاجلة", CreatedAt = new DateTime(2025, 8, 27, 7, 5, 0) },
            new BloodRequest { HospitalId = kah.HospitalId, BloodType = "A+", UnitsNeeded = 4, UnitsConfirmed = 4, Priority = "Normal", Status = "Matched", Department = "مريض تلاسيميا", CreatedAt = new DateTime(2025, 8, 26, 16, 40, 0) },
            new BloodRequest { HospitalId = bashir.HospitalId, BloodType = "B+", UnitsNeeded = 1, UnitsConfirmed = 0, Priority = "Normal", Status = "Open", Department = "احتياط بنك الدم", CreatedAt = new DateTime(2025, 8, 26, 11, 15, 0) },
            new BloodRequest { HospitalId = hamzah.HospitalId, BloodType = "O+", UnitsNeeded = 2, UnitsConfirmed = 2, Priority = "Normal", Status = "Fulfilled", Department = "اكتملت بنجاح", CreatedAt = new DateTime(2025, 8, 23, 14, 0, 0) }
        );

        await db.SaveChangesAsync();

        // --- Donations (history for a few donors) ---
        var samer = await db.Donors.FirstAsync(d => d.Email == "samer@example.com");
        var layla = await db.Donors.FirstAsync(d => d.Email == "layla@example.com");
        var rana = await db.Donors.FirstAsync(d => d.Email == "rana@example.com");
        var omar = await db.Donors.FirstAsync(d => d.Email == "omar@example.com");

        db.Donations.AddRange(
            new Donation { DonorId = samer.DonorId, HospitalId = bashir.HospitalId, BloodType = "O+", DonationDate = new DateTime(2025, 7, 2) },
            new Donation { DonorId = samer.DonorId, HospitalId = hamzah.HospitalId, BloodType = "O+", DonationDate = new DateTime(2025, 4, 28) },
            new Donation { DonorId = samer.DonorId, HospitalId = bashir.HospitalId, BloodType = "O+", DonationDate = new DateTime(2025, 1, 19) },
            new Donation { DonorId = layla.DonorId, HospitalId = kah.HospitalId, BloodType = "A-", DonationDate = new DateTime(2025, 5, 20) },
            new Donation { DonorId = rana.DonorId, HospitalId = bashir.HospitalId, BloodType = "O-", DonationDate = new DateTime(2025, 4, 10) },
            new Donation { DonorId = rana.DonorId, HospitalId = bashir.HospitalId, BloodType = "O-", DonationDate = new DateTime(2024, 11, 11) },
            new Donation { DonorId = omar.DonorId, HospitalId = kah.HospitalId, BloodType = "O+", DonationDate = new DateTime(2025, 6, 15) }
        );

        await db.SaveChangesAsync();
    }
}
