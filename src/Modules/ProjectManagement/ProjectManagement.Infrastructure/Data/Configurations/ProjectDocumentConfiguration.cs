using ProjectManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static ProjectManagement.Domain.Common.BaseEntity;

namespace ProjectManagement.Infrastructure.Data.Configurations
{
    public class ProjectDocumentConfiguration: IEntityTypeConfiguration<ProjectDocument>
    {
        public void Configure(EntityTypeBuilder<ProjectDocument> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("ProjectDocument", "Project");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(d => d.ProjectId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(d => d.DocumentId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(d => d.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(d => d.UploadedDate)
                .IsRequired();



            // FK: ProjectDocument.ProjectId → ProjectMaster.Id
            builder.HasOne(d => d.Project)
                   .WithMany(p => p.ProjectDocuments)
                   .HasForeignKey(d => d.ProjectId)
                   .OnDelete(DeleteBehavior.Cascade);
                   
                   
        }
        
    }
}