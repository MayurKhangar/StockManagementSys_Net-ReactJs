using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Application.Interfaces;

namespace SmartStock.Api.Controllers;

[Authorize(Policy = "AdminOrManager")]
public class ReportsController : BaseApiController
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard() => FromResult(await _reportService.GetDashboardSummaryAsync());

    [HttpGet("stock-valuation")]
    public async Task<IActionResult> GetStockValuation() => FromResult(await _reportService.GetStockValuationAsync());

    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts([FromQuery] int count = 5) => FromResult(await _reportService.GetTopProductsAsync(count));

    [HttpGet("sales-trend")]
    public async Task<IActionResult> GetSalesTrend([FromQuery] int days = 14) => FromResult(await _reportService.GetSalesTrendAsync(days));
}
