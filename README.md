# نجدة · Najda — Blood Donor Matching Platform

Connecting hospitals and blood banks with the nearest **compatible, available** blood donors — in minutes, not hours.

Najda (Arabic for _“rescue / aid”_) turns the chaotic, manual search for a blood donor into an organized, fast system. A hospital posts an urgent request, and the platform instantly surfaces the donors whose blood type is **compatible** and who are **closest** to the hospital. It is **free for both donors and hospitals** — and adds a trust layer: a donor's blood type is treated as _preliminary_ until a hospital confirms it in the lab.

Graduation project · **Orange Coding Academy**

---

## Key Features

- **Three roles** — Donor, Hospital, and Administrator, each with its own dashboard.
- **Smart matching** — by blood-type compatibility, geographic proximity, availability, and 56-day eligibility.
- **Blood-type verification** — a donor's type is _unverified_ until a hospital confirms (or corrects) it after a real donation.
- **Hospital workflow** — post requests, track status (Open / Matched / Fulfilled), view matched donors, and verify types.
- **Admin controls** — approve/suspend hospitals, edit hospital & contact-person data, manage donors, platform overview.
- **Donor experience** — matched requests, donation history, availability toggle, and next-eligible-date reminders.
- **Bilingual UI** — Arabic (default, RTL) and English, switchable at runtime and remembered.
- **Ethical revenue model** — community-partner rewards and responsible sponsors; never from the blood itself.

---

## Tech Stack

| Layer        | Technology                                                       |
| ------------ | ---------------------------------------------------------------- |
| Framework    | ASP.NET Core **MVC** (.NET 10)                                   |
| Language     | C#                                                               |
| Data access  | **Entity Framework Core** — Code First + Migrations              |
| Database     | **Microsoft SQL Server** (LocalDB / Express)                     |
| Auth         | ASP.NET Core **Identity** (role-based: Donor / Hospital / Admin) |
| Front-end    | HTML, CSS, JavaScript, **Bootstrap 5**, custom design system     |
| Localization | Bilingual AR/EN via dual-node markup (no reload)                 |

---

## Database

Built **Code First** — the C# model classes define the schema, and EF Core migrations create it in SQL Server.

Core tables: `Hospitals`, `Donors`, `Requests`, `RequestMatches`, `Donations`.
Revenue tables: `Partners`, `Coupons`, `CouponRedemptions`, `Sponsors`.
Identity: `AspNetUsers`, `AspNetRoles`, … (linked to `Donors`/`Hospitals` via `UserId`).

The full ERD and schema are in the project documentation (`Najda-Documentation-Phase1.docx`).

---

## Getting Started

### Prerequisites

- .NET 10 SDK
- **SQL Server** — LocalDB (ships with Visual Studio) or SQL Server Express
- Visual Studio 2026

### 1. Clone

```bash
git clone https://github.com/<ahmad-ziad03>/najda.git
cd najda
```

### 2. Set the connection string

In `appsettings.json`, point `DefaultConnection` at your SQL Server:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=NajdaDB;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

### 3. Create the database

Run the EF Core migrations (Package Manager Console):

```powershell
Update-Database
```

or with the .NET CLI:

```bash
dotnet ef database update
```

### 4. Run

Press **F5** in Visual Studio (or `dotnet run`). On first launch the app **seeds** the three roles, a default admin, and demo data (hospitals, donors, requests).

### 5. Log in

| Role  | Email            | Password    |
| ----- | ---------------- | ----------- |
| Admin | `admin@najda.jo` | `Admin#123` |

Donors and hospitals can be created from the **Register** page.

---

## Project Structure

```
Najda/
├── Controllers/        # Home, Account (+ role controllers)
├── Models/             # EF Core entities + view models
├── Views/
│   ├── Home/           # Landing, About
│   ├── Account/        # Register, Login, Forgot password
│   └── Shared/         # Layouts + partials (nav, footer, sidebar, topbar)
├── Data/               # ApplicationDbContext, IdentitySeeder, DataSeeder
├── Helpers/            # ViewHelpers (icons, blood-type token, badges)
├── Migrations/         # EF Core migrations
├── wwwroot/            # css/styles.css, js/site.js, static assets
└── appsettings.json    # connection string
```

---

## Localization

The interface defaults to **Arabic (RTL)** and switches to **English (LTR)** at the click of a toggle — the choice is saved in the browser. Text is authored as dual nodes (`data-lang-ar` / `data-lang-en`); CSS shows only the active language, and the layout mirrors automatically using CSS logical properties.

---

## How Matching Works

1. A hospital posts a request for a blood type (what the **patient** needs).
2. The system finds donors whose type is **compatible** (standard red-blood-cell rules), who are **available**, and who are **eligible** (≥ 56 days since their last donation).
3. Results are ranked **nearest first** (same city), then verified donors first.
4. After donation, the hospital **verifies** the donor's type — locking it in for future matches.

---

## Project Status

- Database (Code First + EF Core + Migrations) — done
- Authentication & roles (Donor / Hospital / Admin) — done
- Public pages (Landing, About) + Account (Register / Login / Forgot password) — done
- Role dashboards (Donor → Hospital → Admin) — in progress
- Notifications & rewards program — planned

---

## Author

**\<Ahmad Ziad\>** — Graduation Project, Orange Coding Academy.

---

_Blood is never sold. Najda stays free for the people who save lives._
