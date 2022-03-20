using ApplicationCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApplicationCore.Data.Configurations
{
    public class BulkJobConfiguration : IEntityTypeConfiguration<BulkJob>
    {
        public void Configure(EntityTypeBuilder<BulkJob> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.Property(_ => _.Id).HasMaxLength(37);

            builder.Property(_ => _.Status).HasMaxLength(100);
            builder.Property(_ => _.Error);
            builder.Property(_ => _.Processed);
            builder.Property(_ => _.Failed);
        }
    }
}
