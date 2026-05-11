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

public sealed class WarehouseService(IUnitOfWork uow, IMapper mapper) : IWarehouseService
{
    public async Task<WarehouseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await uow.Repository<Warehouse>().GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Warehouse", id);
        return mapper.Map<WarehouseDto>(entity);
    }

    public async Task<PagedResult<WarehouseDto>> GetAllAsync(PagedRequestDto request, CancellationToken ct = default)
    {
        var query = uow.Repository<Warehouse>().Query();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(w => w.Name.Contains(request.Search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(w => w.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return PagedResult<WarehouseDto>.Create(mapper.Map<IReadOnlyList<WarehouseDto>>(items), request.Page, request.PageSize, total);
    }

    public async Task<WarehouseDto> CreateAsync(CreateWarehouseDto dto, CancellationToken ct = default)
    {
        var entity = new Warehouse
        {
            Name = dto.Name,
            Address = dto.Address,
            City = dto.City,
            Country = dto.Country,
            Status = CommonStatus.Active
        };

        await uow.Repository<Warehouse>().AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return mapper.Map<WarehouseDto>(entity);
    }

    public async Task<WarehouseDto> UpdateAsync(Guid id, UpdateWarehouseDto dto, CancellationToken ct = default)
    {
        var entity = await uow.Repository<Warehouse>().Query(tracking: true)
            .FirstOrDefaultAsync(w => w.Id == id, ct)
            ?? throw new NotFoundException("Warehouse", id);

        entity.Name = dto.Name;
        entity.Address = dto.Address;
        entity.City = dto.City;
        entity.Country = dto.Country;
        entity.Status = dto.Status;

        await uow.SaveChangesAsync(ct);
        return mapper.Map<WarehouseDto>(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await uow.Repository<Warehouse>().GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Warehouse", id);

        if (await uow.Repository<StockItem>().ExistsAsync(s => s.WarehouseId == id && s.QuantityOnHand > 0, ct))
            throw new BusinessException("Cannot delete a warehouse that still holds stock.");

        uow.Repository<Warehouse>().Delete(entity);
        await uow.SaveChangesAsync(ct);
    }
}
