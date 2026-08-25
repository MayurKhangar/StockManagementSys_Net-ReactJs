namespace SmartStock.Domain.Enums;

public enum RoleType
{
    Admin = 1,
    StoreManager = 2,
    Customer = 3
}

public enum StockTransactionType
{
    In = 1,
    Out = 2,
    Adjustment = 3
}

public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    Cancelled = 3,
    Completed = 4
}

public enum AuditAction
{
    Create = 1,
    Update = 2,
    Delete = 3,
    Login = 4,
    StockChange = 5
}
