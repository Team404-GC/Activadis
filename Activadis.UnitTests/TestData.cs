using Activadis.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Activadis.UnitTests
{
    // Shared builders for the tests, so every test only sets what it is about.
    internal static class TestData
    {
        // The first bytes of a JPEG file, enough for the image check.
        public static readonly byte[] JpegBytes = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];

        public static Activity CreateActivity(Action<Activity>? configure = null)
        {
            DateTime now = DateTime.UtcNow;

            Activity activity = new Activity()
            {
                Id = Guid.NewGuid(),
                Name = "Bowlingavond",
                Description = "Een avond bowlen.",
                Location = "Doetinchem",
                MaxParticipants = 10,
                ExternalAllowed = true,
                PlusOneAllowed = true,
                StartDate = now.AddDays(10),
                EndDate = now.AddDays(10).AddHours(3),
                SignUpDeadline = now.AddDays(7),
                SignOutDeadline = now.AddDays(8),
                PublishedOn = now.AddDays(-1),
                CreatedAt = now.AddDays(-2)
            };

            configure?.Invoke(activity);
            return activity;
        }

        public static User CreateUser(Action<User>? configure = null)
        {
            User user = new User()
            {
                Id = Guid.NewGuid(),
                FullName = "Daan Bakker",
                Email = "daan@covadis.nl",
                JobTitle = "Directeur",
                HashedPassword = "hash",
                IsActive = true
            };

            configure?.Invoke(user);
            return user;
        }

        public static SignUp CreateSignUp(Guid activityId, Action<SignUp>? configure = null)
        {
            SignUp signUp = new SignUp()
            {
                Id = Guid.NewGuid(),
                ActivityId = activityId,
                FullName = "Emma Smit",
                Email = "emma@extern.nl",
                IsExternal = true
            };

            configure?.Invoke(signUp);
            return signUp;
        }

        public static IFormFile CreateImage(string fileName = "foto.jpg", string contentType = "image/jpeg", byte[]? content = null)
        {
            byte[] bytes = content ?? JpegBytes;
            return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "Photos", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        // The confirmation services resolve their repositories from a scope, so the mocks are put in a real service provider.
        public static IServiceScopeFactory CreateScopeFactory(Action<IServiceCollection> configure)
        {
            ServiceCollection services = new ServiceCollection();
            configure(services);

            return services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
        }
    }
}
