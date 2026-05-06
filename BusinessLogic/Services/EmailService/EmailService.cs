using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Services.EmailService;

public class EmailService(IConfiguration options) : IEmailService
{
    public async Task SendVerificationEmail(string toEmail, string code)
    {
       
    }
}