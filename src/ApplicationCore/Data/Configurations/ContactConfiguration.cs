using ApplicationCore.Constants.Constants;
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

            builder.Property(_ => _.FirstName).IsRequired().HasMaxLength(ContactConst.FirstNameMaxLength);
            builder.Property(_ => _.LastName).IsRequired().HasMaxLength(ContactConst.LastNameMaxLength);
            builder.Property(_ => _.Email).IsRequired().HasMaxLength(ContactConst.EmailMaxLength);
            builder.Property(_ => _.Phone).IsRequired(false).HasMaxLength(ContactConst.PhoneMaxLength);

            // full index
            builder.HasGeneratedTsVectorColumn(_ =>
                _.SearchVector,
                "english",
                _ => new { _.FirstName, _.LastName, _.Email, _.Phone, _.ExternalId})
                .HasIndex(_ => _.SearchVector)
                .HasMethod("GIN");

            builder.HasOne(_ => _.UserDefined)
               .WithOne(_ => _.Contact)
               .HasForeignKey<ContactUd>(_ => _.ContactId)
               .IsRequired();

            builder.HasOne(_ => _.Customer)                
                .WithMany(_ => _.Contacts)
                .HasForeignKey(_ => _.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }

    public class ContactUdConfiguration : IEntityTypeConfiguration<ContactUd>
    {
        public void Configure(EntityTypeBuilder<ContactUd> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.Property(_ => _.Id).HasMaxLength(37);

            builder.HasOne(_ => _.Contact)
                .WithOne(_ => _.UserDefined)
                .HasForeignKey<ContactUd>(_ => _.ContactId)
                .IsRequired();
        }
    }
}
