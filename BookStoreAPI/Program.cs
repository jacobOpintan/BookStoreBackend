using System.Text;
using BookStoreAPI.Data;
using BookStoreAPI.Endpoints;
using BookStoreAPI.Models;
using BookStoreAPI.Repositories;
using BookStoreAPI.Roles;
using BookStoreAPI.Services;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BookStoreAPI.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers(); // Enables controllers


//registering the emailservice
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<MailHelper>();



// Setting up the connection string
var connString = builder.Configuration.GetConnectionString("BookStoreDbContext");

// Configuring services for SQL Server
builder.Services.AddDbContext<BookStoreDbContext>(options => options.UseSqlServer(connString));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<BookStoreDbContext>()
    .AddDefaultTokenProviders();

    //Console.WriteLine($"JWT Key: {builder.Configuration["Jwt:Key"]}");


// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
    };
});

// Add Authorization
builder.Services.AddAuthorization();

// Registering the repository (Dependency Injection) for the data
builder.Services.AddScoped<IBookRepository, BookDbEfCoreRepository>();

var app = builder.Build();

//creating roles
using (var scope = app.Services.CreateScope())
{

    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    

    // ensure roles exist
    string[] roles = { "Admin", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    //ensure an admin exists
    string adminEmail = "admin@bookstore.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Admin"
        };

        var result = await userManager.CreateAsync(adminUser, "Admin@123"); // Strong password

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine("✅ Admin created successfully!");
        }
        else
        {
            Console.WriteLine("❌ Failed to create admin:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($" - {error.Description}");
            }
        }
    }
    else
    {
        Console.WriteLine("⚠️ Admin already exists.");
    }
}

// calling the method to seed the roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await RoleSeeder.SeedRolesAsync(services);
}


// Middleware Order
app.UseHttpsRedirection(); // Redirect HTTP to HTTPS
app.UseRouting(); // Ensure Routing is Properly Enabled
app.UseAuthentication(); // Enable Authentication
app.UseAuthorization(); // Enable Authorization
//app.UseRouting(); // Ensure Routing is Properly Enabled

// Mapping Controllers
app.MapControllers();

// API Test Endpoints
app.MapGet("/", () => "Hello World!");
app.MapGet("/books", async (IBookRepository repository) => await repository.GetAllAsync()).RequireAuthorization();

// Fully mapping API Endpoints
app.MapBooksEndPoint();

// Fallback Route for Undefined Routes
app.MapFallback(() => Results.NotFound(new { message = "Oops! The page you requested does not exist" }));

// Start the Application
app.Run();
