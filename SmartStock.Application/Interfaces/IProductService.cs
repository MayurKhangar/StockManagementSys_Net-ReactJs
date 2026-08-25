using SmartStock.Application.Common;
using SmartStock.Application.DTOs.Product;

namespace SmartStock.Application.Interfaces;

public interface IProductService
{
    Task<ResultModel<PagedResult<ProductDto>>> GetAllAsync(ProductFilterDto filter);
    Task<ResultModel<ProductDto>> GetByIdAsync(int id);
    Task<ResultModel<ProductDto>> CreateAsync(ProductUpsertDto dto);
    Task<ResultModel<ProductDto>> UpdateAsync(int id, ProductUpsertDto dto);
    Task<ResultModel<bool>> DeleteAsync(int id);
}
