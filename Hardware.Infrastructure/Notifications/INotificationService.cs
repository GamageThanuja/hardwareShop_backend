namespace Hardware.Infrastructure.Notifications;

public interface INotificationService
{
    Task NotifyNewSalesOrderAsync(Guid orderId, string orderNumber, decimal grandTotal);
    Task NotifyLowStockAsync(Guid productId, string productName, string sku, int quantityOnHand, int reorderLevel, string warehouseName);
    Task NotifyPurchaseOrderReceivedAsync(Guid orderId, string poNumber, string supplierName);
    Task NotifySalesReturnAsync(Guid returnId, string returnNumber, string orderNumber);
}
