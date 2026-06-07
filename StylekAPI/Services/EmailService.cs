using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using StylekAPI.Helpers;

namespace StylekAPI.Services;

public class EmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendOtpEmailAsync(string toEmail, string code)
    {
        var subject = "Stylek - Password Reset OTP";
        var body = $"""
            <h2>Stylek Password Reset</h2>
            <p>Your OTP code is: <strong>{code}</strong></p>
            <p>This code expires in 10 minutes.</p>
            """;

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendOrderConfirmationAsync(string toEmail, string orderNumber, decimal total)
    {
        var subject = $"Stylek - Order Confirmation #{orderNumber}";
        var body = $"""
            <h2>Thank you for your order!</h2>
            <p>Order Number: <strong>{orderNumber}</strong></p>
            <p>Total: <strong>{total:N2} EGP</strong></p>
            """;

        await SendEmailAsync(toEmail, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.User))
        {
            _logger.LogWarning("Email not configured. OTP/content for {Email}: {Subject}", toEmail, subject);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.From));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_settings.User, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
