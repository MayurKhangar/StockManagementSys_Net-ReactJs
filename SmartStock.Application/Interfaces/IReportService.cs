using SmartStock.Application.Common;
using SmartStock.Application.DTOs.Report;

namespace SmartStock.Application.Interfaces;

public interface IReportService
{
    Task<ResultModel<DashboardSummaryDto>> GetDashboardSummaryAsync();
    Task<ResultModel<List<StockValuationDto>>> GetStockValuationAsync();
    Task<ResultModel<List<TopProductDto>>> GetTopProductsAsync(int count);
    Task<ResultModel<List<SalesTrendPointDto>>> GetSalesTrendAsync(int days);
}
