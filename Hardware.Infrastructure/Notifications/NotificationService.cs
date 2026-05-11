using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Hardware.Infrastructure.Notifications;

public sealed class NotificationService(
    IHubContext<NotificationHub> hub,
    ILogger<NotificationService> logger) : INotificationService
{
    public async Task NotifyNewSalesOrderAsync(Guid orderId, string orderNumber, decimal grandTotal)
    {
        try
        {
            await hub.Clients.All.SendAsync("NewSalesOrder", new
            {
                orderId,
                orderNumber,
                grandTotal,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send NewSalesOrder notification for {OrderNumber}", orderNumber);
        }
    }

    public async Task NotifyLowStockAsync(Guid productId, string productName, string sku, int quantityOnHand, int reorderLevel, string warehouseName)
    {
        try
        {
            await hub.Clients.All.SendAsync("LowStock", new
            {
                productId,
                productName,
                sku,
                quantityOnHand,
                reorderLevel,
                warehouseName,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send LowStock notification for {SKU}", sku);
        }
    }

    public async Task NotifyPurchaseOrderReceivedAsync(Guid orderId, string poNumber, string supplierName)
    {
        try
        {
            await hub.Clients.All.SendAsync("PurchaseOrderReceived", new
            {
                orderId,
                poNumber,
                supplierName,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send PurchaseOrderReceived notification for {PONumber}", poNumber);
        }
    }

    public async Task NotifySalesReturnAsync(Guid returnId, string returnNumber, string orderNumber)
    {
        try
        {
            await hub.Clients.All.SendAsync("SalesReturn", new
            {
                returnId,
                returnNumber,
                orderNumber,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send SalesReturn notification for {ReturnNumber}", returnNumber);
        }
    }
}
