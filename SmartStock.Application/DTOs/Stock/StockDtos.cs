namespace SmartStock.Application.DTOs.Stock;

public class StockInRequestDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceNumber { get; set; }
}

public class StockAdjustmentRequestDto
{
    public int ProductId { get; set; }
    public int NewQuantity { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class StockTransactionDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int StockBeforeTransaction { get; set; }
    public int StockAfterTransaction { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceNumber { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class LowStockProductDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; }
}
