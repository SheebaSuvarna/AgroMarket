using MimeKit;
namespace AgroMarket.Controllers;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string recipientEmail, string subject, string message)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("AgroMarket", _configuration["EmailSettings:SenderEmail"]));
            email.To.Add(new MailboxAddress("", recipientEmail));
            email.Subject = subject;

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(
                    _configuration["EmailSettings:SmtpServer"],
                    int.Parse(_configuration["EmailSettings:SmtpPort"]),
                    MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(
                    _configuration["EmailSettings:SenderEmail"],
                    _configuration["EmailSettings:SenderPassword"]);
                await client.SendAsync(email);
                await client.DisconnectAsync(true);
            }

            Console.WriteLine("Email sent successfully to " + recipientEmail); // Log success
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending email: " + ex.Message); // Log failure
                                                                     // Optionally, you could log this to a logging service if available
        }
    }
}
