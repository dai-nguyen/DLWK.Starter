using ApplicationCore.Constants;
using ApplicationCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApplicationCore.Data.Configurations
{
    public class WebhookMessageConfiguration : IEntityTypeConfiguration<WebhookMessage>
    {
        public void Configure(EntityTypeBuilder<WebhookMessage> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.Property(_ => _.Id).HasMaxLength(37);

            builder.Property(_ => _.EntityId).IsRequired().HasMaxLength(WebhookMessageConst.EntityIdMaxLength);            
            builder.Property(_ => _.IsOkResponse);

            builder.HasOne(_ => _.Subscriber)
                .WithMany(_ => _.Messages)
                .HasForeignKey(_ => _.SubscriberId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
