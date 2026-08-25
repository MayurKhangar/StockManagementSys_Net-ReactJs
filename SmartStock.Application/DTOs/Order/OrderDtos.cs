namespace SmartStock.Application.DTOs.Order;

public class CartItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class PlaceOrderRequestDto
{
    public List<CartItemDto> Items { get; set; } = new();
    public decimal DiscountAmount { get; set; }
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public int? InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
}
