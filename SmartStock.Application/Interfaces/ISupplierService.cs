using SmartStock.Application.Common;
using SmartStock.Application.DTOs.Supplier;

namespace SmartStock.Application.Interfaces;

public interface ISupplierService
{
    Task<ResultModel<List<SupplierDto>>> GetAllAsync();
    Task<ResultModel<SupplierDto>> GetByIdAsync(int id);
    Task<ResultModel<SupplierDto>> CreateAsync(SupplierUpsertDto dto);
    Task<ResultModel<SupplierDto>> UpdateAsync(int id, SupplierUpsertDto dto);
    Task<ResultModel<bool>> DeleteAsync(int id);
}
