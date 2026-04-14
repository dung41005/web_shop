using Microsoft.EntityFrameworkCore;
using UC.eComm.Publish.Context;
using UC.eComm.Publish.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

////builder.Services.ConfigureApplicationCookie(options =>
////{
////    options.Cookie.Name = "cart_cookie";
////    options.Cookie.HttpOnly = true;
////    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
////    options.Cookie.MaxAge = TimeSpan.FromMinutes(60);
////    //options.LoginPath = "/Login";
////    //options.LogoutPath = "/Logout";
////});
////builder.Services.ConfigureApplicationCookie(options =>
////{
////    options.Cookie.Name = "address_cookie";
////    options.Cookie.HttpOnly = true;
////    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
////    options.Cookie.MaxAge = TimeSpan.(60);
////    //options.LoginPath = "/Login";
////    //options.LogoutPath = "/Logout";
////});

builder.Services.AddTransient<JsonService>();

builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(240); // Thời gian tồn tại của session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Bắt buộc cho GDPR
});
builder.Services.AddHttpClient<GeocodingService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "product-details",
        pattern: "product/{slug}-{id}",
        defaults: new { controller = "Product", action = "Detail" });
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});
app.Run();
