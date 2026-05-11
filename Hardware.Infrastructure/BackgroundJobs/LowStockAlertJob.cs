using Hardware.Domain.Entities.Inventory;
using Hardware.Domain.Enums;
using Hardware.Domain.Interfaces.Repositories;
using Hardware.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Hardware.Infrastructure.BackgroundJobs;

public sealed class LowStockAlertJob(
    IUnitOfWork uow,
    IOptions<EmailSettings> emailOptions,
    IOptions<SeedDataSettings> seedOptions,
    ILogger<LowStockAlertJob> logger)
{
    private readonly EmailSettings _email = emailOptions.Value;
    private readonly SeedDataSettings _seed = seedOptions.Value;

    public async Task RunAsync(CancellationToken ct = default)
    {
        var lowStockItems = await uow.Repository<StockItem>().Query(tracking: false)
            .Include(s => s.Product).ThenInclude(p => p.Category)
            .Include(s => s.Warehouse)
            .Where(s => s.QuantityOnHand <= s.Product.ReorderLevel
                     && s.Product.Status == CommonStatus.Active)
            .OrderBy(s => s.QuantityOnHand)
            .Take(50)
            .ToListAsync(ct);

        if (!lowStockItems.Any())
        {
            logger.LogInformation("Low stock alert job: no low-stock items found");
            return;
        }

        logger.LogInformation("Low stock alert job: {Count} items below reorder level", lowStockItems.Count);

        if (string.IsNullOrWhiteSpace(_email.SmtpHost) || string.IsNullOrWhiteSpace(_email.FromEmail))
        {
            logger.LogWarning("Low stock alert job: SMTP not configured, skipping email");
            return;
        }

        try
        {
            var body = BuildEmailBody(lowStockItems);
            await SendEmailAsync(_email.FromEmail, $"[Hardware] Low Stock Alert — {lowStockItems.Count} item(s)", body, ct);
            logger.LogInformation("Low stock alert email sent");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send low stock alert email");
        }
    }

    private static string BuildEmailBody(IReadOnlyList<StockItem> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<h2>Low Stock Alert</h2>");
        sb.AppendLine($"<p>The following {items.Count} item(s) are at or below their reorder level:</p>");
        sb.AppendLine("<table border='1' cellpadding='6' cellspacing='0' style='border-collapse:collapse'>");
        sb.AppendLine("<tr><th>SKU</th><th>Product</th><th>Category</th><th>Warehouse</th><th>On Hand</th><th>Reorder Level</th></tr>");

        foreach (var item in items)
        {
            sb.AppendLine($"<tr>" +
                $"<td>{item.Product.SKU}</td>" +
                $"<td>{item.Product.Name}</td>" +
                $"<td>{item.Product.Category.Name}</td>" +
                $"<td>{item.Warehouse.Name}</td>" +
                $"<td style='color:red;font-weight:bold'>{item.QuantityOnHand}</td>" +
                $"<td>{item.Product.ReorderLevel}</td>" +
                $"</tr>");
        }

        sb.AppendLine("</table>");
        sb.AppendLine($"<p style='color:gray;font-size:12px'>Generated at {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>");
        return sb.ToString();
    }

    private async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct)
    {
        using var client = new SmtpClient(_email.SmtpHost, _email.SmtpPort)
        {
            EnableSsl = _email.EnableSsl,
            Credentials = string.IsNullOrWhiteSpace(_email.SmtpUsername)
                ? null
                : new NetworkCredential(_email.SmtpUsername, _email.SmtpPassword)
        };

        using var message = new MailMessage
        {
            From    = new MailAddress(_email.FromEmail, _email.FromName),
            Subject = subject,
            Body    = htmlBody,
            IsBodyHtml = true
        };

        message.To.Add(to);

        await client.SendMailAsync(message, ct);
    }
}
