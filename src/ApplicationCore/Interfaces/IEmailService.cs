using ApplicationCore.Requests;

namespace ApplicationCore.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(EmailRequest request);
    }
}
