using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SmartStock.Application.Interfaces;
using SmartStock.Domain.Entities;
using SmartStock.Infrastructure.Data;

namespace SmartStock.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Users = new GenericRepository<User>(_context);
        Roles = new GenericRepository<Role>(_context);
        Categories = new GenericRepository<Category>(_context);
        Suppliers = new GenericRepository<Supplier>(_context);
        Products = new GenericRepository<Product>(_context);
        StockTransactions = new GenericRepository<StockTransaction>(_context);
        Orders = new GenericRepository<Order>(_context);
        OrderItems = new GenericRepository<OrderItem>(_context);
        Invoices = new GenericRepository<Invoice>(_context);
        InvoiceItems = new GenericRepository<InvoiceItem>(_context);
        AuditLogs = new GenericRepository<AuditLog>(_context);
    }

    public IGenericRepository<User> Users { get; }
    public IGenericRepository<Role> Roles { get; }
    public IGenericRepository<Category> Categories { get; }
    public IGenericRepository<Supplier> Suppliers { get; }
    public IGenericRepository<Product> Products { get; }
    public IGenericRepository<StockTransaction> StockTransactions { get; }
    public IGenericRepository<Order> Orders { get; }
    public IGenericRepository<OrderItem> OrderItems { get; }
    public IGenericRepository<Invoice> Invoices { get; }
    public IGenericRepository<InvoiceItem> InvoiceItems { get; }
    public IGenericRepository<AuditLog> AuditLogs { get; }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task<IDbContextTransactionScope> BeginTransactionAsync()
    {
        var transaction = await _context.Database.BeginTransactionAsync();
        return new DbContextTransactionScope(transaction);
    }

    public async Task<int> TryDeductStockAsync(int productId, int quantity)
    {
        return await _context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Products WITH (UPDLOCK, ROWLOCK) SET StockQuantity = StockQuantity - {quantity}, UpdatedAt = {DateTime.UtcNow} WHERE Id = {productId} AND StockQuantity >= {quantity} AND IsDeleted = 0");
    }
}

public class DbContextTransactionScope : IDbContextTransactionScope
{
    private readonly IDbContextTransaction _transaction;

    public DbContextTransactionScope(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public async Task CommitAsync() => await _transaction.CommitAsync();
    public async Task RollbackAsync() => await _transaction.RollbackAsync();

    public async ValueTask DisposeAsync() => await _transaction.DisposeAsync();
}
