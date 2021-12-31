using ApplicationCore.Configurations;
using ApplicationCore.Interfaces;
using ApplicationCore.Requests;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Services
{
    public class SmtpEmailService : IEmailService
    {
        readonly ILogger _logger;
        readonly IUserSession _userSession;
        readonly EmailConfiguration _config;

        public SmtpEmailService(
            ILogger<SmtpEmailService> logger,
            IUserSession userSession,
            IOptions<EmailConfiguration> config)
        {
            _logger = logger;
            _userSession = userSession;
            _config = config.Value;
        }

        public async Task SendAsync(EmailRequest request)
        {
            try
            {
                var email = new MimeMessage
                {
                    Sender = new MailboxAddress(_config.DisplayName, request.From ?? _config.From),
                    Subject = request.Subject,
                    Body = new BodyBuilder
                    {
                        HtmlBody = request.Body
                    }.ToMessageBody()
                };
                email.To.Add(MailboxAddress.Parse(request.To));
                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_config.Host, _config.Port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_config.UserName, _config.Password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email {@0} {UserId}", 
                    request, _userSession.UserId);
            }
        }
    }
}
