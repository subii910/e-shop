using e_shop.Models;
using Microsoft.EntityFrameworkCore;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<Customerwebsite1Context>(x => x.UseSqlServer
(builder.Configuration.GetConnectionString("dbShoppingStore")));
builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.LoginPath = "/Home/Login"; // redirect if not logged in
    });

builder.Services.AddAuthorization();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Services.Configure<StripeSettings>(
    builder.Configuration.GetSection("Stripe"));

StripeConfiguration.ApiKey = "sk_test_51RsOPRBRjkkGDzT6l9Xq9tVUghcKuGRhd4MER5UPR4L92sGutKbe2Oo3igteVF4HbtvvCYVRqRN56dhx0Cwqi35G001vRMS4xj";


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();




