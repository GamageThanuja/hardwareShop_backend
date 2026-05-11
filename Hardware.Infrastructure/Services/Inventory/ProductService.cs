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

public sealed class ProductService(IUnitOfWork uow, IMapper mapper) : IProductService
{
    public async Task<ProductDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await uow.Repository<Product>().Query()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Product", id);
        return mapper.Map<ProductDto>(entity);
    }

    public async Task<PagedResult<ProductDto>> GetAllAsync(PagedRequestDto request, CancellationToken ct = default)
    {
        IQueryable<Product> query = uow.Repository<Product>().Query()
            .Include(p => p.Category)
            .Include(p => p.Supplier);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p => p.Name.Contains(request.Search) || p.SKU.Contains(request.Search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return PagedResult<ProductDto>.Create(mapper.Map<IReadOnlyList<ProductDto>>(items), request.Page, request.PageSize, total);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        if (await uow.Repository<Product>().ExistsAsync(p => p.SKU == dto.SKU, ct))
            throw new ConflictException($"Product with SKU '{dto.SKU}' already exists.");

        if (!await uow.Repository<Category>().ExistsAsync(c => c.Id == dto.CategoryId, ct))
            throw new NotFoundException("Category", dto.CategoryId);

        var entity = new Product
        {
            SKU = dto.SKU,
            Name = dto.Name,
            Description = dto.Description,
            Barcode = dto.Barcode,
            CategoryId = dto.CategoryId,
            SupplierId = dto.SupplierId,
            UnitPrice = dto.UnitPrice,
            CostPrice = dto.CostPrice,
            Unit = dto.Unit,
            ReorderLevel = dto.ReorderLevel,
            ReorderQuantity = dto.ReorderQuantity,
            Status = CommonStatus.Active
        };

        await uow.Repository<Product>().AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default)
    {
        var entity = await uow.Repository<Product>().Query(tracking: true)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Product", id);

        if (!await uow.Repository<Category>().ExistsAsync(c => c.Id == dto.CategoryId, ct))
            throw new NotFoundException("Category", dto.CategoryId);

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.Barcode = dto.Barcode;
        entity.CategoryId = dto.CategoryId;
        entity.SupplierId = dto.SupplierId;
        entity.UnitPrice = dto.UnitPrice;
        entity.CostPrice = dto.CostPrice;
        entity.Unit = dto.Unit;
        entity.ReorderLevel = dto.ReorderLevel;
        entity.ReorderQuantity = dto.ReorderQuantity;
        entity.Status = dto.Status;

        await uow.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await uow.Repository<Product>().GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Product", id);

        uow.Repository<Product>().Delete(entity);
        await uow.SaveChangesAsync(ct);
    }
}
