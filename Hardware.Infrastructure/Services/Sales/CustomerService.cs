using AutoMapper;
using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Sales;
using Hardware.Application.Exceptions;
using Hardware.Application.Services.Sales;
using Hardware.Domain.Entities.Sales;
using Hardware.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Hardware.Infrastructure.Services.Sales;

public sealed class CustomerService(IUnitOfWork uow, IMapper mapper) : ICustomerService
{
    public async Task<CustomerDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await uow.Repository<Customer>().GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Customer", id);
        return mapper.Map<CustomerDto>(entity);
    }

    public async Task<PagedResult<CustomerDto>> GetAllAsync(PagedRequestDto request, CancellationToken ct = default)
    {
        var query = uow.Repository<Customer>().Query();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(c => c.FirstName.Contains(request.Search)
                || c.LastName.Contains(request.Search)
                || (c.Email != null && c.Email.Contains(request.Search)));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return PagedResult<CustomerDto>.Create(mapper.Map<IReadOnlyList<CustomerDto>>(items), request.Page, request.PageSize, total);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto, CancellationToken ct = default)
    {
        var entity = new Customer
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            Country = dto.Country,
            CustomerType = dto.CustomerType,
            CreditLimit = dto.CreditLimit,
            Status = Hardware.Domain.Enums.CommonStatus.Active
        };

        await uow.Repository<Customer>().AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return mapper.Map<CustomerDto>(entity);
    }

    public async Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerDto dto, CancellationToken ct = default)
    {
        var entity = await uow.Repository<Customer>().Query(tracking: true)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("Customer", id);

        entity.FirstName = dto.FirstName;
        entity.LastName = dto.LastName;
        entity.Email = dto.Email;
        entity.Phone = dto.Phone;
        entity.Address = dto.Address;
        entity.City = dto.City;
        entity.Country = dto.Country;
        entity.CustomerType = dto.CustomerType;
        entity.CreditLimit = dto.CreditLimit;
        entity.Status = dto.Status;

        await uow.SaveChangesAsync(ct);
        return mapper.Map<CustomerDto>(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await uow.Repository<Customer>().GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Customer", id);

        uow.Repository<Customer>().Delete(entity);
        await uow.SaveChangesAsync(ct);
    }
}
