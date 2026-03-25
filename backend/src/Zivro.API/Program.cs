using Zivro.Application.Interfaces;
using Zivro.Application.Services;
using Zivro.Domain.Interfaces;
using Zivro.Infrastructure.Data;
using Zivro.Infrastructure.Repositories;
using Zivro.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add database
builder.Services.AddDbContext<ZivroDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.MigrationsAssembly("Zivro.Infrastructure")
    )
);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT security
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Zivro API",
        Version = "v1",
        Description = "API de autenticação e autorização da plataforma Zivro",
        Contact = new OpenApiContact
        {
            Name = "Zivro Team",
            Email = "dev@zivro.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License"
        }
    });

    // Define JWT Bearer scheme
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", securityScheme);

    // Require JWT for all endpoints by default
    var securityRequirement = new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    };

    options.AddSecurityRequirement(securityRequirement);

    // XML comments for API documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Add Authentication & Authorization
builder.Services.AddSingleton<IPasswordHasher, Sha256PasswordHasher>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
builder.Services.AddScoped<ITwoFactorAuthRepository, TwoFactorAuthRepository>();

// Add Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
builder.Services.AddScoped<ITwoFactorAuthService, TwoFactorAuthService>();

// Configure JWT Bearer Authentication with real JWT validation
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secretKey = builder.Configuration["JWT:SecretKey"] ?? throw new InvalidOperationException("JWT:SecretKey is not configured");
        var issuer = builder.Configuration["JWT:Issuer"] ?? "Zivro.API";
        var audience = builder.Configuration["JWT:Audience"] ?? "Zivro.App";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Configure Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Register, Login, VerifyEmail rate limiters
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("auth-register", limiterOptions =>
    {
        limiterOptions.PermitLimit = int.Parse(builder.Configuration["RateLimiting:RegisterLimitPerHour"] ?? "5");
        limiterOptions.Window = TimeSpan.FromHours(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("auth-login", limiterOptions =>
    {
        limiterOptions.PermitLimit = int.Parse(builder.Configuration["RateLimiting:LoginLimitPerHour"] ?? "10");
        limiterOptions.Window = TimeSpan.FromHours(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("email-verify", limiterOptions =>
    {
        limiterOptions.PermitLimit = int.Parse(builder.Configuration["RateLimiting:VerifyEmailLimitPerHour"] ?? "20");
        limiterOptions.Window = TimeSpan.FromHours(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ZivroDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    try
    {
        // Apply pending migrations
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("✓ Database migrations applied successfully.");

        // Seed power user
        await SeedData.SeedAsync(dbContext, passwordHasher);

        // Uncomment to seed test users in development
        if (app.Environment.IsDevelopment())
        {
            // await SeedData.SeedTestUsersAsync(dbContext, passwordHasher);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Error during database initialization: {ex.Message}");
        if (app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Add rate limiting middleware
app.UseRateLimiter();

// Order matters: Authentication before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
