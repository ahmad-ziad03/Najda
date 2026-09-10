using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Najda.Services;

// Options bound from configuration (appsettings / user secrets).
public class BrevoOptions
{
    public string? ApiKey { get; set; }
    public string SenderEmail { get; set; } = "no-reply@najda.jo";
    public string SenderName { get; set; } = "Najda";
}

// Sends email through Brevo's transactional API (https://api.brevo.com/v3/smtp/email).
// If no API key is configured, it does NOT throw — it logs the email instead, so
// the app keeps working in development without credentials.
public class BrevoEmailSender : IEmailSender
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly BrevoOptions _opt;
    private readonly ILogger<BrevoEmailSender> _log;

    public BrevoEmailSender(IHttpClientFactory httpFactory, BrevoOptions opt, ILogger<BrevoEmailSender> log)
    {
        _httpFactory = httpFactory;
        _opt = opt;
        _log = log;
    }

    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody, string? textBody = null)
    {
        if (string.IsNullOrWhiteSpace(_opt.ApiKey))
        {
            // No credentials: log instead of sending, so nothing breaks in dev.
            _log.LogInformation("[EMAIL - not sent, no Brevo API key] To: {To} | Subject: {Subject}", toEmail, subject);
            return;
        }

        var payload = new
        {
            sender = new { email = _opt.SenderEmail, name = _opt.SenderName },
            to = new[] { new { email = toEmail, name = string.IsNullOrWhiteSpace(toName) ? toEmail : toName } },
            subject,
            htmlContent = htmlBody,
            textContent = string.IsNullOrWhiteSpace(textBody) ? StripHtml(htmlBody) : textBody,
        };

        var client = _httpFactory.CreateClient();
        var req = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
        req.Headers.Add("api-key", _opt.ApiKey);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        try
        {
            var res = await client.SendAsync(req);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                _log.LogWarning("Brevo email to {To} failed: {Status} {Body}", toEmail, res.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            // Never let a mail failure break the user's action.
            _log.LogError(ex, "Error sending Brevo email to {To}", toEmail);
        }
    }

    private static string StripHtml(string html)
        => System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
}
