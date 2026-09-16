using Activadis.Domain.Interfaces.Repositories;
using Activadis.Application.Validations.SignUp;
using Activadis.Application.Interfaces;
using Activadis.Application.Extensions;
using Activadis.Shared.DTOs.SignUp;
using Activadis.Domain.Entities;

namespace Activadis.Application.Services
{
    public class SignUpService : ISignUpService
    {
        private readonly IRepository<SignUp> SignUpRepository;
        private readonly IActivityRepository ActivityRepository;

        public SignUpService(IActivityRepository activityRepository, IRepository<SignUp> signUpRepository)
        {
            ActivityRepository = activityRepository;
            SignUpRepository = signUpRepository;
        }

        public async Task SignUpAsync(SignUpRequest request, Guid userId)
        {
            Activity? activity = await ActivityRepository.GetByIdAsync(request.ActivityId);
            request.Validate(activity, userId);

            await SignUpRepository.AddAsync(
                request.ToSignUp(userId)
            );
        }
    }
}
