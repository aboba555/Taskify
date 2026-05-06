using DataAccess.Models;

namespace BusinessLogic.Services.EmailService;

public interface IEmailService
{
    Task SendVerificationEmail(string toEmail, string code);
}