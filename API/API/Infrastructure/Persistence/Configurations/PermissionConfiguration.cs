using Authorization.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.HasKey(p => p.Id);

            builder.HasIndex(p => p.Code).IsUnique();

            builder.Property(p => p.Code).IsRequired();
            builder.Property(p => p.Name).IsRequired();

            builder.HasOne(p => p.Parent)
                   .WithMany(p => p.Children)
                   .HasForeignKey(p => p.ParentId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
