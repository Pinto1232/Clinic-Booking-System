using System.Text;
using ClinicBookingSystem.Application.Services;
using ClinicBookingSystem.Domain.Interfaces;
using ClinicBookingSystem.Infrastructure.Data;
using ClinicBookingSystem.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Add DbContext with SQLite
builder.Services.AddDbContext<ClinicBookingSystemDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

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
        ValidIssuer = jwtSettings["Issuer"] ?? "ClinicBookingSystem",
        ValidAudience = jwtSettings["Audience"] ?? "ClinicBookingSystemUsers",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configure CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173", 
                "https://localhost:5173", 
                "http://localhost:5000", 
                "https://localhost:5001",
                "http://localhost:5074",
                "https://localhost:7074",
                "http://localhost:5139",
                "https://localhost:7205")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Register repositories
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<ITimeSlotRepository, TimeSlotRepository>();

// Register services
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IAuthService, ClinicBookingSystem.Application.Services.AuthService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<ITimeSlotService, TimeSlotService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created and seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ClinicBookingSystemDbContext>();
    dbContext.Database.EnsureCreated();
    
    // Seed doctors if none exist
    if (!dbContext.Doctors.Any())
    {
        var doctors = new[]
        {
            new ClinicBookingSystem.Domain.Entities.Doctor 
            { 
                FirstName = "John", 
                LastName = "Smith", 
                Email = "john.smith@clinic.com", 
                Specialization = "General Medicine", 
                LicenseNumber = "GM001",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            },
            new ClinicBookingSystem.Domain.Entities.Doctor 
            { 
                FirstName = "Sarah", 
                LastName = "Johnson", 
                Email = "sarah.johnson@clinic.com", 
                Specialization = "Pediatrics", 
                LicenseNumber = "PD002",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            },
            new ClinicBookingSystem.Domain.Entities.Doctor 
            { 
                FirstName = "Michael", 
                LastName = "Williams", 
                Email = "michael.williams@clinic.com", 
                Specialization = "Cardiology", 
                LicenseNumber = "CD003",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            },
            new ClinicBookingSystem.Domain.Entities.Doctor 
            { 
                FirstName = "Emily", 
                LastName = "Brown", 
                Email = "emily.brown@clinic.com", 
                Specialization = "Dermatology", 
                LicenseNumber = "DM004",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            },
            new ClinicBookingSystem.Domain.Entities.Doctor 
            { 
                FirstName = "David", 
                LastName = "Davis", 
                Email = "david.davis@clinic.com", 
                Specialization = "Orthopedics", 
                LicenseNumber = "OR005",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            }
        };
        
        dbContext.Doctors.AddRange(doctors);
        dbContext.SaveChanges();
        Console.WriteLine("Seeded 5 doctors into the database.");
    }
}

app.Run();
