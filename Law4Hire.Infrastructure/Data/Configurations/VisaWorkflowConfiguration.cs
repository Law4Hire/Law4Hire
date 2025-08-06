using Law4Hire.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Law4Hire.Infrastructure.Data.Configurations;

/// <summary>
/// EF configuration for the VisaWorkflow entity.
/// </summary>
public class VisaWorkflowConfiguration : IEntityTypeConfiguration<VisaWorkflow>
{
    public void Configure(EntityTypeBuilder<VisaWorkflow> builder)
    {
        builder.ToTable("VisaWorkflows");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.WorkflowJson)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .IsRequired();

        builder.Property(e => e.Hash)
            .HasMaxLength(64);

        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(e => e.CountryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<BaseVisaType>()
            .WithMany()
            .HasForeignKey(e => e.VisaTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.CountryId, e.VisaTypeId })
            .IsUnique();
    }
}