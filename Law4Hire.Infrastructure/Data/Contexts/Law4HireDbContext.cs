﻿using Law4Hire.Core.Entities;
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
    public DbSet<ScrapeLog> ScrapeLogs { get; set; }


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

        // Seed data for visa types
        modelBuilder.Entity<VisaType>().HasData(
            new VisaType
            {
                Id = Guid.Parse("162E3E30-EC8B-438E-8F96-E836465D0908"),
                Name = "H1B Specialty Occupation",
                Description = "Work visa for specialty occupations",
                Category = "Work",
                VisaGroupId = Guid.Parse("44444444-4444-4444-4444-444444444444")
            }
        );

        // Seed data for document types
        modelBuilder.Entity<DocumentType>().HasData(
            new DocumentType
            {
                Id = Guid.Parse("3E27B6A3-8A52-4185-854C-5F584AEA28E8"),
                FormNumber = "I-129",
                Name = "Petition for a Nonimmigrant Worker",
                Description = "For H-1B and other workers",
                IssuingAgency = "USCIS"
            },
            new DocumentType
            {
                Id = Guid.Parse("678634B3-DE95-4F16-83C9-DC86AAD68723"),
                FormNumber = "DS-160",
                Name = "Online Nonimmigrant Visa Application",
                Description = "Application for temporary visas",
                IssuingAgency = "DOS"
            },
            new DocumentType
            {
                Id = Guid.Parse("FE86CA4B-3808-482B-89FB-E2FC9375684B"),
                FormNumber = "I-864",
                Name = "Affidavit of Support",
                Description = "Sponsor financial support form",
                IssuingAgency = "USCIS"
            }
        );

        // Seed visa document requirements with fixed GUIDs for migration support
        modelBuilder.Entity<VisaDocumentRequirement>().HasData(
            new VisaDocumentRequirement
            {
                Id = Guid.Parse("51860CFE-BA25-4461-8134-77C8FE6939FE"),
                VisaTypeId = Guid.Parse("162E3E30-EC8B-438E-8F96-E836465D0908"),
                DocumentTypeId = Guid.Parse("3E27B6A3-8A52-4185-854C-5F584AEA28E8"),
                IsRequired = true
            },
            new VisaDocumentRequirement
            {
                Id = Guid.Parse("44E14FD0-418F-4719-8E94-A2FB862FAF0F"),
                VisaTypeId = Guid.Parse("162E3E30-EC8B-438E-8F96-E836465D0908"),
                DocumentTypeId = Guid.Parse("678634B3-DE95-4F16-83C9-DC86AAD68723"),
                IsRequired = true
            },
            new VisaDocumentRequirement
            {
                Id = Guid.Parse("E8E5455E-1878-4B73-AFCA-33A32D4B66ED"),
                VisaTypeId = Guid.Parse("162E3E30-EC8B-438E-8F96-E836465D0908"),
                DocumentTypeId = Guid.Parse("FE86CA4B-3808-482B-89FB-E2FC9375684B"),
                IsRequired = true
            }
        );
    }
}