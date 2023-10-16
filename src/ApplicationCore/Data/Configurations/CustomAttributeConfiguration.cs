using ApplicationCore.Constants;
using ApplicationCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApplicationCore.Data.Configurations
{
    public class CustomAttributeConfiguration : IEntityTypeConfiguration<CustomAttribute>
    {
        public void Configure(EntityTypeBuilder<CustomAttribute> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.Property(_ => _.Id).HasMaxLength(37);

            builder.Property(_ => _.EntityId)
                .IsRequired(true)
                .HasMaxLength(CustomAttributeConst.EntityId);
            builder.HasIndex(_ => _.EntityId)
                .IsUnique(false);
            builder.Property(_ => _.CustomAttributeValue)
                .HasMaxLength(CustomAttributeConst.CustomAttributeValue);

            builder.HasOne(_ => _.CustomAttributeDefinition)
                .WithMany(_ => _.CustomAttributes)
                .HasForeignKey(_ => _.CustomAttributeDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
