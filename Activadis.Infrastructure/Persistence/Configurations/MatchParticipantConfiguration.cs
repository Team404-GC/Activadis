using Activadis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Activadis.Infrastructure.Persistence.Configurations
{
    public static class MatchParticipantConfiguration
    {
        public static void Configure(this EntityTypeBuilder<MatchParticipant> builder)
        {
            builder.HasOne(mp => mp.Match)
                .WithMany(m => m.Participants)
                .HasForeignKey(mp => mp.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(mp => mp.User)
                .WithMany()
                .HasForeignKey(mp => mp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(mp => new { mp.MatchId, mp.UserId })
                .IsUnique();

            builder.HasQueryFilter(mp => mp.DeletedAt == null);
        }
    }
}
