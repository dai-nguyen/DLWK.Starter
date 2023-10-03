using ApplicationCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApplicationCore.Data.Configurations
{
    public class IndustryConfiguration : IEntityTypeConfiguration<Industry>
    {
        public void Configure(EntityTypeBuilder<Industry> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.Property(_ => _.Id).HasMaxLength(37);

            builder.Property(_ => _.Name).IsRequired().HasMaxLength(128);
            builder.HasIndex(_ => _.Name).IsUnique();
        }
    }
}
