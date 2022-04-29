using ApplicationCore.Modules.PointReward.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApplicationCore.Modules.PointReward.Configurations
{
    public class PointRewardSummaryConfiguration : IEntityTypeConfiguration<PointRewardSummary>
    {
        public void Configure(EntityTypeBuilder<PointRewardSummary> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.Property(_ => _.Id).HasMaxLength(37);

            builder.Property(_ => _.UserId).HasMaxLength(100);
            builder.HasIndex(_ => _.UserId);

            builder.Property(_ => _.TotalPoint);
        }
    }
}
