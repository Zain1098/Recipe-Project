namespace Recipe_Project.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody);
        Task<bool> SendPasswordResetOtpAsync(string toEmail, string userName, string otp);
    }
}
