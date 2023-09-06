using ApplicationCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApplicationCore.Data.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.Property(_ => _.Id).HasMaxLength(37);

            builder.Property(_ => _.Name).IsRequired().HasMaxLength(100);
            builder.HasIndex(_ => _.Name).IsUnique();
            builder.Property(_ => _.Description).HasMaxLength(255);
            builder.Property(_ => _.Industries);
            builder.Property(_ => _.Address1).HasMaxLength(100);
            builder.Property(_ => _.Address2).HasMaxLength(100);
            builder.Property(_ => _.City).HasMaxLength(100);
            builder.Property(_ => _.State).HasMaxLength(5);
            builder.Property(_ => _.Zip).HasMaxLength(10);
            builder.Property(_ => _.Country).HasMaxLength(10);

            // full index
            builder.HasGeneratedTsVectorColumn(_ =>
                _.SearchVector,
                "english",
                _ => new { _.Name, _.Description })
                .HasIndex(_ => _.SearchVector)
                .HasMethod("GIN");
        }
    }
}
