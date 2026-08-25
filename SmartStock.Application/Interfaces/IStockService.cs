using SmartStock.Application.Common;
using SmartStock.Application.DTOs.Stock;

namespace SmartStock.Application.Interfaces;

public interface IStockService
{
    Task<ResultModel<StockTransactionDto>> StockInAsync(StockInRequestDto dto, int performedByUserId);
    Task<ResultModel<StockTransactionDto>> AdjustStockAsync(StockAdjustmentRequestDto dto, int performedByUserId);
    Task<ResultModel<List<StockTransactionDto>>> GetLedgerAsync(int? productId);
    Task<ResultModel<List<LowStockProductDto>>> GetLowStockProductsAsync();
}
