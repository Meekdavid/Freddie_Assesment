using Common.ConfigurationSettings;
using Freddie.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Freddie.Helpers.Services
{
    public class SmtpEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendQualificationEmail(Candidate candidate)
        {
            try
            {
                var message = CreateEmailMessage(candidate);
                await SendEmailAsync(message);
                _logger.LogInformation("Email sent successfully to {Email}", candidate.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", candidate.Email);
                //throw;
            }
        }

        private MimeMessage CreateEmailMessage(Candidate candidate)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Freddie AI Recruiter", ConfigSettings.ApplicationSetting.EmailDetails.SenderEmail));
            message.To.Add(new MailboxAddress(candidate.FullName, candidate.Email));
            //message.To.Add(new MailboxAddress(candidate.FullName, "davidmboko2020@gmail.com"));
            message.Subject = "Application Update";
            message.Date = DateTimeOffset.Now.DateTime;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head><meta charset='UTF-8'></head>
                <body style='font-family: Arial, sans-serif; line-height: 1.5; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='margin-bottom: 20px;'>
                        <p style='font-size: 16px; margin-bottom: 10px;'>
                            Hi <strong>{candidate.FullName}</strong>,
                        </p>
                        <p style='font-size: 16px;'>
                            Thanks for applying! Based on our initial screening, we'd like to
                            move forward with your application.
                        </p>
                    </div>
                    <div style='margin-top: 30px; font-size: 14px; color: #666;'>
                        <p>Best regards,</p>
                        <p>The Hiring Team</p>
                    </div>
                </body>
                </html>",
                //TextBody = $"Hi {candidate.FullName},\n\nThanks for applying! We'd like to move forward with your application.\n\nBest regards,\nThe Hiring Team"
            };

            message.Body = bodyBuilder.ToMessageBody();
            return message;
        }

        private async Task SendEmailAsync(MimeMessage message)
        {
            using var client = new SmtpClient();

            try
            {
                // SMTP configuration (using Gmail SMTP as example)
                await client.ConnectAsync(
                    ConfigSettings.ApplicationSetting.EmailDetails.SMTPServer,
                     ConfigSettings.ApplicationSetting.EmailDetails.Port,
                    SecureSocketOptions.StartTls);

                // Note: For Gmail you may need to generate an "App Password"
                await client.AuthenticateAsync(
                    ConfigSettings.ApplicationSetting.EmailDetails.SenderEmail,
                    "ednnppsbnlhjykav");

                await client.SendAsync(message);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}
