using CheckInSystem.Application.Interfaces.Repositories;
using CheckInSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CheckInSystem.Infrastructure.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserToken> UserTokens => Set<UserToken>();
    public DbSet<CheckInRecord> CheckInRecords => Set<CheckInRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserName).HasMaxLength(50).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.HasIndex(x => x.UserName).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(250);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(250);
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(x => new { x.RoleId, x.PermissionId });
            entity.HasOne(x => x.Role).WithMany(x => x.RolePermissions).HasForeignKey(x => x.RoleId);
            entity.HasOne(x => x.Permission).WithMany(x => x.RolePermissions).HasForeignKey(x => x.PermissionId);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(x => new { x.UserId, x.RoleId });
            entity.HasOne(x => x.User).WithMany(x => x.UserRoles).HasForeignKey(x => x.UserId);
            entity.HasOne(x => x.Role).WithMany(x => x.UserRoles).HasForeignKey(x => x.RoleId);
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.ToTable("UserTokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.RevokedReason).HasMaxLength(250);
            entity.Property(x => x.GeneratedBy).HasMaxLength(50);
            entity.HasIndex(x => x.TokenId).IsUnique();
            entity.HasIndex(x => new { x.UserId, x.IsCurrent }).HasFilter("[IsCurrent] = 1").IsUnique();
            entity.HasOne(x => x.User).WithMany(x => x.UserTokens).HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<CheckInRecord>(entity =>
        {
            entity.ToTable("CheckInRecords");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SubmittedTokenId).HasMaxLength(100);
            entity.Property(x => x.FailureReason).HasMaxLength(250);
            entity.Property(x => x.SourceIp).HasMaxLength(100);
            entity.Property(x => x.UserAgent).HasMaxLength(500);
            entity.HasOne(x => x.User).WithMany(x => x.CheckInRecords).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.UserToken).WithMany().HasForeignKey(x => x.UserTokenId).OnDelete(DeleteBehavior.NoAction);
        });

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseAuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
