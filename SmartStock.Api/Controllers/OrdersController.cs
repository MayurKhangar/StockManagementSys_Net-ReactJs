using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Application.DTOs.Order;
using SmartStock.Application.Interfaces;

namespace SmartStock.Api.Controllers;

[Authorize]
public class OrdersController : BaseApiController
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [Authorize(Policy = "CustomerOnly")]
    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequestDto dto) => FromResult(await _orderService.PlaceOrderAsync(dto, CurrentUserId));

    [Authorize(Policy = "CustomerOnly")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders() => FromResult(await _orderService.GetMyOrdersAsync(CurrentUserId));

    [Authorize(Policy = "AdminOrManager")]
    [HttpGet]
    public async Task<IActionResult> GetAll() => FromResult(await _orderService.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) => FromResult(await _orderService.GetByIdAsync(id, CurrentUserId, IsAdmin));

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id) => FromResult(await _orderService.CancelOrderAsync(id, CurrentUserId, IsAdmin));
}
