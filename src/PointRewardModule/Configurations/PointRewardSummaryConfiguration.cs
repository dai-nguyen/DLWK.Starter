using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PointRewardModule.Configurations
{
    public class PointRewardSummaryConfiguration : IEntityTypeConfiguration<Entities.PointRewardSummary>
    {
        public void Configure(EntityTypeBuilder<Entities.PointRewardSummary> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.Property(_ => _.Id).HasMaxLength(37);

            builder.Property(_ => _.UserId).HasMaxLength(100);
            builder.HasIndex(_ => _.UserId);

            builder.Property(_ => _.TotalPoint);
        }
    }
}
