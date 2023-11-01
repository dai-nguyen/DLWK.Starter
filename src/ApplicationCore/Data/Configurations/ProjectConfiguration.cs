using ApplicationCore.Constants;
using ApplicationCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApplicationCore.Data.Configurations
{
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.Property(_ => _.Id).HasMaxLength(37);

            builder.Property(_ => _.Name).IsRequired().HasMaxLength(ProjectConst.NameMaxLength);
            builder.Property(_ => _.Description).IsRequired().HasMaxLength(ProjectConst.DescriptionMaxLength);
            builder.Property(_ => _.Status).IsRequired().HasMaxLength(ProjectConst.StatusMaxLength);
            builder.Property(_ => _.DateStart).IsRequired();
            builder.Property(_ => _.DateDue).IsRequired();

            // full index
            builder.HasGeneratedTsVectorColumn(_ =>
                _.SearchVector,
                "english",
                _ => new { _.Name, _.Description })
                .HasIndex(_ => _.SearchVector)
                .HasMethod("GIN");

            builder.HasOne(_ => _.UserDefined)
               .WithOne(_ => _.Project)
               .HasForeignKey<ProjectUd>(_ => _.ProjectId)
               .IsRequired();

            builder.HasOne(_ => _.Customer)
                .WithMany(_ => _.Projects)
                .HasForeignKey(_ => _.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(_ => _.Contact)
                .WithMany(_ => _.Projects)
                .HasForeignKey(_ => _.ContactId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }

    public class ProjectUdConfiguration : IEntityTypeConfiguration<ProjectUd>
    {
        public void Configure(EntityTypeBuilder<ProjectUd> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.Property(_ => _.Id).HasMaxLength(37);

            builder.HasOne(_ => _.Project)
                .WithOne(_ => _.UserDefined)
                .HasForeignKey<ProjectUd>(_ => _.ProjectId)
                .IsRequired();
        }
    }
}
