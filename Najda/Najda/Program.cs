using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Najda.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Send unauthenticated / unauthorized users to our own pages instead of the
// default Identity ones, so the Najda design and role messaging are used.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();

// --- Email (Brevo) ---
// The API key is read from configuration; keep it in user secrets, NOT in the repo.
var brevoOptions = builder.Configuration.GetSection("Brevo").Get<Najda.Services.BrevoOptions>()
                   ?? new Najda.Services.BrevoOptions();
builder.Services.AddSingleton(brevoOptions);
builder.Services.AddHttpClient();
builder.Services.AddScoped<Najda.Services.IEmailSender, Najda.Services.BrevoEmailSender>();

var app = builder.Build();

// Seed roles + admin, then demo data (both are safe to run every startup).
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await IdentitySeeder.SeedAsync(services);
    await DataSeeder.SeedAsync(services.GetRequiredService<ApplicationDbContext>());
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Show our own 404 / status pages
app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();   // must come before UseAuthorization
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
