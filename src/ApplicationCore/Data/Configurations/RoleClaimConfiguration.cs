using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApplicationCore.Data.Configurations
{
    public class RoleClaimConfiguration : IEntityTypeConfiguration<AppRoleClaim>
    {
        public void Configure(EntityTypeBuilder<AppRoleClaim> builder)
        {
            builder.Property(_ => _.Description).HasMaxLength(100);

            builder.HasOne(r => r.Role)
                    .WithMany(rc => rc.RoleClaims)
                    .HasForeignKey(fk => fk.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
