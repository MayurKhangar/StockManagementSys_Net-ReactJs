using SmartStock.Application.Common;
using SmartStock.Application.DTOs.Order;

namespace SmartStock.Application.Interfaces;

public interface IOrderService
{
    Task<ResultModel<OrderDto>> PlaceOrderAsync(PlaceOrderRequestDto dto, int customerId);
    Task<ResultModel<OrderDto>> GetByIdAsync(int id, int? requestingUserId, bool isAdmin);
    Task<ResultModel<List<OrderDto>>> GetMyOrdersAsync(int customerId);
    Task<ResultModel<List<OrderDto>>> GetAllAsync();
    Task<ResultModel<OrderDto>> CancelOrderAsync(int id, int? requestingUserId, bool isAdmin);
}
