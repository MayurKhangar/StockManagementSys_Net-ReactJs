using SmartStock.Domain.Entities;

namespace SmartStock.Application.Interfaces;

public interface IUnitOfWork
{
    IGenericRepository<User> Users { get; }
    IGenericRepository<Role> Roles { get; }
    IGenericRepository<Category> Categories { get; }
    IGenericRepository<Supplier> Suppliers { get; }
    IGenericRepository<Product> Products { get; }
    IGenericRepository<StockTransaction> StockTransactions { get; }
    IGenericRepository<Order> Orders { get; }
    IGenericRepository<OrderItem> OrderItems { get; }
    IGenericRepository<Invoice> Invoices { get; }
    IGenericRepository<InvoiceItem> InvoiceItems { get; }
    IGenericRepository<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync();
    Task<IDbContextTransactionScope> BeginTransactionAsync();

    /// <summary>
    /// Atomically decrements stock only if sufficient quantity is available, using a row-locking
    /// UPDATE so concurrent purchases of the same product cannot oversell. Returns rows affected
    /// (1 = success, 0 = insufficient stock or product not found).
    /// </summary>
    Task<int> TryDeductStockAsync(int productId, int quantity);
}

public interface IDbContextTransactionScope : IAsyncDisposable
{
    Task CommitAsync();
    Task RollbackAsync();
}
