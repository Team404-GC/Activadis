namespace Activadis.Shared.DTOs.User
{
    public static class JobTitles
    {
        public static readonly IReadOnlyList<string> All =
        [
            "Backend developer",
            "Directeur",
            "HR-medewerker",
            "Office manager",
            "Projectmanager",
            "Softwareontwikkelaar",
            "Stagiair softwareontwikkeling",
            "Testengineer",
            "UX-designer"
        ];

        public static bool IsValid(string? jobTitle)
            => jobTitle is not null && All.Contains(jobTitle);
    }
}
