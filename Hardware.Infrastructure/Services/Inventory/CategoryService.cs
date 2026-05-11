using AutoMapper;
using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Inventory;
using Hardware.Application.Exceptions;
using Hardware.Application.Services.Inventory;
using Hardware.Domain.Entities.Inventory;
using Hardware.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Hardware.Infrastructure.Services.Inventory;

public sealed class CategoryService(IUnitOfWork uow, IMapper mapper) : ICategoryService
{
    public async Task<CategoryDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await uow.Repository<Category>().Query()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("Category", id);
        return mapper.Map<CategoryDto>(entity);
    }

    public async Task<PagedResult<CategoryDto>> GetAllAsync(PagedRequestDto request, CancellationToken ct = default)
    {
        IQueryable<Category> query = uow.Repository<Category>().Query()
            .Include(c => c.ParentCategory);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(c => c.Name.Contains(request.Search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return PagedResult<CategoryDto>.Create(mapper.Map<IReadOnlyList<CategoryDto>>(items), request.Page, request.PageSize, total);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default)
    {
        if (await uow.Repository<Category>().ExistsAsync(c => c.Name == dto.Name && c.ParentCategoryId == dto.ParentCategoryId, ct))
            throw new ConflictException($"Category '{dto.Name}' already exists under the same parent.");

        var entity = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            ParentCategoryId = dto.ParentCategoryId,
            Status = dto.Status
        };

        await uow.Repository<Category>().AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken ct = default)
    {
        var entity = await uow.Repository<Category>().Query(tracking: true)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("Category", id);

        if (entity.Id == dto.ParentCategoryId)
            throw new BusinessException("A category cannot be its own parent.");

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.ParentCategoryId = dto.ParentCategoryId;
        entity.Status = dto.Status;

        await uow.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await uow.Repository<Category>().GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Category", id);

        if (await uow.Repository<Category>().ExistsAsync(c => c.ParentCategoryId == id, ct))
            throw new BusinessException("Cannot delete a category that has subcategories.");

        if (await uow.Repository<Product>().ExistsAsync(p => p.CategoryId == id, ct))
            throw new BusinessException("Cannot delete a category that has products.");

        uow.Repository<Category>().Delete(entity);
        await uow.SaveChangesAsync(ct);
    }
}
