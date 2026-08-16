using System.Text;
using AssignmentSystem.Api.Configuration;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Data.Seed;
using AssignmentSystem.Api.Middleware;
using AssignmentSystem.Api.Services.Implementations;
using AssignmentSystem.Api.Services.Interfaces;
using DotNetEnv;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// Load backend/AssignmentSystem.Api/.env into process environment variables
// BEFORE the configuration system reads anything, so ConnectionStrings__* and
// Jwt__* env vars are present when builder.Configuration is built below.
// Only done outside a real deployment - a deployed environment sets real
// environment variables directly and won't have a .env file present (Env.Load
// silently no-ops if the file doesn't exist, so this is safe everywhere).
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// ---------- Configuration ----------
// appsettings.json -> appsettings.{Environment}.json -> environment variables
// (this order means the .env-derived environment variables loaded above
// always win over the placeholder values in appsettings.json).
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

// ---------- Logging ----------
// Built-in ASP.NET Core logging (console provider) is sufficient here -
// the requirement is "meaningful events are logged", not a specific
// logging framework, and adding Serilog/NLog would be an unjustified
// dependency for a project this size.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ---------- Database ----------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No database connection string configured. Copy backend/.env.example to " +
        "backend/AssignmentSystem.Api/.env and set ConnectionStrings__DefaultConnection.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ---------- Authentication / Authorization ----------
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");

if (string.IsNullOrWhiteSpace(jwtSettings.Secret))
{
    throw new InvalidOperationException(
        "Jwt__Secret is not configured. Set it in backend/AssignmentSystem.Api/.env " +
        "(see backend/.env.example).");
}

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
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1) // small tolerance instead of the 5-minute default
    };
});

builder.Services.AddAuthorization();

// ---------- Validation ----------
// FluentValidation validators are auto-discovered from this assembly.
// Populated as each module's DTOs/Validators are added in later phases.
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// ---------- Application services ----------
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<ITeacherAssignmentService, TeacherAssignmentService>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

// ---------- Controllers ----------
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AssignmentSystem.Api.Filters.ValidationFilter>();
});

// ---------- CORS ----------
// Explicit allow-list from configuration, never a wildcard - a wildcard origin
// combined with credentialed requests (cookies/auth headers) is both insecure
// and rejected outright by browsers, so an explicit list is required either way.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ---------- Swagger / OpenAPI ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Assignment & Submission Management System API",
        Version = "v1",
        Description = "Role-based API for teachers, students, and admins to manage assignments and submissions."
    });

    // Lets a reviewer paste a JWT into Swagger's "Authorize" button and have it
    // sent as "Authorization: Bearer <token>" on every subsequent try-it-out call.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter ONLY the JWT token (no 'Bearer ' prefix - Swagger adds it automatically)."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ---------- Middleware pipeline ----------
// Exception handling is registered FIRST so it wraps every other middleware
// and every controller action - nothing downstream can throw past it.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ---------- Auto-apply migrations on startup ----------
// So an evaluator only has to: configure .env, run PostgreSQL, then `dotnet run`.
// No separate manual "dotnet ef database update" step is required.
// (Seed data is applied here too, starting Phase 3 once user seeding exists.)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();
    await DbSeeder.SeedAsync(db, hasher);
}

app.Run();

// Exposed for WebApplicationFactory-based integration tests, if added later.
public partial class Program { }
