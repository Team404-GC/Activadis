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
                // Dates are counted from today, so past activities stay in the past and upcoming ones stay upcoming.
                DateTime today = DateTime.UtcNow.Date;

                byte[] bowling = ReadImage("bowling.webp");
                byte[] gaming = ReadImage("gaming.jpg");
                byte[] pizza = ReadImage("pizza.jpg");
                byte[] darts = ReadImage("darts.jpg");
                byte[] tafeltennis = ReadImage("tafeltennis.jpg");

                IEnumerable<Activity> activities = [
                    new Activity()
                    {
                        Name = "Bowlingavond",
                        Description = "Een avond bowlen met het hele team. De schoenen en de banen zijn geregeld, na afloop is er een hapje en een drankje.",
                        CostPerPerson = 12.50m,
                        Location = "Bowlingcentrum Doetinchem",
                        Photos = [new ActivityPhoto() { Image = bowling, ImageContentType = "image/webp", CreatedAt = DateTime.UtcNow }],
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
                        Photos = [new ActivityPhoto() { Image = gaming, ImageContentType = "image/jpeg", CreatedAt = DateTime.UtcNow }],
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
                        Photos = [new ActivityPhoto() { Image = pizza, ImageContentType = "image/jpeg", CreatedAt = DateTime.UtcNow }],
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
                        Photos = [new ActivityPhoto() { Image = bowling, ImageContentType = "image/webp", CreatedAt = DateTime.UtcNow }],
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
                        Photos = [new ActivityPhoto() { Image = gaming, ImageContentType = "image/jpeg", CreatedAt = DateTime.UtcNow }],
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
                        Photos = [new ActivityPhoto() { Image = pizza, ImageContentType = "image/jpeg", CreatedAt = DateTime.UtcNow }],
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
                    },

                    // Tournaments with ELO categories. The match seeder links the seeded matches to these activities.
                    new Activity()
                    {
                        Name = "Kantoorkampioenschap",
                        Description = "Het jaarlijkse kampioenschap op kantoor. Een hele middag en avond strijden in alle ELO-categorieën, van tafeltennis tot Mario Kart.",
                        CostPerPerson = 0.00m,
                        Location = "Kantoor Covadis",
                        Photos = [
                            new ActivityPhoto() { Image = gaming, ImageContentType = "image/jpeg", SortOrder = 0, CreatedAt = DateTime.UtcNow },
                            new ActivityPhoto() { Image = darts, ImageContentType = "image/jpeg", SortOrder = 1, CreatedAt = DateTime.UtcNow },
                            new ActivityPhoto() { Image = tafeltennis, ImageContentType = "image/jpeg", SortOrder = 2, CreatedAt = DateTime.UtcNow }
                        ],
                        MinParticipants = 4,
                        MaxParticipants = 40,
                        FoodIncluded = true,
                        ExternalAllowed = false,
                        PlusOneAllowed = false,
                        StartDate = today.AddDays(-330).AddHours(12),
                        EndDate = today.AddDays(-330).AddHours(21),
                        SignUpDeadline = today.AddDays(-333),
                        SignOutDeadline = today.AddDays(-332),
                        PublishedOn = today.AddDays(-350),
                        CreatedAt = DateTime.UtcNow
                    },
                    new Activity()
                    {
                        Name = "Zomertoernooi",
                        Description = "Toernooi in de zomer met tafeltennis, tafelvoetbal, pool en darts. Na afloop een barbecue op het terras.",
                        CostPerPerson = 10.00m,
                        Location = "Kantoor Covadis, kantine en terras",
                        Photos = [
                            new ActivityPhoto() { Image = tafeltennis, ImageContentType = "image/jpeg", SortOrder = 0, CreatedAt = DateTime.UtcNow },
                            new ActivityPhoto() { Image = darts, ImageContentType = "image/jpeg", SortOrder = 1, CreatedAt = DateTime.UtcNow }
                        ],
                        MinParticipants = 4,
                        MaxParticipants = 32,
                        FoodIncluded = true,
                        ExternalAllowed = false,
                        PlusOneAllowed = true,
                        StartDate = today.AddDays(-239).AddHours(13),
                        EndDate = today.AddDays(-239).AddHours(20),
                        SignUpDeadline = today.AddDays(-242),
                        SignOutDeadline = today.AddDays(-241),
                        PublishedOn = today.AddDays(-260),
                        CreatedAt = DateTime.UtcNow
                    },
                    new Activity()
                    {
                        Name = "Darttoernooi",
                        Description = "Darttoernooi tijdens de vrijdagmiddagborrel. Iedereen speelt in poules, de beste spelers gaan door naar de finale.",
                        CostPerPerson = 0.00m,
                        Location = "Kantoor Covadis, kantine",
                        Photos = [new ActivityPhoto() { Image = darts, ImageContentType = "image/jpeg", SortOrder = 0, CreatedAt = DateTime.UtcNow }],
                        MinParticipants = 4,
                        MaxParticipants = 24,
                        FoodIncluded = false,
                        ExternalAllowed = false,
                        PlusOneAllowed = false,
                        StartDate = today.AddDays(-40).AddHours(15),
                        EndDate = today.AddDays(-40).AddHours(19),
                        SignUpDeadline = today.AddDays(-42),
                        SignOutDeadline = today.AddDays(-41),
                        PublishedOn = today.AddDays(-55),
                        CreatedAt = DateTime.UtcNow
                    },
                    new Activity()
                    {
                        Name = "Spelletjesmiddag",
                        Description = "Een middag spelletjes op kantoor: tafeltennis, tafelvoetbal, pool, darts, schaken en Mario Kart. Alle uitslagen tellen mee voor de ranglijst.",
                        CostPerPerson = 0.00m,
                        Location = "Kantoor Covadis",
                        Photos = [
                            new ActivityPhoto() { Image = gaming, ImageContentType = "image/jpeg", SortOrder = 0, CreatedAt = DateTime.UtcNow },
                            new ActivityPhoto() { Image = tafeltennis, ImageContentType = "image/jpeg", SortOrder = 1, CreatedAt = DateTime.UtcNow },
                            new ActivityPhoto() { Image = darts, ImageContentType = "image/jpeg", SortOrder = 2, CreatedAt = DateTime.UtcNow }
                        ],
                        MinParticipants = 2,
                        MaxParticipants = 30,
                        FoodIncluded = false,
                        ExternalAllowed = false,
                        PlusOneAllowed = false,
                        StartDate = today.AddDays(-1).AddHours(12),
                        EndDate = today.AddDays(-1).AddHours(17),
                        SignUpDeadline = today.AddDays(-3),
                        SignOutDeadline = today.AddDays(-2),
                        PublishedOn = today.AddDays(-14),
                        CreatedAt = DateTime.UtcNow
                    },
                    new Activity()
                    {
                        Name = "Tafeltennis- en dartsmiddag",
                        Description = "Een middag tafeltennis en darts op kantoor. De uitslagen tellen mee voor de ranglijst.",
                        CostPerPerson = 0.00m,
                        Location = "Kantoor Covadis, kantine",
                        Photos = [
                            new ActivityPhoto() { Image = tafeltennis, ImageContentType = "image/jpeg", SortOrder = 0, CreatedAt = DateTime.UtcNow },
                            new ActivityPhoto() { Image = darts, ImageContentType = "image/jpeg", SortOrder = 1, CreatedAt = DateTime.UtcNow }
                        ],
                        MinParticipants = 2,
                        MaxParticipants = 20,
                        FoodIncluded = false,
                        ExternalAllowed = false,
                        PlusOneAllowed = false,
                        StartDate = today.AddHours(12),
                        EndDate = today.AddHours(17),
                        SignUpDeadline = today.AddDays(-2),
                        SignOutDeadline = today.AddDays(-1),
                        PublishedOn = today.AddDays(-7),
                        CreatedAt = DateTime.UtcNow
                    },
                    new Activity()
                    {
                        Name = "Darts op de vrijdagmiddagborrel",
                        Description = "Darts tijdens de borrel. De uitslagen worden als wedstrijd vastgelegd en tellen mee voor de ranglijst darts.",
                        CostPerPerson = 0.00m,
                        Location = "Kantoor Covadis, kantine",
                        Photos = [new ActivityPhoto() { Image = darts, ImageContentType = "image/jpeg", SortOrder = 0, CreatedAt = DateTime.UtcNow }],
                        MinParticipants = 2,
                        MaxParticipants = 20,
                        FoodIncluded = true,
                        ExternalAllowed = false,
                        PlusOneAllowed = false,
                        StartDate = today.AddDays(21).AddHours(15),
                        EndDate = today.AddDays(21).AddHours(18),
                        SignUpDeadline = today.AddDays(19),
                        SignOutDeadline = today.AddDays(20),
                        PublishedOn = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Activity()
                    {
                        Name = "Tafeltennis- en tafelvoetbaltoernooi",
                        Description = "Toernooi tafeltennis en tafelvoetbal, enkel en dubbel. Schrijf je in en speel mee voor de ranglijst.",
                        CostPerPerson = 0.00m,
                        Location = "Kantoor Covadis, pauzeruimte",
                        Photos = [new ActivityPhoto() { Image = tafeltennis, ImageContentType = "image/jpeg", SortOrder = 0, CreatedAt = DateTime.UtcNow }],
                        MinParticipants = 4,
                        MaxParticipants = 24,
                        FoodIncluded = false,
                        ExternalAllowed = false,
                        PlusOneAllowed = false,
                        StartDate = today.AddDays(45).AddHours(14),
                        EndDate = today.AddDays(45).AddHours(18),
                        SignUpDeadline = today.AddDays(42),
                        SignOutDeadline = today.AddDays(43),
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
