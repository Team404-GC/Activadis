namespace Activadis.Shared.DTOs.Elo.Category
{
    public class UpdateCategoryRankTitlesRequest
    {
        public Guid CategoryId { get; set; }
        public Dictionary<double, string> RankTitles { get; set; } = [];
    }
}
