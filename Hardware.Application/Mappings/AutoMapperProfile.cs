using AutoMapper;
using Hardware.Application.DTOs.Inventory;
using Hardware.Application.DTOs.Purchasing;
using Hardware.Application.DTOs.Sales;
using Hardware.Domain.Entities.Inventory;
using Hardware.Domain.Entities.Purchasing;
using Hardware.Domain.Entities.Sales;
using Hardware.Domain.Enums;

namespace Hardware.Application.Mappings;

public sealed class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Inventory
        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.ParentCategoryName, o => o.MapFrom(s => s.ParentCategory != null ? s.ParentCategory.Name : null));
        CreateMap<Supplier, SupplierDto>();
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier != null ? s.Supplier.Name : null));
        CreateMap<Warehouse, WarehouseDto>();
        CreateMap<StockItem, StockItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name))
            .ForMember(d => d.ProductSKU, o => o.MapFrom(s => s.Product.SKU))
            .ForMember(d => d.WarehouseName, o => o.MapFrom(s => s.Warehouse.Name))
            .ForMember(d => d.QuantityAvailable, o => o.MapFrom(s => s.QuantityOnHand - s.QuantityReserved));
        CreateMap<InventoryTransaction, InventoryTransactionDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name))
            .ForMember(d => d.WarehouseName, o => o.MapFrom(s => s.Warehouse.Name));

        // Sales
        CreateMap<Customer, CustomerDto>();
        CreateMap<Payment, PaymentDto>()
            .ForMember(d => d.OrderNumber, o => o.MapFrom(s => s.SalesOrder.OrderNumber));
        CreateMap<SalesOrderItem, SalesOrderItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name))
            .ForMember(d => d.ProductSKU, o => o.MapFrom(s => s.Product.SKU));
        CreateMap<SalesOrder, SalesOrderDto>()
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer != null
                ? s.Customer.FirstName + " " + s.Customer.LastName : null))
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));

        // Purchasing
        CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name))
            .ForMember(d => d.ProductSKU, o => o.MapFrom(s => s.Product.SKU));
        CreateMap<PurchaseOrder, PurchaseOrderDto>()
            .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier.Name))
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));
    }
}
