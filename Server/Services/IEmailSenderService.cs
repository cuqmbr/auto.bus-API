namespace Server.Services;

public interface IEmailSenderService
{
    Task<(bool succeeded, string message)> SendMail(string toEmail, string subject, string message);
}