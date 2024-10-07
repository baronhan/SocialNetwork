using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyMVCApp.Services;
using Neo4j.Services;

var builder = WebApplication.CreateBuilder(args);

// Thêm MVC với TempData Provider
var mvcBuilder = builder.Services.AddControllersWithViews();
mvcBuilder.AddSessionStateTempDataProvider();

// Cấu hình session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Đăng ký Neo4jService
builder.Services.AddScoped<Neo4jService>(provider =>
{
    var configuration = builder.Configuration;
    var uri = configuration["Neo4j:Uri"];
    var username = configuration["Neo4j:Username"];
    var password = configuration["Neo4j:Password"];
    return new Neo4jService(uri, username, password);
});

// Cấu hình kích thước tối đa cho tải lên file
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // Ví dụ: 100MB
});

// Đăng ký UserService
builder.Services.AddScoped<UserService>();

// Đăng ký IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Đăng ký EmailSender
builder.Services.AddScoped<IEmailSender, EmailSenderService>();

var app = builder.Build();

// Cấu hình pipeline HTTP request.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Đảm bảo Session được gọi trước Routing
app.UseSession();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=SignUp}/{action=SignIn}/{id?}");

app.Run();
