using ApplicationCore.Constants.Constants;
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

            builder.Property(_ => _.Name).IsRequired().HasMaxLength(CustomerConst.NameMaxLength);
            builder.HasIndex(_ => _.Name).IsUnique();
            builder.Property(_ => _.Description).HasMaxLength(CustomerConst.DescriptionMaxLength);
            builder.Property(_ => _.Industries).IsRequired();
            builder.Property(_ => _.Address1).HasMaxLength(CustomerConst.AddressMaxLength);
            builder.Property(_ => _.Address2).IsRequired(false).HasMaxLength(CustomerConst.AddressMaxLength);
            builder.Property(_ => _.City).HasMaxLength(CustomerConst.CityMaxLength);
            builder.Property(_ => _.State).HasMaxLength(CustomerConst.StateMaxLength);
            builder.Property(_ => _.Zip).HasMaxLength(CustomerConst.ZipMaxLength);
            builder.Property(_ => _.Country).HasMaxLength(CustomerConst.CountryMaxLength);

            // full index
            builder.HasGeneratedTsVectorColumn(_ =>
                _.SearchVector,
                "english",
                _ => new { _.Name, _.Description })
                .HasIndex(_ => _.SearchVector)
                .HasMethod("GIN");

            builder.HasOne(_ => _.UserDefined)
                .WithOne(_ => _.Customer)
                .HasForeignKey<CustomerUd>(_ => _.CustomerId)
                .IsRequired();
        }

        public class CustomerUdConfiguration : IEntityTypeConfiguration<CustomerUd>
        {
            public void Configure(EntityTypeBuilder<CustomerUd> builder)
            {
                builder.HasKey(_ => _.Id);
                builder.Property(_ => _.Id).HasMaxLength(37);

                builder.HasOne(_ => _.Customer)
                    .WithOne(_ => _.UserDefined)
                    .HasForeignKey<CustomerUd>(_ => _.CustomerId)
                    .IsRequired();
            }
        }
    }
}
