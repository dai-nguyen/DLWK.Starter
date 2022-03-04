using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApplicationCore.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.Property(_ => _.FirstName).HasMaxLength(100);
            builder.Property(_ => _.LastName).HasMaxLength(100);
            builder.Property(_ => _.Title).HasMaxLength(100);
            //builder.Property(_ => _.ProfilePictureUrl).HasMaxLength(255);
            builder.Property(_ => _.ProfilePicture);

            builder.HasGeneratedTsVectorColumn(_ =>
                _.SearchVector,
                "english",
                _ => new { _.UserName, _.FirstName, _.LastName, _.Email, _.Title })
                .HasIndex(_ => _.SearchVector)
                .HasMethod("GIN");

            builder.Property(_ => _.CustomAttributes)
                .HasColumnType("jsonb");            
        }
    }
}
