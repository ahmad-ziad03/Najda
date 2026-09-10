namespace Najda.Services;

// Abstraction so the rest of the app never depends on a specific provider.
// A message can carry both plain text and HTML.
public interface IEmailSender
{
    Task SendAsync(string toEmail, string toName, string subject, string htmlBody, string? textBody = null);
}
