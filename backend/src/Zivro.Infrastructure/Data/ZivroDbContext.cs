namespace Zivro.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Zivro.Domain.Entities;

/// <summary>
/// EntityFramework DbContext for the Zivro application.
/// Manages database connections and entity configurations using Fluent API.
/// </summary>
public class ZivroDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the ZivroDbContext.
    /// </summary>
    /// <param name="options">DbContext options.</param>
    public ZivroDbContext(DbContextOptions<ZivroDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// DbSet for User entities.
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// DbSet for RefreshToken entities.
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    /// <summary>
    /// DbSet for AuditLog entities.
    /// </summary>
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    /// <summary>
    /// DbSet for EmailVerification entities.
    /// </summary>
    public DbSet<EmailVerification> EmailVerifications { get; set; } = null!;

    /// <summary>
    /// DbSet for TwoFactorAuth entities.
    /// </summary>
    public DbSet<TwoFactorAuth> TwoFactorAuths { get; set; } = null!;

    /// <summary>
    /// Configures the database schema and entity mappings.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        ConfigureUserEntity(modelBuilder);

        // Configure RefreshToken entity
        ConfigureRefreshTokenEntity(modelBuilder);

        // Configure AuditLog entity
        ConfigureAuditLogEntity(modelBuilder);

        // Configure EmailVerification entity
        ConfigureEmailVerificationEntity(modelBuilder);

        // Configure TwoFactorAuth entity
        ConfigureTwoFactorAuthEntity(modelBuilder);
    }

    /// <summary>
    /// Configures the User entity using Fluent API.
    /// </summary>
    private static void ConfigureUserEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);

            // Properties
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .IsRequired();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.PasswordHash)
                .IsRequired();

            entity.Property(e => e.EmailVerified)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.UpdatedAt)
                .IsRequired(false);

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_User_Email_Unique");

            // Relationships
            entity.HasMany(e => e.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Table name
            entity.ToTable("Users");
        });
    }

    /// <summary>
    /// Configures the RefreshToken entity using Fluent API.
    /// </summary>
    private static void ConfigureRefreshTokenEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);

            // Properties
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .IsRequired();

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.ExpiresAt)
                .IsRequired();

            entity.Property(e => e.IsRevoked)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.UpdatedAt)
                .IsRequired(false);

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes
            entity.HasIndex(e => e.Token)
                .IsUnique()
                .HasDatabaseName("IX_RefreshToken_Token_Unique");

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_RefreshToken_UserId");

            entity.HasIndex(e => e.ExpiresAt)
                .HasDatabaseName("IX_RefreshToken_ExpiresAt");

            // Foreign key
            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Table name
            entity.ToTable("RefreshTokens");
        });
    }

    /// <summary>
    /// Configures the AuditLog entity using Fluent API.
    /// </summary>
    private static void ConfigureAuditLogEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .IsRequired();

            entity.Property(e => e.UserId)
                .IsRequired(false);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.ActionType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.IsSuccessful)
                .IsRequired();

            entity.Property(e => e.FailureReason)
                .HasMaxLength(500);

            entity.Property(e => e.IpAddress)
                .HasMaxLength(45); // IPv6 max length

            entity.Property(e => e.UserAgent)
                .HasMaxLength(500);

            entity.Property(e => e.Metadata)
                .HasMaxLength(1000);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_AuditLog_UserId");

            entity.HasIndex(e => e.Email)
                .HasDatabaseName("IX_AuditLog_Email");

            entity.HasIndex(e => e.ActionType)
                .HasDatabaseName("IX_AuditLog_ActionType");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_AuditLog_CreatedAt");

            // Foreign key
            entity.HasOne(e => e.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.ToTable("AuditLogs");
        });
    }

    /// <summary>
    /// Configures the EmailVerification entity using Fluent API.
    /// </summary>
    private static void ConfigureEmailVerificationEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmailVerification>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .IsRequired();

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.VerificationToken)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.ExpiresAt)
                .IsRequired();

            entity.Property(e => e.VerifiedAt)
                .IsRequired(false);

            entity.Property(e => e.AttemptCount)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.UpdatedAt)
                .IsRequired(false);

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes
            entity.HasIndex(e => e.VerificationToken)
                .IsUnique()
                .HasDatabaseName("IX_EmailVerification_Token_Unique");

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_EmailVerification_UserId");

            // Foreign key
            entity.HasOne(e => e.User)
                .WithMany(u => u.EmailVerifications)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.ToTable("EmailVerifications");
        });
    }

    /// <summary>
    /// Configures the TwoFactorAuth entity using Fluent API.
    /// </summary>
    private static void ConfigureTwoFactorAuthEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TwoFactorAuth>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .IsRequired();

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.IsEnabled)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.SecretKey)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.BackupCodes)
                .HasMaxLength(1000);

            entity.Property(e => e.EnabledAt)
                .IsRequired(false);

            entity.Property(e => e.LastUsedAt)
                .IsRequired(false);

            entity.Property(e => e.FailedAttempts)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.UpdatedAt)
                .IsRequired(false);

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes
            entity.HasIndex(e => e.UserId)
                .IsUnique()
                .HasDatabaseName("IX_TwoFactorAuth_UserId_Unique");

            // Foreign key
            entity.HasOne(e => e.User)
                .WithOne(u => u.TwoFactorAuth)
                .HasForeignKey<TwoFactorAuth>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.ToTable("TwoFactorAuths");
        });
    }
}
