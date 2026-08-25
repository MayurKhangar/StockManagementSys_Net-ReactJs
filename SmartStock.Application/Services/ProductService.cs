using Microsoft.EntityFrameworkCore;
using SmartStock.Application.Common;
using SmartStock.Application.DTOs.Product;
using SmartStock.Application.Interfaces;
using SmartStock.Domain.Entities;

namespace SmartStock.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ResultModel<PagedResult<ProductDto>>> GetAllAsync(ProductFilterDto filter)
    {
        var query = _unitOfWork.Products.Query().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(search) || p.Sku.ToLower().Contains(search));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
        }

        if (filter.SupplierId.HasValue)
        {
            query = query.Where(p => p.SupplierId == filter.SupplierId.Value);
        }

        if (filter.LowStockOnly == true)
        {
            query = query.Where(p => p.StockQuantity <= p.LowStockThreshold);
        }

        if (filter.ActiveOnly == true)
        {
            query = query.Where(p => p.IsActive);
        }

        var totalCount = await query.CountAsync();

        var pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize <= 0 ? 20 : filter.PageSize;

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Sku = p.Sku,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                CostPrice = p.CostPrice,
                StockQuantity = p.StockQuantity,
                LowStockThreshold = p.LowStockThreshold,
                IsLowStock = p.StockQuantity <= p.LowStockThreshold,
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier.Name
            })
            .ToListAsync();

        return ResultModel<PagedResult<ProductDto>>.Ok(new PagedResult<ProductDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    public async Task<ResultModel<ProductDto>> GetByIdAsync(int id)
    {
        var product = await _unitOfWork.Products.Query()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return ResultModel<ProductDto>.Fail("Product not found.");
        }

        return ResultModel<ProductDto>.Ok(MapToDto(product));
    }

    public async Task<ResultModel<ProductDto>> CreateAsync(ProductUpsertDto dto)
    {
        var skuExists = await _unitOfWork.Products.Query().AnyAsync(p => p.Sku == dto.Sku);
        if (skuExists)
        {
            return ResultModel<ProductDto>.Fail("A product with this SKU already exists.");
        }

        var categoryExists = await _unitOfWork.Categories.Query().AnyAsync(c => c.Id == dto.CategoryId);
        var supplierExists = await _unitOfWork.Suppliers.Query().AnyAsync(s => s.Id == dto.SupplierId);
        if (!categoryExists || !supplierExists)
        {
            return ResultModel<ProductDto>.Fail("Invalid category or supplier.");
        }

        var product = new Product
        {
            Sku = dto.Sku.Trim(),
            Name = dto.Name.Trim(),
            Description = dto.Description,
            Price = dto.Price,
            CostPrice = dto.CostPrice,
            StockQuantity = dto.StockQuantity,
            LowStockThreshold = dto.LowStockThreshold,
            ImageUrl = dto.ImageUrl,
            IsActive = dto.IsActive,
            CategoryId = dto.CategoryId,
            SupplierId = dto.SupplierId
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(product.Id);
    }

    public async Task<ResultModel<ProductDto>> UpdateAsync(int id, ProductUpsertDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
        {
            return ResultModel<ProductDto>.Fail("Product not found.");
        }

        var skuTaken = await _unitOfWork.Products.Query().AnyAsync(p => p.Sku == dto.Sku && p.Id != id);
        if (skuTaken)
        {
            return ResultModel<ProductDto>.Fail("A product with this SKU already exists.");
        }

        product.Sku = dto.Sku.Trim();
        product.Name = dto.Name.Trim();
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.CostPrice = dto.CostPrice;
        product.LowStockThreshold = dto.LowStockThreshold;
        product.ImageUrl = dto.ImageUrl;
        product.IsActive = dto.IsActive;
        product.CategoryId = dto.CategoryId;
        product.SupplierId = dto.SupplierId;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(product.Id);
    }

    public async Task<ResultModel<bool>> DeleteAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
        {
            return ResultModel<bool>.Fail("Product not found.");
        }

        product.IsDeleted = true;
        product.IsActive = false;
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        return ResultModel<bool>.Ok(true, "Product deleted.");
    }

    private static ProductDto MapToDto(Product p) => new()
    {
        Id = p.Id,
        Sku = p.Sku,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        CostPrice = p.CostPrice,
        StockQuantity = p.StockQuantity,
        LowStockThreshold = p.LowStockThreshold,
        IsLowStock = p.StockQuantity <= p.LowStockThreshold,
        ImageUrl = p.ImageUrl,
        IsActive = p.IsActive,
        CategoryId = p.CategoryId,
        CategoryName = p.Category != null ? p.Category.Name : string.Empty,
        SupplierId = p.SupplierId,
        SupplierName = p.Supplier != null ? p.Supplier.Name : string.Empty
    };
}
