using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore.Requests.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Services
{
    public class AccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IFileService _fileService;
        private readonly IStringLocalizer<AccountService> _localizer;

        public AccountService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IFileService fileService,
            IStringLocalizer<AccountService> localizer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _fileService = fileService;
            _localizer = localizer;
        }

        public async Task<IResult> ChangePasswordAsync(
            ChangePasswordRequest model, 
            string userId)
        {
            var user = await this._userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Fail(_localizer["User Not Found."]);
            }

            var identityResult = await this._userManager.ChangePasswordAsync(
                user,
                model.Password,
                model.NewPassword);

            var errors = identityResult.Errors
                .Select(e => _localizer[e.Description].ToString())
                .ToArray();

            return identityResult.Succeeded ? Result.Success() : Result.Fail(errors);
        }

        public async Task<IResult> UpdateProfileAsync(
            UpdateProfileRequest request, 
            string userId)
        {
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                var userWithSamePhoneNumber = await _userManager.Users
                    .FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber);

                if (userWithSamePhoneNumber != null)
                {
                    return Result.Fail(string.Format(_localizer["Phone number {0} is already used."], request.PhoneNumber));
                }
            }

            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);

            if (userWithSameEmail == null || userWithSameEmail.Id == userId)
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return Result.Fail(_localizer["User Not Found."]);
                }

                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.PhoneNumber = request.PhoneNumber;
                var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
                
                if (request.PhoneNumber != phoneNumber)
                {
                    var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, request.PhoneNumber);
                }
                
                var identityResult = await _userManager.UpdateAsync(user);
                var errors = identityResult.Errors
                    .Select(e => _localizer[e.Description].ToString())
                    .ToArray();

                await _signInManager.RefreshSignInAsync(user);

                return identityResult.Succeeded ? Result.Success() : Result.Fail(errors);
            }
            else
            {
                return Result.Fail(string.Format(_localizer["Email {0} is already used."], request.Email));
            }
        }

        public async Task<IResult<string>> GetProfilePictureAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                return Result<string>.Fail(_localizer["User Not Found"]);
            }

            //return Result<string>.Success(data: user.ProfilePictureUrl);
            return Result<string>.Success(data: "");
        }

        public async Task<IResult<string>> UpdateProfilePictureAsync(
            UpdateProfilePictureRequest request, 
            string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null) return Result<string>.Fail(message: _localizer["User Not Found"]);
            
            var filePath = await _fileService.UploadAsync(request);
            
            //user.ProfilePictureUrl = filePath;

            var identityResult = await _userManager.UpdateAsync(user);
            
            var errors = identityResult.Errors.Select(e => _localizer[e.Description].ToString()).ToArray();

            return identityResult.Succeeded ? Result<string>.Success(data: filePath) : Result<string>.Fail(errors);
        }
    }
}
