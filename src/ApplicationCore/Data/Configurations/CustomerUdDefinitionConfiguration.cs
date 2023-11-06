using ApplicationCore.Constants;
using ApplicationCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApplicationCore.Data.Configurations
{
    public class CustomerUdDefinitionConfiguration : IEntityTypeConfiguration<CustomerUdDefinition>
    {
        public void Configure(EntityTypeBuilder<CustomerUdDefinition> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.Property(_ => _.Id).HasMaxLength(37);

            builder.Property(_ => _.Label)
                .IsRequired()
                .HasMaxLength(UserDefinedDefinitionConst.LabelMaxLength);
            builder.Property(_ => _.Code)
                .IsRequired()
                .HasMaxLength(UserDefinedDefinitionConst.CodeMaxLength);
            builder.Property(_ => _.DropdownValues)
                .IsRequired(false);
            builder.HasIndex(_ => _.Code).IsUnique(true);
        }
    }
}
