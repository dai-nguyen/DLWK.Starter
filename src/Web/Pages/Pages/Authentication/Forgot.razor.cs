using ApplicationCore.Configurations;
using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Requests;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Encodings.Web;

namespace Web.Pages.Pages.Authentication
{
    public partial class Forgot
    {
        [Inject]
        ILogger<Forgot> _logger { get; set; }
        [Inject]
        UserManager<AppUser> _userManager { get; set; }        
        [Inject]
        IEmailService _emailService { get; set; }
        [Inject]
        IOptions<EmailConfiguration> emailOption { get; set; }

        string Email { get; set; }

        async Task SubmitForgot()
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(Email);

                if (user == null)
                {
                    _navigationManager.NavigateTo("/pages/authentication/login");
                    return;
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                //token = token.Replace('/', '_');

                var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

                var callbackUrl = _navigationManager.BaseUri + 
                    $"pages/authentication/reset-password/{encoded}";

                var emailRequest = new EmailRequest()
                {
                    From = emailOption.Value.From,
                    To = Email,
                    Subject = "Reset Password",
                    Body = $"Please reset your password by <a href=\"{callbackUrl}\">clicking here</a>."
                };

                await _emailService.SendAsync(emailRequest);
            }
            catch (Exception ex)
            {

            }
            _navigationManager.NavigateTo("/pages/authentication/login");
        }
    }
}
