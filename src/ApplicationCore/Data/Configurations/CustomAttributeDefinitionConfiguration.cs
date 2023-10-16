using ApplicationCore.Constants;
using ApplicationCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApplicationCore.Data.Configurations
{
    public class CustomAttributeDefinitionConfiguration : IEntityTypeConfiguration<CustomAttributeDefinition>
    {
        public void Configure(EntityTypeBuilder<CustomAttributeDefinition> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.Property(_ => _.Id).HasMaxLength(37);

            builder.Property(_ => _.EntityName)
                .IsRequired()
                .HasMaxLength(CustomAttributeDefinitionConst.EntityNameMaxLength);
            builder.Property(_ => _.AttributeLabel)
                .IsRequired()
                .HasMaxLength(CustomAttributeDefinitionConst.AttributeLabelMaxLength);
            builder.Property(_ => _.AttributeCode)
                .IsRequired()
                .HasMaxLength(CustomAttributeDefinitionConst.AttributeCodeMaxLength);
            builder.Property(_ => _.DropdownValues)
                .IsRequired(false);

            builder.HasIndex(_ => new { _.EntityName, _.AttributeCode })
                .IsUnique(true);
        }
    }
}
