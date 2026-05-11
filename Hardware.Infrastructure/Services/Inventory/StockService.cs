using AutoMapper;
using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Inventory;
using Hardware.Application.Exceptions;
using Hardware.Application.Services.Inventory;
using Hardware.Domain.Entities.Inventory;
using Hardware.Domain.Enums;
using Hardware.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Hardware.Infrastructure.Services.Inventory;

public sealed class StockService(IUnitOfWork uow, IMapper mapper) : IStockService
{
    public async Task<IReadOnlyList<StockItemDto>> GetStockByProductAsync(Guid productId, CancellationToken ct = default)
    {
        if (!await uow.Repository<Product>().ExistsAsync(p => p.Id == productId, ct))
            throw new NotFoundException("Product", productId);

        var items = await uow.Repository<StockItem>().Query()
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.ProductId == productId)
            .ToListAsync(ct);

        return mapper.Map<IReadOnlyList<StockItemDto>>(items);
    }

    public async Task<PagedResult<StockItemDto>> GetStockByWarehouseAsync(Guid warehouseId, PagedRequestDto request, CancellationToken ct = default)
    {
        if (!await uow.Repository<Warehouse>().ExistsAsync(w => w.Id == warehouseId, ct))
            throw new NotFoundException("Warehouse", warehouseId);

        IQueryable<StockItem> query = uow.Repository<StockItem>().Query()
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.WarehouseId == warehouseId);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(s => s.Product.Name.Contains(request.Search) || s.Product.SKU.Contains(request.Search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(s => s.Product.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return PagedResult<StockItemDto>.Create(mapper.Map<IReadOnlyList<StockItemDto>>(items), request.Page, request.PageSize, total);
    }

    public async Task<InventoryTransactionDto> RecordTransactionAsync(CreateInventoryTransactionDto dto, CancellationToken ct = default)
    {
        if (!await uow.Repository<Product>().ExistsAsync(p => p.Id == dto.ProductId, ct))
            throw new NotFoundException("Product", dto.ProductId);

        if (!await uow.Repository<Warehouse>().ExistsAsync(w => w.Id == dto.WarehouseId, ct))
            throw new NotFoundException("Warehouse", dto.WarehouseId);

        var stockItem = await uow.Repository<StockItem>().Query(tracking: true)
            .FirstOrDefaultAsync(s => s.ProductId == dto.ProductId && s.WarehouseId == dto.WarehouseId, ct);

        if (stockItem is null)
        {
            stockItem = new StockItem { ProductId = dto.ProductId, WarehouseId = dto.WarehouseId };
            await uow.Repository<StockItem>().AddAsync(stockItem, ct);
        }

        var isOutbound = dto.TransactionType is InventoryTransactionType.StockOut
            or InventoryTransactionType.TransferOut;

        if (isOutbound)
        {
            if (stockItem.QuantityOnHand < dto.Quantity)
                throw new BusinessException($"Insufficient stock. Available: {stockItem.QuantityOnHand}, Requested: {dto.Quantity}.");
            stockItem.QuantityOnHand -= dto.Quantity;
        }
        else
        {
            stockItem.QuantityOnHand += dto.Quantity;
        }

        var transaction = new InventoryTransaction
        {
            ProductId = dto.ProductId,
            WarehouseId = dto.WarehouseId,
            TransactionType = dto.TransactionType,
            Quantity = dto.Quantity,
            ReferenceType = dto.ReferenceType,
            ReferenceId = dto.ReferenceId,
            Notes = dto.Notes
        };

        await uow.Repository<InventoryTransaction>().AddAsync(transaction, ct);
        await uow.SaveChangesAsync(ct);

        var result = await uow.Repository<InventoryTransaction>().Query()
            .Include(t => t.Product)
            .Include(t => t.Warehouse)
            .FirstOrDefaultAsync(t => t.Id == transaction.Id, ct);

        return mapper.Map<InventoryTransactionDto>(result!);
    }

    public async Task<StockTransferResultDto> TransferStockAsync(TransferStockDto dto, CancellationToken ct = default)
    {
        if (!await uow.Repository<Product>().ExistsAsync(p => p.Id == dto.ProductId, ct))
            throw new NotFoundException("Product", dto.ProductId);

        if (!await uow.Repository<Warehouse>().ExistsAsync(w => w.Id == dto.FromWarehouseId, ct))
            throw new NotFoundException("Warehouse", dto.FromWarehouseId);

        if (!await uow.Repository<Warehouse>().ExistsAsync(w => w.Id == dto.ToWarehouseId, ct))
            throw new NotFoundException("Warehouse", dto.ToWarehouseId);

        var fromStock = await uow.Repository<StockItem>().Query(tracking: true)
            .FirstOrDefaultAsync(s => s.ProductId == dto.ProductId && s.WarehouseId == dto.FromWarehouseId, ct);

        if (fromStock is null || fromStock.QuantityOnHand < dto.Quantity)
            throw new BusinessException($"Insufficient stock in source warehouse. Available: {fromStock?.QuantityOnHand ?? 0}, Requested: {dto.Quantity}.");

        var toStock = await uow.Repository<StockItem>().Query(tracking: true)
            .FirstOrDefaultAsync(s => s.ProductId == dto.ProductId && s.WarehouseId == dto.ToWarehouseId, ct);

        if (toStock is null)
        {
            toStock = new StockItem { ProductId = dto.ProductId, WarehouseId = dto.ToWarehouseId };
            await uow.Repository<StockItem>().AddAsync(toStock, ct);
        }

        fromStock.QuantityOnHand -= dto.Quantity;
        toStock.QuantityOnHand   += dto.Quantity;

        var transferOut = new InventoryTransaction
        {
            ProductId       = dto.ProductId,
            WarehouseId     = dto.FromWarehouseId,
            TransactionType = InventoryTransactionType.TransferOut,
            Quantity        = dto.Quantity,
            ReferenceType   = "Transfer",
            Notes           = dto.Notes
        };

        var transferIn = new InventoryTransaction
        {
            ProductId       = dto.ProductId,
            WarehouseId     = dto.ToWarehouseId,
            TransactionType = InventoryTransactionType.TransferIn,
            Quantity        = dto.Quantity,
            ReferenceType   = "Transfer",
            Notes           = dto.Notes
        };

        await uow.Repository<InventoryTransaction>().AddAsync(transferOut, ct);
        await uow.Repository<InventoryTransaction>().AddAsync(transferIn, ct);
        await uow.SaveChangesAsync(ct);

        var outResult = await uow.Repository<InventoryTransaction>().Query(tracking: false)
            .Include(t => t.Product).Include(t => t.Warehouse)
            .FirstAsync(t => t.Id == transferOut.Id, ct);

        var inResult = await uow.Repository<InventoryTransaction>().Query(tracking: false)
            .Include(t => t.Product).Include(t => t.Warehouse)
            .FirstAsync(t => t.Id == transferIn.Id, ct);

        return new StockTransferResultDto(mapper.Map<InventoryTransactionDto>(outResult), mapper.Map<InventoryTransactionDto>(inResult));
    }

    public async Task<PagedResult<InventoryTransactionDto>> GetTransactionsAsync(PagedRequestDto request, Guid? productId, Guid? warehouseId, CancellationToken ct = default)
    {
        IQueryable<InventoryTransaction> query = uow.Repository<InventoryTransaction>().Query()
            .Include(t => t.Product)
            .Include(t => t.Warehouse);

        if (productId.HasValue)
            query = query.Where(t => t.ProductId == productId.Value);

        if (warehouseId.HasValue)
            query = query.Where(t => t.WarehouseId == warehouseId.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return PagedResult<InventoryTransactionDto>.Create(mapper.Map<IReadOnlyList<InventoryTransactionDto>>(items), request.Page, request.PageSize, total);
    }
}
