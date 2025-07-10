using Law4Hire.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Xml.Linq;

namespace Law4Hire.Infrastructure.Data.Contexts;

//public class Law4HireDbContext(DbContextOptions<Law4HireDbContext> options) : DbContext(options)
//{
public class Law4HireDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public Law4HireDbContext(DbContextOptions<Law4HireDbContext> options) : base(options)
    {
    }
    public DbSet<ServicePackage> ServicePackages { get; set; }
    public DbSet<IntakeSession> IntakeSessions { get; set; }
    public DbSet<IntakeQuestion> IntakeQuestions { get; set; }
    public DbSet<ServiceRequest> ServiceRequests { get; set; }
    public DbSet<LocalizedContent> LocalizedContents { get; set; }
    public DbSet<LegalProfessional> LegalProfessionals { get; set; }
    public DbSet<UserDocumentStatus> UserDocumentStatuses { get; set; }
    public DbSet<VisaDocumentRequirement> VisaDocumentRequirements { get; set; }
    public DbSet<VisaGroup> VisaGroups { get; set; }
    public DbSet<VisaType> VisaTypes { get; set; }
    public DbSet<DocumentType> DocumentTypes { get; set; }
    public DbSet<UserVisa> UserVisas { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Law4HireDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
        // This will automatically apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.Entity<ServicePackage>()
            .Property(p => p.BasePrice)
            .HasPrecision(18, 2);  // Or whatever suits your pricing logic

        modelBuilder.Entity<ServiceRequest>()
            .Property(p => p.AgreedPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<VisaType>().ToTable("VisaTypes");
        modelBuilder.Entity<VisaGroup>().ToTable("VisaGroups");
        modelBuilder.Entity<DocumentType>().ToTable("DocumentTypes");

        modelBuilder.Entity<UserDocumentStatus>()
        .HasOne(uds => uds.DocumentType)
        .WithMany(dt => dt.UserDocumentStatuses)
        .HasForeignKey(uds => uds.DocumentTypeId)
        .OnDelete(DeleteBehavior.Restrict); // or Cascade, based on your logic

        modelBuilder.Entity<VisaDocumentRequirement>()
            .HasOne(vdr => vdr.DocumentType)
            .WithMany(dt => dt.VisaDocumentRequirements)
            .OnDelete(DeleteBehavior.Restrict)
            .HasForeignKey(vdr => vdr.DocumentTypeId);

        // VisaDocumentRequirement -> VisaType
        modelBuilder.Entity<VisaDocumentRequirement>()
            .HasOne(vdr => vdr.VisaType)
            .WithMany(vt => vt.DocumentRequirements)
            .HasForeignKey(vdr => vdr.VisaTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // UserDocumentStatus -> VisaType
        modelBuilder.Entity<UserDocumentStatus>()
            .HasOne(uds => uds.VisaType)
            .WithMany(vt => vt.UserDocumentStatuses)
            .HasForeignKey(uds => uds.VisaTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    
        modelBuilder.Entity<UserVisa>().ToTable("UserVisas")
            .HasOne(uv => uv.VisaType)
            .WithMany(vt => vt.UserVisas)
            .HasForeignKey(uv => uv.VisaTypeId);

        modelBuilder.Entity<VisaType>()
            .HasOne(v => v.VisaGroup)
            .WithMany(g => g.VisaTypes)
            .HasForeignKey(v => v.VisaGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserVisa>()
            .HasOne(uv => uv.User)
            .WithMany()
            .HasForeignKey(uv => uv.UserId);

        modelBuilder.Entity<LegalProfessional>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<LegalProfessional>()
            .HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<LegalProfessional>(x => x.Id);
    }
}