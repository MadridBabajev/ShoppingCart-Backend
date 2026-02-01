using System.Security.Claims;
using System.Text;
using App.BLL;
using App.BLL.Contracts;
using App.DAL;
using App.DAL.Contracts;
using App.DAL.Seeding;
using App.Domain.Identity;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebApp;

var builder = WebApplication.CreateBuilder(args);

// ===================== Set up dependency injection for the container =====================
// Get the connection string for Postgres database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configure Postgres database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Enable legacy timestamp behavior for Npgsql
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Dependency Injection: Register Unit of Work (UOW) and BLL layers
builder.Services.AddScoped<IAppUOW, AppUOW>();
builder.Services.AddScoped<IAppBLL, AppBLL>();
builder.Services.AddSingleton<DataGuids>();

// Configure Identity system with custom AppUser and AppRole, no confirmed account requirement
builder.Services
    .AddIdentity<AppUser, AppRole>(
        options => options.SignIn.RequireConfirmedAccount = false)
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Register UserManager and RoleManager services for identity management
builder.Services.AddScoped<UserManager<AppUser>>();
builder.Services.AddScoped<RoleManager<AppRole>>();

// Configure authentication to use JWT Bearer tokens
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true; // Store token in HttpContext for easier access
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true, // Ensure token hasn't expired
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!)
            ),
            ClockSkew = TimeSpan.Zero, // No time skew for token expiration
        };
    });

// Configure authorization policies
builder.Services.AddAuthorization(options =>
{
    // Optional: Allows access without authentication (for example, for public APIs)
    options.AddPolicy("Optional", policy =>
    {
        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAssertion(_ => true);
    });
    
    // DefaultPolicy: Require authenticated user and a valid NameIdentifier claim
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();  
        policy.RequireClaim(ClaimTypes.NameIdentifier);
    });
});

// Enable CORS to allow cross-origin requests from any origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsAllowAll", policy =>
    {
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.AllowAnyOrigin();
    });
});

// Register AutoMapper with configurations from AutomapperConfig class
builder.Services.AddAutoMapper(typeof(AutomapperConfig));

// Configure API versioning and explorer for Swagger documentation
builder.Services.AddApiVersioning(options =>
    {
        options.ReportApiVersions = true;
        options.DefaultApiVersion = new ApiVersion(1, 0); // Default API version
    })
    .AddApiExplorer(options =>
    {
        // Format API version as 'v1', 'v2', etc. in Swagger
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>(); // Swagger config
builder.Services.AddSwaggerGen();

// =========================================
var app = builder.Build();
// =========================================

// ===================== Pipeline Setup =====================

// Initialize and configure the database, including seeding data
SetupAppData(app, app.Configuration, app.Environment);

// Configure middleware for development and production environments
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint(); // Automatically apply migrations during development
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Use global exception handler in production
    app.UseHsts(); // Enable HTTP Strict Transport Security for production
}

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
app.UseStaticFiles(); // Serve static files (e.g., images, CSS, JavaScript)
app.UseRouting(); // Enable routing for MVC and API controllers

app.UseCors("CorsAllowAll"); // Apply the CORS policy to allow all origins

app.UseAuthentication(); // Enable authentication middleware
app.UseAuthorization(); // Enable authorization middleware

// Configure Swagger for API documentation and versioning
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json",
            description.GroupName
        );
    }
});

// Application is set to listen on all interfaces
builder.WebHost.UseUrls("http://*:80");

// Map routes for controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ===================== Run the server and start waiting for requests =====================
app.Run();

// Method to configure database, apply migrations, and seed data
static void SetupAppData(IApplicationBuilder app, IConfiguration configuration, IWebHostEnvironment env)
{
    using var serviceScope = app.ApplicationServices
        .GetRequiredService<IServiceScopeFactory>()
        .CreateScope();
    using var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

    // Get the DataGuids instance for initial seeding
    var guids = serviceScope.ServiceProvider.GetRequiredService<DataGuids>();

    if (context == null)
    {
        throw new ApplicationException("Problem in services. Can't initialize DB Context");
    }

    using var userManager = serviceScope.ServiceProvider.GetService<UserManager<AppUser>>();
    using var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<AppRole>>();
    
    if (userManager == null || roleManager == null)
    {
        throw new ApplicationException("Problem in services. Can't initialize UserManager or RoleManager");
    }

    var logger = serviceScope.ServiceProvider.GetService<ILogger<IApplicationBuilder>>();
    if (logger == null)
    {
        throw new ApplicationException("Problem in services. Can't initialize logger");
    }

    // Drop and recreate database if configured
    if (env.IsDevelopment() && configuration.GetValue<bool>("DataInit:DropDatabase"))
    {
        logger.LogWarning("Dropping database");
        AppDataInit.DropDatabase(context);
    }

    // Apply migrations if configured
    if (configuration.GetValue<bool>("DataInit:MigrateDatabase"))
    {
        logger.LogInformation("Migrating database");
        AppDataInit.MigrateDatabase(context);
    }

    // Seed identity-related data (users, roles) if configured
    if (configuration.GetValue<bool>("DataInit:SeedIdentity"))
    {
        logger.LogInformation("Seeding identity");
        AppDataInit.SeedIdentity(userManager, context, guids);
    }

    // Seed application-specific data if configured
    if (configuration.GetValue<bool>("DataInit:SeedData"))
    {
        logger.LogInformation("Seeding app data");
        AppDataInit.SeedAppData(context, Path.Combine(env.WebRootPath, "imgs"), guids);
    }
}


/// <summary>
/// Partial Program class for running the SetupAppData function 
/// </summary>
public partial class Program
{
}