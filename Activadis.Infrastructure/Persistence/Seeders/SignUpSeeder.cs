using Microsoft.EntityFrameworkCore;
using Activadis.Domain.Entities;

namespace Activadis.Infrastructure.Persistence.Seeders
{
    public static class SignUpSeeder
    {
        public static void UseSignUpSeeder(this DbContext context)
        {
            DbSet<SignUp> set = context.Set<SignUp>();

            if (!set.Any())
            {
                Dictionary<string, User> users = context.Set<User>()
                    .ToDictionary(x => x.FullName);

                List<Activity> activities = context.Set<Activity>()
                    .Include(x => x.Categories)
                    .ToList();

                List<Match> matches = context.Set<Match>()
                    .Include(x => x.Participants)
                    .ToList();

                // Employees that are signed up next to the players of the matches.
                Dictionary<string, string[]> employees = new()
                {
                    ["Bowlingavond"] = ["Peter Hoekstra", "Sanne de Vries", "Daan Bakker", "Lotte Jansen", "Thijs Visser", "Emma Smit", "Milan van Dijk", "Fleur Mulder"],
                    ["LAN-party"] = ["Daan Bakker", "Thijs Visser", "Milan van Dijk", "Ruben Bos"],
                    ["Pizzalunch"] = ["Peter Hoekstra", "Sanne de Vries", "Daan Bakker", "Lotte Jansen", "Thijs Visser", "Emma Smit", "Milan van Dijk", "Fleur Mulder", "Ruben Bos"],
                    ["Bowlingtoernooi"] = ["Sanne de Vries", "Daan Bakker", "Lotte Jansen", "Emma Smit", "Ruben Bos"],
                    ["Game-avond"] = ["Peter Hoekstra", "Thijs Visser", "Milan van Dijk", "Ruben Bos"],
                    ["Pizza-avond"] = ["Peter Hoekstra", "Lotte Jansen", "Emma Smit", "Milan van Dijk", "Fleur Mulder"],
                    ["Darts op de vrijdagmiddagborrel"] = ["Sanne de Vries", "Daan Bakker", "Thijs Visser", "Emma Smit", "Milan van Dijk"],
                    ["Tafeltennis- en tafelvoetbaltoernooi"] = ["Sanne de Vries", "Daan Bakker", "Lotte Jansen", "Thijs Visser", "Milan van Dijk", "Fleur Mulder"]
                };

                // External guests, only for activities that allow them.
                Dictionary<string, (string FullName, string Email)> externals = new()
                {
                    ["Bowlingtoernooi"] = ("Sophie Meijer", "sophie.meijer@example.com"),
                    ["Pizza-avond"] = ("Jeroen de Boer", "jeroen.deboer@example.com")
                };

                List<SignUp> signUps = [];
                foreach (Activity activity in activities)
                {
                    // Everyone who played a match in one of the activity's categories on the day of the activity was there.
                    IEnumerable<Guid> playerIds = matches
                        .Where(match => activity.Categories.Any(category => category.Id == match.CategoryId)
                            && activity.StartDate.Date <= match.MatchDate.Date
                            && activity.EndDate.Date >= match.MatchDate.Date)
                        .SelectMany(match => match.Participants)
                        .Select(participant => participant.UserId);

                    IEnumerable<Guid> employeeIds = employees.GetValueOrDefault(activity.Name, [])
                        .Where(users.ContainsKey)
                        .Select(name => users[name].Id);

                    foreach (User user in users.Values.Where(user => playerIds.Concat(employeeIds).Contains(user.Id)).Take(activity.MaxParticipants))
                        signUps.Add(CreateSignUp(activity, user.FullName, user.Email, user.Id));

                    if (activity.ExternalAllowed && externals.TryGetValue(activity.Name, out (string FullName, string Email) external))
                        signUps.Add(CreateSignUp(activity, external.FullName, external.Email, null));
                }

                set.AddRange(signUps);
                context.SaveChanges();
            }
        }

        private static SignUp CreateSignUp(Activity activity, string fullName, string email, Guid? userId)
        {
            return new SignUp()
            {
                ActivityId = activity.Id,
                UserId = userId,
                FullName = fullName,
                Email = email,
                IsExternal = userId is null,
                CreatedAt = activity.SignUpDeadline.AddDays(-1)
            };
        }
    }
}
