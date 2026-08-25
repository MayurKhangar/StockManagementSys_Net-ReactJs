using Microsoft.EntityFrameworkCore;
using SmartStock.Application.Common;
using SmartStock.Application.DTOs.Category;
using SmartStock.Application.Interfaces;
using SmartStock.Domain.Entities;

namespace SmartStock.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ResultModel<List<CategoryDto>>> GetAllAsync()
    {
        var categories = await _unitOfWork.Categories.Query()
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ProductCount = c.Products.Count(p => !p.IsDeleted)
            })
            .OrderBy(c => c.Name)
            .ToListAsync();

        return ResultModel<List<CategoryDto>>.Ok(categories);
    }

    public async Task<ResultModel<CategoryDto>> GetByIdAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
        {
            return ResultModel<CategoryDto>.Fail("Category not found.");
        }

        return ResultModel<CategoryDto>.Ok(new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        });
    }

    public async Task<ResultModel<CategoryDto>> CreateAsync(CategoryUpsertDto dto)
    {
        var category = new Category { Name = dto.Name.Trim(), Description = dto.Description };
        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return ResultModel<CategoryDto>.Ok(new CategoryDto { Id = category.Id, Name = category.Name, Description = category.Description }, "Category created.");
    }

    public async Task<ResultModel<CategoryDto>> UpdateAsync(int id, CategoryUpsertDto dto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
        {
            return ResultModel<CategoryDto>.Fail("Category not found.");
        }

        category.Name = dto.Name.Trim();
        category.Description = dto.Description;
        category.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return ResultModel<CategoryDto>.Ok(new CategoryDto { Id = category.Id, Name = category.Name, Description = category.Description }, "Category updated.");
    }

    public async Task<ResultModel<bool>> DeleteAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
        {
            return ResultModel<bool>.Fail("Category not found.");
        }

        var hasProducts = await _unitOfWork.Products.Query().AnyAsync(p => p.CategoryId == id);
        if (hasProducts)
        {
            return ResultModel<bool>.Fail("Cannot delete a category that has products.");
        }

        category.IsDeleted = true;
        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return ResultModel<bool>.Ok(true, "Category deleted.");
    }
}
