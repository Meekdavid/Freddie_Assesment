using Common.ConfigurationSettings;
using Freddie.Models;
using Google;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Utils;

namespace Freddie.Helpers.Services
{
    public class GmailApiService
    {
        private readonly GmailService _gmailService;
        private readonly ILogger<GmailApiService> _logger;

        public GmailApiService(GoogleAuthService authService, ILogger<GmailApiService> logger)
        {
            _logger = logger;

            // Initialize with proper authentication
            _gmailService = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = authService.GetCredential(),
                ApplicationName = "Freddie AI Recruiter"
            });
        }

        public async Task SendQualificationEmail(Candidate candidate)
        {
            try
            {
                // 1. Create properly formatted MIME message
                var mimeMessage = CreateMimeMessage(candidate);

                // 2. Validate message before sending
                ValidateMimeMessage(mimeMessage);

                // 3. Send with detailed error handling
                await SendEmailThroughGmailApi(mimeMessage);

                _logger.LogInformation("Email successfully sent to {Email}", candidate.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", candidate.Email);
                throw new ApplicationException($"Failed to send email: {ex.Message}", ex);
            }
        }

        private MimeMessage CreateMimeMessage(Candidate candidate)
        {
            var message = new MimeMessage();

            // Set proper headers
            message.From.Add(new MailboxAddress("Freddie AI Recruiter", ConfigSettings.ApplicationSetting.EmailDetails.SenderEmail));
            message.To.Add(new MailboxAddress(candidate.FullName, candidate.Email));
            message.Subject = "Application Update";
            message.Date = DateTimeOffset.Now.DateTime;
            message.MessageId = MimeUtils.GenerateMessageId();

            // Create multipart body
            var bodyBuilder = new BodyBuilder();

            bodyBuilder.HtmlBody = $@"
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
            </html>";

            // Add plain text fallback
            bodyBuilder.TextBody = $"Hi {candidate.FullName},\n\nThanks for applying! Based on our initial screening, we'd like to move forward with your application.\n\nBest regards,\nThe Hiring Team";

            message.Body = bodyBuilder.ToMessageBody();
            return message;
        }

        private void ValidateMimeMessage(MimeMessage message)
        {
            if (message.From.Count == 0)
                throw new ArgumentException("Message must have a From address");

            if (message.To.Count == 0)
                throw new ArgumentException("Message must have a To address");

            if (string.IsNullOrEmpty(message.Subject))
                throw new ArgumentException("Message must have a subject");

            if (message.Body == null)
                throw new ArgumentException("Message must have a body");
        }

        private async Task SendEmailThroughGmailApi(MimeMessage mimeMessage)
        {
            try
            {
                // Convert to RFC 2822 format with proper line endings
                using var memoryStream = new MemoryStream();
                var options = FormatOptions.Default.Clone();
                options.NewLineFormat = NewLineFormat.Dos; // CRLF line endings
                await mimeMessage.WriteToAsync(options, memoryStream);
                memoryStream.Position = 0;

                // Create raw message with proper Base64URL encoding
                var rawMessage = new Message
                {
                    Raw = Convert.ToBase64String(memoryStream.ToArray())
                        .Replace('+', '-')
                        .Replace('/', '_')
                        .Replace("=", "")
                };

                // Verify credentials
                var profile = await _gmailService.Users.GetProfile("me").ExecuteAsync();
                _logger.LogDebug("Authenticated as: {Email}", profile.EmailAddress);

                // Send message
                var request = _gmailService.Users.Messages.Send(rawMessage, "me");
                await request.ExecuteAsync();
            }
            catch (GoogleApiException ex) when (ex.Error.Code == 400)
            {
                _logger.LogError("Bad Request Details:\n{Error}", ex.Error.ToString());
                throw new ApplicationException("Invalid email format or content", ex);
            }
        }
    }
}
