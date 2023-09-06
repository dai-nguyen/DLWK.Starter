using ApplicationCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApplicationCore.Data.Configurations
{
    public class ContactConfiguration : IEntityTypeConfiguration<Contact>
    {
        public void Configure(EntityTypeBuilder<Contact> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.Property(_ => _.Id).HasMaxLength(37);

            builder.Property(_ => _.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(_ => _.LastName).IsRequired().HasMaxLength(100);
            builder.Property(_ => _.Email).IsRequired().HasMaxLength(100);
            builder.Property(_ => _.Phone).HasMaxLength(100);

            // full index
            builder.HasGeneratedTsVectorColumn(_ =>
                _.SearchVector,
                "english",
                _ => new { _.FirstName, _.LastName, _.Email, _.Phone})
                .HasIndex(_ => _.SearchVector)
                .HasMethod("GIN");

            builder.HasOne(_ => _.Customer)
                .WithMany(_ => _.Contacts)
                .HasForeignKey(_ => _.CustomerId);
        }
    }
}
