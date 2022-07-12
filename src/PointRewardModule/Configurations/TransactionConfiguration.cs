using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PointRewardModule.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Entities.Transaction>
    {
        public void Configure(EntityTypeBuilder<Entities.Transaction> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.Property(_ => _.Id).HasMaxLength(37);

            builder.Property(_ => _.Amount);
            builder.Property(_ => _.Notes).HasMaxLength(255);

            builder.Property(_ => _.BankId).HasMaxLength(100);
            
            builder.HasOne(_ => _.Bank)
                .WithMany(b => b.Transactions)
                .HasForeignKey(_ => _.BankId);

            // full index
            builder.HasGeneratedTsVectorColumn(_ =>
                _.SearchVector,
                "english",
                _ => _.Notes)
                .HasIndex(_ => _.SearchVector)
                .HasMethod("GIN");
        }
    }
}
