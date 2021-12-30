using ApplicationCore.Requests.Identity;

namespace ApplicationCore.Interfaces.Identity
{
    public interface IAccountService : IService
    {
        Task<IResult> UpdateProfileAsync(
            UpdateProfileRequest model, 
            string userId);

        Task<IResult> ChangePasswordAsync(
            ChangePasswordRequest model, 
            string userId);

        Task<IResult<string>> GetProfilePictureAsync(string userId);

        Task<IResult<string>> UpdateProfilePictureAsync(
            UpdateProfilePictureRequest request, 
            string userId);
    }
}
