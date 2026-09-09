using Activadis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Activadis.Infrastructure.Persistence.Configurations
{
    public static class SignUpConfiguration
    {
        public static void Configure(this EntityTypeBuilder<SignUp> builder)
        {
            builder.Property(su => su.FullName)
                    .IsRequired(true);

            builder.Property(su => su.Email)
                .IsRequired(true);

            builder.HasOne(su => su.Activity)
               .WithMany(a => a.SignUps)
               .HasForeignKey(su => su.ActivityId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(su => su.User)
                .WithMany(u => u.SignUps)
                .HasForeignKey(su => su.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
