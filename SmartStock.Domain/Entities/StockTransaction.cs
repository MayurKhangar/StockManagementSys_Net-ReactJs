using SmartStock.Domain.Enums;

namespace SmartStock.Domain.Entities;

public class StockTransaction : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public StockTransactionType Type { get; set; }
    public int Quantity { get; set; }
    public int StockBeforeTransaction { get; set; }
    public int StockAfterTransaction { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceNumber { get; set; }

    public int PerformedByUserId { get; set; }
    public User PerformedByUser { get; set; } = null!;
}
