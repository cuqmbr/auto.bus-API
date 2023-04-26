using System.Security.Authentication;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Server.Configurations;

namespace Server.Services;

public class EmailSenderService : IEmailSenderService
{
    private readonly SmtpCredentials _smtpCredentials;
    private readonly ISmtpClient _smtpClient;
    private readonly IConfiguration _configuration;
    
    public EmailSenderService(IOptions<SmtpCredentials> smtpCredentials, IConfiguration configuration)
    {
        _configuration = configuration;

        _smtpCredentials = smtpCredentials.Value;
        _smtpClient = new SmtpClient();
        _smtpClient.SslProtocols = SslProtocols.Ssl3 | SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13;
    }
    
    public async Task<(bool succeeded, string message)> SendMail(string toEmail, string subject, string message)
    {
        string applicationName = _configuration.GetValue<string>("ApplicationName");
        
        MimeMessage mailMessage = new MimeMessage();
        
        mailMessage.From.Add(new MailboxAddress("auto.bus", _smtpCredentials.User));
        mailMessage.To.Add(new MailboxAddress("auto.bus client", toEmail));
        mailMessage.Subject = $"{applicationName}. {subject}";
        mailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message};

        try
        {
            await _smtpClient.ConnectAsync(_smtpCredentials.Host, _smtpCredentials.Port, true);
            await _smtpClient.AuthenticateAsync(Encoding.ASCII, _smtpCredentials.User, _smtpCredentials.Password);
            await _smtpClient.SendAsync(mailMessage);
            await _smtpClient.DisconnectAsync(true);
            return (true, "Letter has been sent successfully");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}