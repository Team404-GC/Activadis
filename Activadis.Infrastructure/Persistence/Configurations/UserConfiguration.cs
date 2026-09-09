using Activadis.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Activadis.Infrastructure.Persistence.Configurations
{
    public static class UserConfiguration
    {
        public static void Configure(this EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.FullName)
                   .IsRequired(true);

            builder.Property(u => u.Email)
                .IsRequired(true);

            builder.Property(u => u.HashedPassword)
                .IsRequired(true);
        }
    }
}
