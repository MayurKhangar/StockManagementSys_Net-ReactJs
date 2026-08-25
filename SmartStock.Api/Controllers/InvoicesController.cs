using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Application.Interfaces;

namespace SmartStock.Api.Controllers;

[Authorize]
public class InvoicesController : BaseApiController
{
    private readonly IInvoiceService _invoiceService;
    private readonly IOrderService _orderService;

    public InvoicesController(IInvoiceService invoiceService, IOrderService orderService)
    {
        _invoiceService = invoiceService;
        _orderService = orderService;
    }

    [Authorize(Policy = "AdminOrManager")]
    [HttpGet]
    public async Task<IActionResult> GetAll() => FromResult(await _invoiceService.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _invoiceService.GetByIdAsync(id);
        if (!result.Success || result.Data == null)
        {
            return FromResult(result);
        }

        if (!IsAdmin)
        {
            var orderCheck = await _orderService.GetByIdAsync(result.Data.OrderId, CurrentUserId, false);
            if (!orderCheck.Success)
            {
                return Forbid();
            }
        }

        return Ok(result);
    }

    [HttpGet("order/{orderId:int}")]
    public async Task<IActionResult> GetByOrderId(int orderId)
    {
        var orderCheck = await _orderService.GetByIdAsync(orderId, CurrentUserId, IsAdmin);
        if (!orderCheck.Success)
        {
            return FromResult(orderCheck);
        }

        return FromResult(await _invoiceService.GetByOrderIdAsync(orderId));
    }

    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> DownloadPdf(int id)
    {
        var invoiceResult = await _invoiceService.GetByIdAsync(id);
        if (!invoiceResult.Success || invoiceResult.Data == null)
        {
            return NotFound(invoiceResult);
        }

        if (!IsAdmin)
        {
            var orderCheck = await _orderService.GetByIdAsync(invoiceResult.Data.OrderId, CurrentUserId, false);
            if (!orderCheck.Success)
            {
                return Forbid();
            }
        }

        var pdfResult = await _invoiceService.GeneratePdfAsync(id);
        if (!pdfResult.Success || pdfResult.Data == null)
        {
            return BadRequest(pdfResult);
        }

        return File(pdfResult.Data, "application/pdf", $"{invoiceResult.Data.InvoiceNumber}.pdf");
    }
}
