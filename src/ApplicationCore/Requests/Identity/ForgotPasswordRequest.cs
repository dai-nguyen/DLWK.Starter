using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.Requests.Identity
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
