using FluentEmail.Core;
using Microsoft.Extensions.Configuration;

namespace BusinessLogic.Services.EmailService;

public class EmailService(IConfiguration options, IFluentEmail  fluentEmail) : IEmailService
{
    public async Task SendVerificationEmail(string toEmail, string code)
    {
       await fluentEmail
           .To(toEmail)
           .Subject("Verification email for Taskify")
           .Body("Your verification code is " + code)
           .SendAsync();
    }

    
}