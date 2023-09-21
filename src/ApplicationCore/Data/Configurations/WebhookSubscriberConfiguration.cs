using ApplicationCore.Constants;
using ApplicationCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApplicationCore.Data.Configurations
{
    public class WebhookSubscriberConfiguration : IEntityTypeConfiguration<WebhookSubscriber>
    {
        public void Configure(EntityTypeBuilder<WebhookSubscriber> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.Property(_ => _.Id).HasMaxLength(37);
               
            builder.Property(_ => _.EntityName).IsRequired().HasMaxLength(WebhookSubscriberConst.EntityNameMaxLength);
            builder.Property(_ => _.Operation).IsRequired().HasMaxLength(WebhookSubscriberConst.OperationMaxLength);
            builder.Property(_ => _.Url).IsRequired().HasMaxLength(WebhookSubscriberConst.UrlMaxLength);
            builder.Property(_ => _.IsEnabled);
            builder.Property(_ => _.FailedCount);

            builder.HasIndex(_ => _.EntityName).IncludeProperties(_ => _.Operation).IncludeProperties(_ => _.IsEnabled);
            builder.HasIndex(_ => new { _.EntityName, _.Operation, _.Url }).IsUnique();
        }
    }
}
