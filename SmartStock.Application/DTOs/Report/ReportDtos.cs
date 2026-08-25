namespace SmartStock.Application.DTOs.Report;

public class SalesSummaryDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int TotalProductsSold { get; set; }
}

public class SalesTrendPointDto
{
    public string Period { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class TopProductDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

public class StockValuationDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal TotalValue { get; set; }
}

public class DashboardSummaryDto
{
    public SalesSummaryDto SalesSummary { get; set; } = new();
    public decimal TotalStockValuation { get; set; }
    public int LowStockCount { get; set; }
    public int TotalProducts { get; set; }
    public List<SalesTrendPointDto> SalesTrend { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();
}
