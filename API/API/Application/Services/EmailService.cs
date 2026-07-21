using System.Net;
using System.Net.Mail;
using API.Application.Services.IServices;
using API.Domain.Entities;

namespace API.Application.Services
{
    public class EmailService : IEmailService
 
{
    private readonly EmailSettings _settings;

    public EmailService(IConfiguration config)
    {
        _settings = config.GetSection("EmailSettings").Get<EmailSettings>()!;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var message = new MailMessage
        {
            From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(to);

        using var smtp = new SmtpClient(_settings.SmtpServer, _settings.Port)
        {
            Credentials = new NetworkCredential(_settings.Username, _settings.Password),
            EnableSsl = true
        };

        await smtp.SendMailAsync(message);
    }
}
}