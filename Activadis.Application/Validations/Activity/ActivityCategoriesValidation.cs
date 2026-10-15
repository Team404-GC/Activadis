using Activadis.Domain.Entities;

namespace Activadis.Application.Validations.Activity
{
    public static class ActivityCategoriesValidation
    {
        public static void ValidateCategories(this IEnumerable<Guid> categoryIds, IEnumerable<Category> categories, IEnumerable<Guid> linkedCategoryIds)
        {
            foreach (Guid categoryId in categoryIds.Distinct())
            {
                Category category = categories.FirstOrDefault(x => x.Id == categoryId)
                    ?? throw new ArgumentException("Een of meer ELO-categorieën bestaan niet.");

                if (!category.IsActive && !linkedCategoryIds.Contains(categoryId))
                    throw new ArgumentException($"De ELO-categorie {category.Name} is niet actief.");
            }
        }

        public static void ValidateCategoryRemoval(this Domain.Entities.Activity activity, IEnumerable<Guid> categoryIds)
        {
            Match? match = activity.Matches.FirstOrDefault(x => !categoryIds.Contains(x.CategoryId));
            if (match is not null)
                throw new ArgumentException($"Er is nog een wedstrijd in de ELO-categorie {match.Category?.Name} gekoppeld. Ontkoppel deze wedstrijd eerst.");
        }

        public static void ValidateCategory(this Domain.Entities.Activity activity, Guid categoryId)
        {
            if (!activity.Categories.Any(x => x.Id == categoryId))
                throw new ArgumentException("De ELO-categorie van de wedstrijd hoort niet bij deze activiteit.");
        }
    }
}
