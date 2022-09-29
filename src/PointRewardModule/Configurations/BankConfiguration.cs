using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PointRewardModule.Configurations
{
    public class BankConfiguration : IEntityTypeConfiguration<Entities.Bank>
    {
        public void Configure(EntityTypeBuilder<Entities.Bank> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.Property(_ => _.Id).HasMaxLength(37);

            builder.Property(_ => _.OwnerId).HasMaxLength(100);
            builder.HasIndex(_ => _.OwnerId);

            builder.Property(_ => _.BankType).HasMaxLength(100);
            builder.Property(_ => _.Balance);
        }
    }
}
