using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace TellaStore.Services;

public class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;
    public EmailSender(ILogger<EmailSender> logger) { _logger = logger; }
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogInformation("Email to {Email}: {Subject}", email, subject);
        return Task.CompletedTask;
    }
}
