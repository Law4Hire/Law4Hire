using Law4Hire.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Law4Hire.Infrastructure.Data.Contexts;

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
    public DbSet<UserDocumentStatus> UserDocumentStatuses { get; set; }
    public DbSet<VisaDocumentRequirement> VisaDocumentRequirements { get; set; }
    public DbSet<VisaType> VisaTypes { get; set; }
    public DbSet<DocumentType> DocumentTypes { get; set; }
    public DbSet<UserVisa> UserVisas { get; set; }
    public DbSet<ScrapeLog> ScrapeLogs { get; set; }
    public DbSet<VisaTypeQuestion> VisaTypeQuestions { get; set; }
    public DbSet<VisaInterviewState> VisaInterviewStates { get; set; }
    public DbSet<WorkflowStep> WorkflowSteps { get; set; }
    public DbSet<WorkflowStepDocument> WorkflowStepDocuments { get; set; }
    public DbSet<VisaCategory> VisaCategories { get; set; }
    public DbSet<VisaSubCategory> VisaSubCategories { get; set; }
    public DbSet<BaseVisaType> BaseVisaTypes { get; set; }
    public DbSet<UserServicePackage> UserServicePackages { get; set; }
    public DbSet<CategoryVisaType> CategoryVisaTypes { get; set; }
    public DbSet<CategoryClass> CategoryClasses { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<USState> USStates { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<VisaWizard> VisaWizards { get; set; }
    public DbSet<VisaWorkflow> VisaWorkflows { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Configure decimal precision
        modelBuilder.Entity<ServicePackage>()
            .Property(p => p.BasePrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ServiceRequest>()
            .Property(p => p.AgreedPrice)
            .HasPrecision(18, 2);

        // Configure table names
        modelBuilder.Entity<VisaType>().ToTable("VisaTypes");
        modelBuilder.Entity<DocumentType>().ToTable("DocumentTypes");
        modelBuilder.Entity<BaseVisaType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Question1);
            entity.Property(e => e.Question2);
            entity.Property(e => e.Question3);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.ConfidenceScore).HasColumnType("decimal(3,2)");

            // Index for performance
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Status);
        });

        // CategoryClass configuration
        modelBuilder.Entity<CategoryClass>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClassCode).HasMaxLength(10).IsRequired();
            entity.Property(e => e.ClassName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.GeneralCategory).HasMaxLength(50);
            
            // Index for performance
            entity.HasIndex(e => e.ClassCode).IsUnique();
        });

        // CategoryVisaType configuration
        modelBuilder.Entity<CategoryVisaType>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Category)
                  .WithMany()
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.VisaType)
                  .WithMany(vt => vt.CategoryVisaTypes)
                  .HasForeignKey(e => e.VisaTypeId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint to prevent duplicate associations
            entity.HasIndex(e => new { e.CategoryId, e.VisaTypeId }).IsUnique();
        });

        // Country configuration
        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CountryCode).HasMaxLength(3).IsRequired();
            entity.Property(e => e.CountryCode2).HasMaxLength(2).IsRequired();
            
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.CountryCode).IsUnique();
            entity.HasIndex(e => e.CountryCode2).IsUnique();
        });

        // USState configuration
        modelBuilder.Entity<USState>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.StateCode).HasMaxLength(2).IsRequired();
            
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.StateCode).IsUnique();
        });

        // UserProfile configuration
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.User)
                  .WithOne(u => u.Profile)
                  .HasForeignKey<UserProfile>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CitizenshipCountry)
                  .WithMany()
                  .HasForeignKey(e => e.CitizenshipCountryId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.Property(e => e.MaritalStatus).HasMaxLength(20);
            entity.Property(e => e.EducationLevel).HasMaxLength(50);
        });

        // User citizenship country configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasOne(e => e.CitizenshipCountry)
                  .WithMany()
                  .HasForeignKey(e => e.CitizenshipCountryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ✅ Fix UserDocumentStatus relationships with NO ACTION to prevent cascade cycles
        modelBuilder.Entity<UserDocumentStatus>()
            .HasOne(uds => uds.DocumentType)
            .WithMany(dt => dt.UserDocumentStatuses)
            .HasForeignKey(uds => uds.DocumentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserDocumentStatus>()
            .HasOne(uds => uds.VisaType)
            .WithMany(vt => vt.UserDocumentStatuses)
            .HasForeignKey(uds => uds.VisaTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // ✅ Add explicit User relationship configuration for UserDocumentStatus
        modelBuilder.Entity<UserDocumentStatus>()
            .HasOne<User>()
            .WithMany(u => u.Documents)
            .HasForeignKey(uds => uds.UserId)
            .OnDelete(DeleteBehavior.NoAction); // Use NoAction to prevent cascade cycles

        // VisaDocumentRequirement relationships
        modelBuilder.Entity<VisaDocumentRequirement>()
            .HasOne(vdr => vdr.DocumentType)
            .WithMany(dt => dt.VisaDocumentRequirements)
            .OnDelete(DeleteBehavior.Restrict)
            .HasForeignKey(vdr => vdr.DocumentTypeId);

        modelBuilder.Entity<VisaDocumentRequirement>()
            .HasOne(vdr => vdr.VisaType)
            .WithMany(vt => vt.DocumentRequirements)
            .HasForeignKey(vdr => vdr.VisaTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // UserVisa relationships
        modelBuilder.Entity<UserVisa>().ToTable("UserVisas")
            .HasOne(uv => uv.VisaType)
            .WithMany(vt => vt.UserVisas)
            .HasForeignKey(uv => uv.VisaTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserVisa>()
            .HasOne(uv => uv.User)
            .WithMany()
            .HasForeignKey(uv => uv.UserId)
            .OnDelete(DeleteBehavior.NoAction); // Use NoAction to prevent cascade cycles

        // VisaTypeQuestion relationships
        modelBuilder.Entity<VisaTypeQuestion>()
            .HasOne(vq => vq.VisaType)
            .WithMany()
            .HasForeignKey(vq => vq.VisaTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        // ✅ VisaInterviewState relationship with User - one-to-one
        modelBuilder.Entity<VisaInterviewState>()
            .ToTable("VisaInterviewStates")
            .HasOne(vis => vis.User)
            .WithOne(u => u.VisaInterview)
            .HasForeignKey<VisaInterviewState>(vis => vis.UserId)
            .OnDelete(DeleteBehavior.NoAction); // Use NoAction to prevent cascade cycles


        modelBuilder.Entity<WorkflowStep>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.VisaType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.EstimatedCost).HasColumnType("decimal(10,2)");

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // WorkflowStepDocument configuration
        modelBuilder.Entity<WorkflowStepDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DocumentName).HasMaxLength(255).IsRequired();

            entity.HasOne(e => e.WorkflowStep)
                  .WithMany(ws => ws.Documents)
                  .HasForeignKey(e => e.WorkflowStepId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
        );
    }
}