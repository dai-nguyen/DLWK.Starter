using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApplicationCore.Modules.PointReward.Configurations
{
    public class PointRewardConfiguration : IEntityTypeConfiguration<Entities.PointReward>
    {
        public void Configure(EntityTypeBuilder<Entities.PointReward> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.Property(_ => _.Id).HasMaxLength(37);

            builder.Property(_ => _.UserId).HasMaxLength(100);
            builder.HasIndex(_ => _.UserId);

            builder.Property(_ => _.Point);
            builder.Property(_ => _.Notes).HasMaxLength(255);

            // full index
            builder.HasGeneratedTsVectorColumn(_ =>
                _.SearchVector,
                "english",
                _ => _.Notes)
                .HasIndex(_ => _.SearchVector)
                .HasMethod("GIN");
        }
    }
}
