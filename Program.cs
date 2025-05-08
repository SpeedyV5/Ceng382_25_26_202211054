using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ClassManagement.Data;
using ClassManagement.Models;

var builder = WebApplication.CreateBuilder(args);

// ➡️ 1) Razor Pages + konvansiyonel koruma
builder.Services
  .AddRazorPages(options =>
  {
      // Index sayfasını ve tüm POST handler'larını koru:
      options.Conventions.AuthorizePage("/Index");
  });

// ➡️ 2) EF Core
builder.Services.AddDbContext<SchoolDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("SchoolDbConnection")));

// ➡️ 3) Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opts =>
{
    opts.SignIn.RequireConfirmedAccount = false;
    // ... diğer ayarlar
})
.AddEntityFrameworkStores<SchoolDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// ➡️ 4) Cookie ayarları: redirect için doğru yol
builder.Services.ConfigureApplicationCookie(opts =>
{
    opts.LoginPath = "/Identity/Account/Login";
    opts.LogoutPath="/Identity/Account/Logout";
    opts.AccessDeniedPath = "/Identity/Account/AccessDenied";
    opts.SlidingExpiration = true;
});

var app = builder.Build();

// pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
