using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Application.DTOs.Stock;
using SmartStock.Application.Interfaces;

namespace SmartStock.Api.Controllers;

[Authorize(Policy = "AdminOrManager")]
public class StockController : BaseApiController
{
    private readonly IStockService _stockService;

    public StockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    [HttpPost("in")]
    public async Task<IActionResult> StockIn([FromBody] StockInRequestDto dto) => FromResult(await _stockService.StockInAsync(dto, CurrentUserId));

    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust([FromBody] StockAdjustmentRequestDto dto) => FromResult(await _stockService.AdjustStockAsync(dto, CurrentUserId));

    [HttpGet("ledger")]
    public async Task<IActionResult> GetLedger([FromQuery] int? productId) => FromResult(await _stockService.GetLedgerAsync(productId));

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock() => FromResult(await _stockService.GetLowStockProductsAsync());
}
