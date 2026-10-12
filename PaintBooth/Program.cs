
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PaintBooth.Database;
using PaintBooth.Repositories;
using PaintBooth.Repositories.Interface;
using PaintBooth.Repositories.Repositories;
using PaintBooth.Service;


var builder = WebApplication.CreateBuilder(args);

// ============================================================
//  ALL builder.Services MUST come BEFORE builder.Build()
// ============================================================

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

// ?? 1. MVC ????????????????????????????????????????????????????
builder.Services.AddControllersWithViews();

// ?? 2. EF Core + MySQL ????????????????????????????????????????
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));

// ?? 3. Repositories ???????????????????????????????????????????
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IEnquiryRepository, EnquiryRepository>();
builder.Services.AddScoped<IPanelSettingRepository, PanelSettingRepository>();
builder.Services.AddScoped<IPaintBoothRepository, PaintBoothRepository>();
builder.Services.AddScoped<ICostingRepository, CostingRepository>();
builder.Services.AddScoped<IExceptionRepository, ExceptionRepository>();

// ?? 3. Services ???????????????????????????????????????????
builder.Services.AddHttpClient<FrappeOpportunityService>();

// ?? 4. Cookie Authentication ??????????????????????????????????
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// ?? 5. Session ????????????????????????????????????????????????
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ?? 6. HttpContextAccessor ????????????????????????????????????
builder.Services.AddHttpContextAccessor();

// ============================================================
//  builder.Build() — only called ONCE, after all services
// ============================================================
var app = builder.Build();

// ============================================================
//  Middleware pipeline — everything below uses app.Use...
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();           // ? must be before UseAuthentication
app.UseAuthentication();    // ? must be before UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"); // ? start at Login page

app.Run();
