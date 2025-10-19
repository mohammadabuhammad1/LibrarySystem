using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Infrastructure.Data;

public class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Library> Libraries { get; set; }
    public DbSet<BorrowRecord> BorrowRecords { get; set; }

    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<UserOrganizationUnit> UserOrganizationUnits { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        base.OnModelCreating(builder);

        // ApplicationUser config
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.MembershipDate).IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        });

        // Library config
        builder.Entity<Library>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Location).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
        });

        // Book config
        builder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Author).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ISBN).IsRequired().HasMaxLength(20);
            entity.Property(e => e.PublishedYear).IsRequired();
            entity.Property(e => e.TotalCopies).IsRequired();
            entity.Property(e => e.CopiesAvailable).IsRequired();
            entity.HasIndex(e => e.ISBN).IsUnique();
            entity.HasOne(b => b.Library).WithMany(l => l.Books).HasForeignKey(b => b.LibraryId).OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
        });

        // BorrowRecord config
        builder.Entity<BorrowRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BorrowDate).IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.DueDate).IsRequired();
            entity.Property(e => e.FineAmount).HasPrecision(10, 2);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.HasOne(br => br.Book).WithMany(b => b.BorrowRecords).HasForeignKey(br => br.BookId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(br => br.User).WithMany(u => u.BorrowRecords).HasForeignKey(br => br.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
        });

        builder.Entity<OrganizationUnit>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(e => e.Code)
                .IsUnique();

            entity.Property(e => e.DisplayName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.Type)
                .HasMaxLength(50);

            entity.Property(e => e.ContactEmail)
                .HasMaxLength(200);

            entity.Property(e => e.ContactPhone)
                .HasMaxLength(50);

            // Self-referencing relationship for hierarchy
            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.ParentId);
            entity.HasIndex(e => e.IsActive);
        });

        // NEW: UserOrganizationUnit configuration (Many-to-Many)
        builder.Entity<UserOrganizationUnit>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserOrganizationUnits)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.OrganizationUnit)
                .WithMany(ou => ou.UserOrganizationUnits)
                .HasForeignKey(e => e.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Cascade);

            // Composite index for uniqueness
            entity.HasIndex(e => new { e.UserId, e.OrganizationUnitId })
                .IsUnique();

            entity.HasIndex(e => e.IsDefault);
        });

        // ApplicationUser additional configuration
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(50);
        });

    }
}
