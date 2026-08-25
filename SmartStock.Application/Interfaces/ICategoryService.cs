using SmartStock.Application.Common;
using SmartStock.Application.DTOs.Category;

namespace SmartStock.Application.Interfaces;

public interface ICategoryService
{
    Task<ResultModel<List<CategoryDto>>> GetAllAsync();
    Task<ResultModel<CategoryDto>> GetByIdAsync(int id);
    Task<ResultModel<CategoryDto>> CreateAsync(CategoryUpsertDto dto);
    Task<ResultModel<CategoryDto>> UpdateAsync(int id, CategoryUpsertDto dto);
    Task<ResultModel<bool>> DeleteAsync(int id);
}
