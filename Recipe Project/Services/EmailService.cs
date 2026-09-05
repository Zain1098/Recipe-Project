using System.Net;
using System.Net.Mail;

namespace Recipe_Project.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var host = _configuration["SmtpSettings:Host"] ?? "smtp.gmail.com";
            var portStr = _configuration["SmtpSettings:Port"] ?? "587";
            var senderEmail = _configuration["SmtpSettings:SenderEmail"];
            var senderName = _configuration["SmtpSettings:SenderName"] ?? "Deliciousa Recipes";
            var username = _configuration["SmtpSettings:Username"] ?? senderEmail;
            var password = _configuration["SmtpSettings:Password"];
            var enableSsl = bool.Parse(_configuration["SmtpSettings:EnableSsl"] ?? "true");

            int port = int.TryParse(portStr, out var p) ? p : 587;

            // Check if Gmail SMTP credentials are configured
            if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning(
                    "⚠️ Gmail SMTP credentials not configured in appsettings.json. Email to {ToEmail} with subject '{Subject}' was not dispatched via SMTP.",
                    toEmail, subject);
                return false;
            }

            try
            {
                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = enableSsl
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                message.To.Add(new MailAddress(toEmail));

                await client.SendMailAsync(message);
                _logger.LogInformation("✅ Email sent successfully to {ToEmail} with subject: {Subject}", toEmail, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send email to {ToEmail} via Gmail SMTP.", toEmail);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetOtpAsync(string toEmail, string userName, string otp)
        {
            var subject = $"{otp} is your Deliciousa Password Reset Code";
            var encodedName = WebUtility.HtmlEncode(userName);
            var year = DateTime.UtcNow.Year;

            var htmlBody = $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8" />
                <title>Password Reset OTP</title>
            </head>
            <body style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f7f5f0; margin: 0; padding: 20px;">
                <div style="max-width: 520px; margin: 0 auto; background: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.08); border: 1px solid #eee;">
                    <div style="background: linear-gradient(135deg, #2b2b2b, #1b1b1b); padding: 25px; text-align: center;">
                        <h1 style="color: #d1885b; margin: 0; font-size: 24px; font-weight: 700; letter-spacing: 1px;">Deliciousa Recipes</h1>
                        <p style="color: #dddddd; margin: 5px 0 0 0; font-size: 13px;">Culinary Arts & Community Kitchen</p>
                    </div>
                    <div style="padding: 30px 25px;">
                        <p style="font-size: 16px; color: #333333; margin-top: 0;">Hello <strong>{{encodedName}}</strong>,</p>
                        <p style="font-size: 14px; color: #666666; line-height: 1.6;">
                            We received a request to reset your password. Use the 6-digit verification code below to complete your password reset:
                        </p>
                        <div style="background: #fdf5f0; border: 2px dashed #d1885b; border-radius: 8px; padding: 18px; text-align: center; margin: 25px 0;">
                            <span style="font-family: monospace, Consolas, Courier; font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #b85d2e;">{{otp}}</span>
                        </div>
                        <p style="font-size: 13px; color: #888888; line-height: 1.5;">
                            ⏱️ This code will expire in <strong>15 minutes</strong>.<br />
                            If you did not request a password reset, please ignore this email or make sure your account is secure.
                        </p>
                        <hr style="border: none; border-top: 1px solid #eeeeee; margin: 25px 0;" />
                        <p style="font-size: 12px; color: #999999; text-align: center; margin-bottom: 0;">
                            &copy; {{year}} Deliciousa Recipes. All rights reserved.
                        </p>
                    </div>
                </div>
            </body>
            </html>
            """;

            // Always log OTP to server logs so developer can test even without SMTP configured
            _logger.LogInformation("🔑 [OTP DISPATCH] User: {User} ({Email}) | Reset Code: {Otp} | Expires in: 15 mins",
                userName, toEmail, otp);

            return await SendEmailAsync(toEmail, subject, htmlBody);
        }
    }
}
