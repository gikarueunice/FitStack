using FitStackDBL.Model;
using FitStackDBL.Repository;
using FitStackDBL.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

//builder.Services.AddScoped<IUsersRepository>(sp =>
//{
//    var configuration = sp.GetRequiredService<IConfiguration>();

//    var connectionString = configuration.GetConnectionString("DefaultConnection")
//        ?? throw new InvalidOperationException("Connection string not found");

//    return new UsersRepository(connectionString);
//});
try
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    using var testConnection = new SqlConnection(connectionString);
    await testConnection.OpenAsync();
    Console.WriteLine("Database connection successful!");

    // Ensure database exists
    using var command = testConnection.CreateCommand();
    command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users'";
    var tableExists = (int)await command.ExecuteScalarAsync();

    if (tableExists == 0)
    {
        Console.WriteLine("Users table not found. Please run the database creation script.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Database connection failed: {ex.Message}");
}

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";

        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;

        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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