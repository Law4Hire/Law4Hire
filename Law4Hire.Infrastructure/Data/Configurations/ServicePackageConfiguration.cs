using Law4Hire.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Law4Hire.Infrastructure.Configurations
{
    public class ServicePackageConfiguration : IEntityTypeConfiguration<ServicePackage>
    {
        public void Configure(EntityTypeBuilder<ServicePackage> builder)
        {
            builder.Property(p => p.BasePrice)
                   .HasPrecision(18, 2);
                   
            builder.Property(p => p.L4HLLCFee)
                   .HasPrecision(18, 2);
        }
    }
}
