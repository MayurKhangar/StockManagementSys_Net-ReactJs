using Microsoft.EntityFrameworkCore;
using SmartStock.Application.Common;
using SmartStock.Application.DTOs.Order;
using SmartStock.Application.Interfaces;
using SmartStock.Domain.Entities;
using SmartStock.Domain.Enums;
using SmartStock.Shared.Constants;
using SmartStock.Shared.Helpers;

namespace SmartStock.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ResultModel<OrderDto>> PlaceOrderAsync(PlaceOrderRequestDto dto, int customerId)
    {
        if (dto.Items == null || dto.Items.Count == 0)
        {
            return ResultModel<OrderDto>.Fail("Cart is empty.");
        }

        var mergedItems = dto.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new CartItemDto { ProductId = g.Key, Quantity = g.Sum(i => i.Quantity) })
            .ToList();

        if (mergedItems.Any(i => i.Quantity <= 0))
        {
            return ResultModel<OrderDto>.Fail("Quantity must be greater than zero for all items.");
        }

        await using var scope = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var orderItems = new List<OrderItem>();
            var stockTransactions = new List<StockTransaction>();
            decimal subTotal = 0;

            foreach (var item in mergedItems)
            {
                var product = await _unitOfWork.Products.Query()
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId && p.IsActive);

                if (product == null)
                {
                    await scope.RollbackAsync();
                    return ResultModel<OrderDto>.Fail($"Product with id {item.ProductId} is not available.");
                }

                var stockBefore = product.StockQuantity;
                var affected = await _unitOfWork.TryDeductStockAsync(product.Id, item.Quantity);
                if (affected == 0)
                {
                    await scope.RollbackAsync();
                    return ResultModel<OrderDto>.Fail($"Insufficient stock for '{product.Name}'. Please reduce the quantity.");
                }

                var lineTotal = product.Price * item.Quantity;
                subTotal += lineTotal;

                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductNameSnapshot = product.Name,
                    UnitPrice = product.Price,
                    Quantity = item.Quantity,
                    LineTotal = lineTotal
                });

                stockTransactions.Add(new StockTransaction
                {
                    ProductId = product.Id,
                    Type = StockTransactionType.Out,
                    Quantity = item.Quantity,
                    StockBeforeTransaction = stockBefore,
                    StockAfterTransaction = stockBefore - item.Quantity,
                    Reason = "Customer order",
                    PerformedByUserId = customerId
                });
            }

            var discount = Math.Min(dto.DiscountAmount, subTotal);
            var taxable = subTotal - discount;
            var tax = Math.Round(taxable * AppConstants.DefaultTaxRate, 2);
            var total = taxable + tax;

            var year = DateTime.UtcNow.Year;
            var orderSequence = await _unitOfWork.Orders.Query().CountAsync(o => o.CreatedAt.Year == year) + 1;
            var orderNumber = NumberSeriesHelper.Generate(AppConstants.Series.OrderPrefix, year, orderSequence);

            var order = new Order
            {
                OrderNumber = orderNumber,
                CustomerId = customerId,
                Status = OrderStatus.Confirmed,
                SubTotal = subTotal,
                DiscountAmount = discount,
                TaxAmount = tax,
                TotalAmount = total,
                OrderItems = orderItems
            };

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            foreach (var st in stockTransactions)
            {
                await _unitOfWork.StockTransactions.AddAsync(st);
            }
            await _unitOfWork.SaveChangesAsync();

            var invoiceSequence = await _unitOfWork.Invoices.Query().CountAsync(i => i.CreatedAt.Year == year) + 1;
            var invoiceNumber = NumberSeriesHelper.Generate(AppConstants.Series.InvoicePrefix, year, invoiceSequence);

            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                OrderId = order.Id,
                IssueDate = DateTime.UtcNow,
                SubTotal = order.SubTotal,
                DiscountAmount = order.DiscountAmount,
                TaxAmount = order.TaxAmount,
                TotalAmount = order.TotalAmount,
                InvoiceItems = orderItems.Select(oi => new InvoiceItem
                {
                    ProductId = oi.ProductId,
                    ProductNameSnapshot = oi.ProductNameSnapshot,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity,
                    LineTotal = oi.LineTotal
                }).ToList()
            };

            await _unitOfWork.Invoices.AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync();
            await scope.CommitAsync();

            return await GetByIdAsync(order.Id, customerId, false);
        }
        catch
        {
            await scope.RollbackAsync();
            throw;
        }
    }

    public async Task<ResultModel<OrderDto>> GetByIdAsync(int id, int? requestingUserId, bool isAdmin)
    {
        var order = await _unitOfWork.Orders.Query()
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .Include(o => o.Invoice)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return ResultModel<OrderDto>.Fail("Order not found.");
        }

        if (!isAdmin && order.CustomerId != requestingUserId)
        {
            return ResultModel<OrderDto>.Fail("You do not have access to this order.");
        }

        return ResultModel<OrderDto>.Ok(MapToDto(order));
    }

    public async Task<ResultModel<List<OrderDto>>> GetMyOrdersAsync(int customerId)
    {
        var orders = await _unitOfWork.Orders.Query()
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .Include(o => o.Invoice)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return ResultModel<List<OrderDto>>.Ok(orders.Select(MapToDto).ToList());
    }

    public async Task<ResultModel<List<OrderDto>>> GetAllAsync()
    {
        var orders = await _unitOfWork.Orders.Query()
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .Include(o => o.Invoice)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return ResultModel<List<OrderDto>>.Ok(orders.Select(MapToDto).ToList());
    }

    public async Task<ResultModel<OrderDto>> CancelOrderAsync(int id, int? requestingUserId, bool isAdmin)
    {
        await using var scope = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var order = await _unitOfWork.Orders.Query()
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                await scope.RollbackAsync();
                return ResultModel<OrderDto>.Fail("Order not found.");
            }

            if (!isAdmin && order.CustomerId != requestingUserId)
            {
                await scope.RollbackAsync();
                return ResultModel<OrderDto>.Fail("You do not have access to this order.");
            }

            if (order.Status != OrderStatus.Confirmed && order.Status != OrderStatus.Pending)
            {
                await scope.RollbackAsync();
                return ResultModel<OrderDto>.Fail("Only pending or confirmed orders can be cancelled.");
            }

            foreach (var item in order.OrderItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    var before = product.StockQuantity;
                    product.StockQuantity += item.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Products.Update(product);

                    await _unitOfWork.StockTransactions.AddAsync(new StockTransaction
                    {
                        ProductId = product.Id,
                        Type = StockTransactionType.In,
                        Quantity = item.Quantity,
                        StockBeforeTransaction = before,
                        StockAfterTransaction = product.StockQuantity,
                        Reason = $"Order {order.OrderNumber} cancelled",
                        PerformedByUserId = requestingUserId ?? order.CustomerId
                    });
                }
            }

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();
            await scope.CommitAsync();

            return await GetByIdAsync(id, requestingUserId, isAdmin);
        }
        catch
        {
            await scope.RollbackAsync();
            throw;
        }
    }

    private static OrderDto MapToDto(Order o) => new()
    {
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        CustomerId = o.CustomerId,
        CustomerName = o.Customer?.FullName ?? string.Empty,
        Status = o.Status.ToString(),
        SubTotal = o.SubTotal,
        DiscountAmount = o.DiscountAmount,
        TaxAmount = o.TaxAmount,
        TotalAmount = o.TotalAmount,
        CreatedAt = o.CreatedAt,
        Items = o.OrderItems.Select(i => new OrderItemDto
        {
            ProductId = i.ProductId,
            ProductName = i.ProductNameSnapshot,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity,
            LineTotal = i.LineTotal
        }).ToList(),
        InvoiceId = o.Invoice?.Id,
        InvoiceNumber = o.Invoice?.InvoiceNumber
    };
}
