using Microsoft.EntityFrameworkCore;
using SmartStock.Application.Common;
using SmartStock.Application.DTOs.Stock;
using SmartStock.Application.Interfaces;
using SmartStock.Domain.Entities;
using SmartStock.Domain.Enums;

namespace SmartStock.Application.Services;

public class StockService : IStockService
{
    private readonly IUnitOfWork _unitOfWork;

    public StockService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ResultModel<StockTransactionDto>> StockInAsync(StockInRequestDto dto, int performedByUserId)
    {
        if (dto.Quantity <= 0)
        {
            return ResultModel<StockTransactionDto>.Fail("Quantity must be greater than zero.");
        }

        await using var scope = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null)
            {
                return ResultModel<StockTransactionDto>.Fail("Product not found.");
            }

            var before = product.StockQuantity;
            product.StockQuantity += dto.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Products.Update(product);

            var transaction = new StockTransaction
            {
                ProductId = product.Id,
                Type = StockTransactionType.In,
                Quantity = dto.Quantity,
                StockBeforeTransaction = before,
                StockAfterTransaction = product.StockQuantity,
                Reason = dto.Reason,
                ReferenceNumber = dto.ReferenceNumber,
                PerformedByUserId = performedByUserId
            };
            await _unitOfWork.StockTransactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();
            await scope.CommitAsync();

            return ResultModel<StockTransactionDto>.Ok(await MapToDtoAsync(transaction.Id), "Stock added.");
        }
        catch
        {
            await scope.RollbackAsync();
            throw;
        }
    }

    public async Task<ResultModel<StockTransactionDto>> AdjustStockAsync(StockAdjustmentRequestDto dto, int performedByUserId)
    {
        if (dto.NewQuantity < 0)
        {
            return ResultModel<StockTransactionDto>.Fail("Quantity cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(dto.Reason))
        {
            return ResultModel<StockTransactionDto>.Fail("A reason is required for stock adjustments.");
        }

        await using var scope = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null)
            {
                return ResultModel<StockTransactionDto>.Fail("Product not found.");
            }

            var before = product.StockQuantity;
            var delta = dto.NewQuantity - before;
            product.StockQuantity = dto.NewQuantity;
            product.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Products.Update(product);

            var transaction = new StockTransaction
            {
                ProductId = product.Id,
                Type = StockTransactionType.Adjustment,
                Quantity = delta,
                StockBeforeTransaction = before,
                StockAfterTransaction = product.StockQuantity,
                Reason = dto.Reason,
                PerformedByUserId = performedByUserId
            };
            await _unitOfWork.StockTransactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();
            await scope.CommitAsync();

            return ResultModel<StockTransactionDto>.Ok(await MapToDtoAsync(transaction.Id), "Stock adjusted.");
        }
        catch
        {
            await scope.RollbackAsync();
            throw;
        }
    }

    public async Task<ResultModel<List<StockTransactionDto>>> GetLedgerAsync(int? productId)
    {
        var query = _unitOfWork.StockTransactions.Query()
            .Include(t => t.Product)
            .Include(t => t.PerformedByUser)
            .AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(t => t.ProductId == productId.Value);
        }

        var results = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new StockTransactionDto
            {
                Id = t.Id,
                ProductId = t.ProductId,
                ProductName = t.Product.Name,
                Type = t.Type.ToString(),
                Quantity = t.Quantity,
                StockBeforeTransaction = t.StockBeforeTransaction,
                StockAfterTransaction = t.StockAfterTransaction,
                Reason = t.Reason,
                ReferenceNumber = t.ReferenceNumber,
                PerformedBy = t.PerformedByUser.FullName,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        return ResultModel<List<StockTransactionDto>>.Ok(results);
    }

    public async Task<ResultModel<List<LowStockProductDto>>> GetLowStockProductsAsync()
    {
        var products = await _unitOfWork.Products.Query()
            .Where(p => p.IsActive && p.StockQuantity <= p.LowStockThreshold)
            .OrderBy(p => p.StockQuantity)
            .Select(p => new LowStockProductDto
            {
                ProductId = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                StockQuantity = p.StockQuantity,
                LowStockThreshold = p.LowStockThreshold
            })
            .ToListAsync();

        return ResultModel<List<LowStockProductDto>>.Ok(products);
    }

    private async Task<StockTransactionDto> MapToDtoAsync(int transactionId)
    {
        var t = await _unitOfWork.StockTransactions.Query()
            .Include(x => x.Product)
            .Include(x => x.PerformedByUser)
            .FirstAsync(x => x.Id == transactionId);

        return new StockTransactionDto
        {
            Id = t.Id,
            ProductId = t.ProductId,
            ProductName = t.Product.Name,
            Type = t.Type.ToString(),
            Quantity = t.Quantity,
            StockBeforeTransaction = t.StockBeforeTransaction,
            StockAfterTransaction = t.StockAfterTransaction,
            Reason = t.Reason,
            ReferenceNumber = t.ReferenceNumber,
            PerformedBy = t.PerformedByUser.FullName,
            CreatedAt = t.CreatedAt
        };
    }
}
