using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApplicationCore.Data.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<AppRole>
    {
        public void Configure(EntityTypeBuilder<AppRole> builder)
        {
            builder.Property(_ => _.Description).HasMaxLength(100);

            builder.HasGeneratedTsVectorColumn(_ =>
                _.SearchVector,
                "english",
                _ => new { _.Name, _.Description })
                .HasIndex(_ => _.SearchVector)
                .HasMethod("GIN");
        }
    }
}
