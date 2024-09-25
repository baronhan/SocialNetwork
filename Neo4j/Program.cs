using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyMVCApp.Services;
using Neo4j.Services;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();

// Cấu hình session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register Neo4jService as Scoped
builder.Services.AddScoped<Neo4jService>(provider =>
{
    var configuration = builder.Configuration;
    var uri = configuration["Neo4j:Uri"];
    var username = configuration["Neo4j:Username"];
    var password = configuration["Neo4j:Password"];
    return new Neo4jService(uri, username, password);
});

builder.Services.AddControllersWithViews();

// Cấu hình kích thước tối đa cho tải lên file
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // Ví dụ: 100MB
});

// Register UserService
builder.Services.AddScoped<UserService>();

// Đăng ký IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=SignUp}/{action=SignIn}/{id?}");

app.Run();
