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

public sealed class SupplierService(IUnitOfWork uow, IMapper mapper) : ISupplierService
{
    public async Task<SupplierDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await uow.Repository<Supplier>().GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Supplier", id);
        return mapper.Map<SupplierDto>(entity);
    }

    public async Task<PagedResult<SupplierDto>> GetAllAsync(PagedRequestDto request, CancellationToken ct = default)
    {
        var query = uow.Repository<Supplier>().Query();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(s => s.Name.Contains(request.Search) || (s.Email != null && s.Email.Contains(request.Search)));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(s => s.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return PagedResult<SupplierDto>.Create(mapper.Map<IReadOnlyList<SupplierDto>>(items), request.Page, request.PageSize, total);
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto, CancellationToken ct = default)
    {
        var entity = new Supplier
        {
            Name = dto.Name,
            ContactName = dto.ContactName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            Country = dto.Country,
            Status = CommonStatus.Active
        };

        await uow.Repository<Supplier>().AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return mapper.Map<SupplierDto>(entity);
    }

    public async Task<SupplierDto> UpdateAsync(Guid id, UpdateSupplierDto dto, CancellationToken ct = default)
    {
        var entity = await uow.Repository<Supplier>().Query(tracking: true)
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException("Supplier", id);

        entity.Name = dto.Name;
        entity.ContactName = dto.ContactName;
        entity.Email = dto.Email;
        entity.Phone = dto.Phone;
        entity.Address = dto.Address;
        entity.City = dto.City;
        entity.Country = dto.Country;
        entity.Status = dto.Status;

        await uow.SaveChangesAsync(ct);
        return mapper.Map<SupplierDto>(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await uow.Repository<Supplier>().GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Supplier", id);

        if (await uow.Repository<Product>().ExistsAsync(p => p.SupplierId == id, ct))
            throw new BusinessException("Cannot delete a supplier that has associated products.");

        uow.Repository<Supplier>().Delete(entity);
        await uow.SaveChangesAsync(ct);
    }
}
