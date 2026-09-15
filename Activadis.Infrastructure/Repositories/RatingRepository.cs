using Activadis.Domain.Entities;
using Activadis.Domain.Interfaces.Repositories;
using Activadis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Activadis.Infrastructure.Repositories
{
    public class RatingRepository : Repository<Rating>, IRatingRepository
    {
        private readonly ApplicationDBContext _context;
        private const double InitialRating = 1200;

        public RatingRepository(ApplicationDBContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Rating>> GetUserRatingsAsync(Guid userId)
            => await _context.Ratings
                .Include(r => r.Category)
                .Where(r => r.UserId == userId && r.DeletedAt == null)
                .ToListAsync();

        public async Task<IEnumerable<Rating>> GetCategoryRatingsAsync(Guid categoryId)
            => await _context.Ratings
                .Include(r => r.User)
                .Where(r => r.CategoryId == categoryId && r.DeletedAt == null)
                .OrderByDescending(r => r.CurrentRating)
                .ToListAsync();

        public async Task<Rating?> GetUserCategoryRatingAsync(Guid userId, Guid categoryId)
            => await _context.Ratings
                .Include(r => r.User)
                .Include(r => r.Category)
                .FirstOrDefaultAsync(r =>
                    r.UserId == userId &&
                    r.CategoryId == categoryId &&
                    r.DeletedAt == null);

        public async Task<Rating> GetOrCreateRatingAsync(Guid userId, Guid categoryId)
        {
            var existingRating = await GetUserCategoryRatingAsync(userId, categoryId);
            if (existingRating != null)
                return existingRating;

            var newRating = new Rating
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CategoryId = categoryId,
                CurrentRating = InitialRating,
                MatchCount = 0,
                PeakRating = InitialRating,
                CreatedAt = DateTime.UtcNow
            };

            return await AddAsync(newRating);
        }
    }
}
