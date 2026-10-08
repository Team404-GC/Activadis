using Microsoft.EntityFrameworkCore;
using Activadis.Domain.Entities;

namespace Activadis.Infrastructure.Persistence.Seeders
{
    public static class ActivitySeeder
    {
        public static void UseActivitySeeder(this DbContext context)
        {
            DbSet<Activity> set = context.Set<Activity>();

            if (!set.Any())
            {
                // Dates are counted from today, so the first three are always in the past and the last three always upcoming.
                DateTime today = DateTime.UtcNow.Date;

                byte[] bowling = ReadImage("bowling.webp");
                byte[] gaming = ReadImage("gaming.jpg");
                byte[] pizza = ReadImage("pizza.jpg");

                IEnumerable<Activity> activities = [
                    new Activity()
                    {
                        Name = "Bowlingavond",
                        Description = "Een avond bowlen met het hele team. De schoenen en de banen zijn geregeld, na afloop is er een hapje en een drankje.",
                        CostPerPerson = 12.50m,
                        Location = "Bowlingcentrum Doetinchem",
                        Image = bowling,
                        ImageContentType = "image/webp",
                        MinParticipants = 6,
                        MaxParticipants = 24,
                        FoodIncluded = true,
                        ExternalAllowed = false,
                        PlusOneAllowed = true,
                        StartDate = today.AddDays(-28).AddHours(17),
                        EndDate = today.AddDays(-28).AddHours(20),
                        SignUpDeadline = today.AddDays(-31),
                        SignOutDeadline = today.AddDays(-30),
                        PublishedOn = today.AddDays(-42),
                        CreatedAt = DateTime.UtcNow
                    },
                    new Activity()
                    {
                        Name = "LAN-party",
                        Description = "Neem je eigen laptop of pc mee en speel de hele avond samen games. Voor frisdrank en snacks wordt gezorgd.",
                        CostPerPerson = 5.00m,
                        Location = "Kantoor Covadis, grote vergaderzaal",
                        Image = gaming,
                        ImageContentType = "image/jpeg",
                        MinParticipants = 4,
                        MaxParticipants = 16,
                        FoodIncluded = true,
                        ExternalAllowed = false,
                        PlusOneAllowed = false,
                        StartDate = today.AddDays(-14).AddHours(16),
                        EndDate = today.AddDays(-14).AddHours(22),
                        SignUpDeadline = today.AddDays(-17),
                        SignOutDeadline = today.AddDays(-16),
                        PublishedOn = today.AddDays(-28),
                        CreatedAt = DateTime.UtcNow
                    },
                    new Activity()
                    {
                        Name = "Pizzalunch",
                        Description = "Gezamenlijke lunch op kantoor met pizza voor iedereen. Geef bij je inschrijving door als je vegetarisch eet.",
                        CostPerPerson = 0.00m,
                        Location = "Kantoor Covadis, kantine",
                        Image = pizza,
                        ImageContentType = "image/jpeg",
                        MinParticipants = 0,
                        MaxParticipants = 40,
                        FoodIncluded = true,
                        ExternalAllowed = false,
                        PlusOneAllowed = false,
                        StartDate = today.AddDays(-5).AddHours(10),
                        EndDate = today.AddDays(-5).AddHours(11),
                        SignUpDeadline = today.AddDays(-7),
                        SignOutDeadline = today.AddDays(-6),
                        PublishedOn = today.AddDays(-19),
                        CreatedAt = DateTime.UtcNow
                    },
                    new Activity()
                    {
                        Name = "Bowlingtoernooi",
                        Description = "Het jaarlijkse bowlingtoernooi in teams van vier. Het winnende team krijgt de wisselbeker mee naar kantoor.",
                        CostPerPerson = 15.00m,
                        Location = "Bowlingcentrum Doetinchem",
                        Image = bowling,
                        ImageContentType = "image/webp",
                        MinParticipants = 8,
                        MaxParticipants = 32,
                        FoodIncluded = true,
                        ExternalAllowed = true,
                        PlusOneAllowed = true,
                        StartDate = today.AddDays(9).AddHours(17),
                        EndDate = today.AddDays(9).AddHours(21),
                        SignUpDeadline = today.AddDays(6),
                        SignOutDeadline = today.AddDays(7),
                        PublishedOn = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Activity()
                    {
                        Name = "Game-avond",
                        Description = "Een avond gamen op kantoor, van shooters tot racespellen. Er staan een paar pc's klaar, maar je eigen laptop meenemen mag ook.",
                        CostPerPerson = 5.00m,
                        Location = "Kantoor Covadis, grote vergaderzaal",
                        Image = gaming,
                        ImageContentType = "image/jpeg",
                        MinParticipants = 4,
                        MaxParticipants = 16,
                        FoodIncluded = false,
                        ExternalAllowed = false,
                        PlusOneAllowed = false,
                        StartDate = today.AddDays(16).AddHours(16),
                        EndDate = today.AddDays(16).AddHours(22),
                        SignUpDeadline = today.AddDays(13),
                        SignOutDeadline = today.AddDays(14),
                        PublishedOn = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Activity()
                    {
                        Name = "Pizza-avond",
                        Description = "Samen pizza eten en bijpraten na het werk. Partners zijn ook welkom.",
                        CostPerPerson = 7.50m,
                        Location = "Kantoor Covadis, kantine",
                        Image = pizza,
                        ImageContentType = "image/jpeg",
                        MinParticipants = 0,
                        MaxParticipants = 40,
                        FoodIncluded = true,
                        ExternalAllowed = true,
                        PlusOneAllowed = true,
                        StartDate = today.AddDays(30).AddHours(16),
                        EndDate = today.AddDays(30).AddHours(19),
                        SignUpDeadline = today.AddDays(27),
                        SignOutDeadline = today.AddDays(28),
                        PublishedOn = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    }
                ];

                set.AddRange(activities);
                context.SaveChanges();
            }
        }

        private static byte[] ReadImage(string fileName)
        {
            string resourceName = $"Activadis.Infrastructure.Persistence.Seeders.Images.{fileName}";

            using Stream stream = typeof(ActivitySeeder).Assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"The seed image '{fileName}' was not found.");

            using MemoryStream memory = new MemoryStream();
            stream.CopyTo(memory);

            return memory.ToArray();
        }
    }
}
