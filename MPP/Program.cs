using DAL.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Model.Models;
using MPP;
using MPP.Filter;
using MPP.ViewComponents;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddSessionStateTempDataProvider();
builder.Services.AddRazorPages().AddSessionStateTempDataProvider();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddTransient<MenuViewComponent>();
builder.Services.AddTransient<SubMenuViewComponent>();
builder.Services.AddTransient<ShowAttributeViewComponent>();
builder.Services.AddTransient<GetSearchDataViewComponent>();
builder.Services.AddSingleton<LogError>();

// Add Session Services
builder.Services.AddMemoryCache();
builder.Services.AddSession();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".Mpp.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Customize the session timeout
});

//Add the IWebHostEnvironment service
builder.Services.AddSingleton(builder.Environment);

builder.Services.AddDbContext<MPP_Context>(
     options => options.UseSqlServer(builder.Configuration.GetConnectionString("Data Source=zwdmyad0001;Initial Catalog=MPP_DEV;Persist Security Info=True;User ID=MPP_DEV_APP;Password=LASyYbj0ZX#B;TrustServerCertificate=True")));  

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
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action}/{id}",
    defaults: new { controller = "Home", action = "Index", id = "{id?}" });


app.Run();