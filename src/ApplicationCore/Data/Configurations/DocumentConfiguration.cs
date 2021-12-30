using ApplicationCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApplicationCore.Data.Configurations
{
    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.Property(_ => _.Id).HasMaxLength(37);

            builder.Property(_ => _.Title).HasMaxLength(100);
            builder.Property(_ => _.Description).HasMaxLength(255);
            builder.Property(_ => _.IsPublic);
            builder.Property(_ => _.URL).HasMaxLength(255);

            // full index
            builder.HasGeneratedTsVectorColumn(_ => 
                _.SearchVector,
                "english",
                _ => new { _.Title, _.Description })
                .HasIndex(_ => _.SearchVector)
                .HasMethod("GIN");
            
            builder.HasOne(_ => _.DocumentType)
                .WithMany()
                .HasForeignKey(_ => _.DocumentTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
