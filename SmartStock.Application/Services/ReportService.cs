using Microsoft.EntityFrameworkCore;
using SmartStock.Application.Common;
using SmartStock.Application.DTOs.Report;
using SmartStock.Application.Interfaces;
using SmartStock.Domain.Enums;

namespace SmartStock.Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ResultModel<DashboardSummaryDto>> GetDashboardSummaryAsync()
    {
        var salesSummary = await GetSalesSummaryInternalAsync();
        var stockValuation = await _unitOfWork.Products.Query()
            .Where(p => p.IsActive)
            .SumAsync(p => (decimal?)(p.CostPrice * p.StockQuantity)) ?? 0;

        var lowStockCount = await _unitOfWork.Products.Query()
            .CountAsync(p => p.IsActive && p.StockQuantity <= p.LowStockThreshold);

        var totalProducts = await _unitOfWork.Products.Query().CountAsync(p => p.IsActive);

        var trend = await GetSalesTrendInternalAsync(14);
        var topProducts = await GetTopProductsInternalAsync(5);

        return ResultModel<DashboardSummaryDto>.Ok(new DashboardSummaryDto
        {
            SalesSummary = salesSummary,
            TotalStockValuation = stockValuation,
            LowStockCount = lowStockCount,
            TotalProducts = totalProducts,
            SalesTrend = trend,
            TopProducts = topProducts
        });
    }

    public async Task<ResultModel<List<StockValuationDto>>> GetStockValuationAsync()
    {
        var results = await _unitOfWork.Products.Query()
            .Where(p => p.IsActive)
            .Select(p => new StockValuationDto
            {
                ProductId = p.Id,
                Name = p.Name,
                CategoryName = p.Category.Name,
                StockQuantity = p.StockQuantity,
                CostPrice = p.CostPrice,
                TotalValue = p.CostPrice * p.StockQuantity
            })
            .OrderByDescending(x => x.TotalValue)
            .ToListAsync();

        return ResultModel<List<StockValuationDto>>.Ok(results);
    }

    public async Task<ResultModel<List<TopProductDto>>> GetTopProductsAsync(int count)
    {
        return ResultModel<List<TopProductDto>>.Ok(await GetTopProductsInternalAsync(count));
    }

    public async Task<ResultModel<List<SalesTrendPointDto>>> GetSalesTrendAsync(int days)
    {
        return ResultModel<List<SalesTrendPointDto>>.Ok(await GetSalesTrendInternalAsync(days));
    }

    private async Task<SalesSummaryDto> GetSalesSummaryInternalAsync()
    {
        var confirmedOrders = _unitOfWork.Orders.Query()
            .Where(o => o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Completed);

        var totalRevenue = await confirmedOrders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
        var totalOrders = await confirmedOrders.CountAsync();
        var totalProductsSold = await _unitOfWork.OrderItems.Query()
            .Where(oi => oi.Order.Status == OrderStatus.Confirmed || oi.Order.Status == OrderStatus.Completed)
            .SumAsync(oi => (int?)oi.Quantity) ?? 0;

        return new SalesSummaryDto
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            AverageOrderValue = totalOrders == 0 ? 0 : Math.Round(totalRevenue / totalOrders, 2),
            TotalProductsSold = totalProductsSold
        };
    }

    private async Task<List<SalesTrendPointDto>> GetSalesTrendInternalAsync(int days)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-(days - 1));

        var raw = await _unitOfWork.Orders.Query()
            .Where(o => (o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Completed) && o.CreatedAt >= startDate)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Revenue = g.Sum(o => o.TotalAmount), Count = g.Count() })
            .ToListAsync();

        var result = new List<SalesTrendPointDto>();
        for (var d = startDate; d <= DateTime.UtcNow.Date; d = d.AddDays(1))
        {
            var match = raw.FirstOrDefault(r => r.Date == d);
            result.Add(new SalesTrendPointDto
            {
                Period = d.ToString("yyyy-MM-dd"),
                Revenue = match?.Revenue ?? 0,
                OrderCount = match?.Count ?? 0
            });
        }

        return result;
    }

    private async Task<List<TopProductDto>> GetTopProductsInternalAsync(int count)
    {
        return await _unitOfWork.OrderItems.Query()
            .Where(oi => oi.Order.Status == OrderStatus.Confirmed || oi.Order.Status == OrderStatus.Completed)
            .GroupBy(oi => new { oi.ProductId, oi.ProductNameSnapshot })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId,
                Name = g.Key.ProductNameSnapshot,
                QuantitySold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.LineTotal)
            })
            .OrderByDescending(x => x.QuantitySold)
            .Take(count)
            .ToListAsync();
    }
}
