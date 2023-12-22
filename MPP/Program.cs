using DAL.Common;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using Model.Models;
using MPP;
using MPP.ViewComponents;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddSessionStateTempDataProvider();
builder.Services.AddSession();
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});
builder.Services.AddRazorPages().AddSessionStateTempDataProvider();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddTransient<MenuViewComponent>();
builder.Services.AddTransient<SubMenuViewComponent>();
builder.Services.AddTransient<ShowAttributeViewComponent>();
builder.Services.AddTransient<GetSearchDataViewComponent>();
builder.Services.AddSingleton<LogError>();
// Add Session Services

builder.Services.AddMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".Mpp.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(50); // Customize the session timeout
});

//Add the IWebHostEnvironment service
builder.Services.AddSingleton(builder.Environment);

builder.Services.AddDbContext<MPP_Context>(
      //  options => options.UseSqlServer(builder.Configuration.GetConnectionString("Data Source=zwqmyad0001;Initial Catalog=MPP_QA;Persist Security Info=True;Connection Timeout=180;User ID=MPP_DEV_APP;Password=LZ/&&S]Q9rnin8)5;TrustServerCertificate=True")));
         options => options.UseSqlServer(builder.Configuration.GetConnectionString("Data Source=zwdmyad0001;Initial Catalog=MPP_DEV;Connection Timeout=180;Persist Security Info=True;User ID=MPP_DEV_APP;Password=LASyYbj0ZX#B;TrustServerCertificate=True")));

builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
    options.HttpsPort = 5001;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseMiddleware<ErrorHandlingMiddleware>();
}
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action}/{id}",
    defaults: new { controller = "Home", action = "Index", id = "{id?}" });

app.Run();