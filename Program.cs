using Microsoft.EntityFrameworkCore;
using ClassManagement.Data; // DbContext'in olduğu namespace

var builder = WebApplication.CreateBuilder(args);

// ➡️ Add services to the container:
builder.Services.AddRazorPages();

// ➡️ Add and configure the database context
builder.Services.AddDbContext<SchoolDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SchoolDbConnection")));

// ➡️ Add Session middleware
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ➡️ Configure the HTTP request pipeline:
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession(); // Session middleware aktif
app.UseAuthorization();

app.MapRazorPages();

app.Run();
