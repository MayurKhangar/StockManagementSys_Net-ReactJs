using Microsoft.EntityFrameworkCore;
using SmartStock.Application.Common;
using SmartStock.Application.DTOs.Supplier;
using SmartStock.Application.Interfaces;
using SmartStock.Domain.Entities;

namespace SmartStock.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _unitOfWork;

    public SupplierService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ResultModel<List<SupplierDto>>> GetAllAsync()
    {
        var suppliers = await _unitOfWork.Suppliers.Query()
            .Select(s => new SupplierDto
            {
                Id = s.Id,
                Name = s.Name,
                ContactPerson = s.ContactPerson,
                Email = s.Email,
                PhoneNumber = s.PhoneNumber,
                Address = s.Address
            })
            .OrderBy(s => s.Name)
            .ToListAsync();

        return ResultModel<List<SupplierDto>>.Ok(suppliers);
    }

    public async Task<ResultModel<SupplierDto>> GetByIdAsync(int id)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
        if (supplier == null)
        {
            return ResultModel<SupplierDto>.Fail("Supplier not found.");
        }

        return ResultModel<SupplierDto>.Ok(MapToDto(supplier));
    }

    public async Task<ResultModel<SupplierDto>> CreateAsync(SupplierUpsertDto dto)
    {
        var supplier = new Supplier
        {
            Name = dto.Name.Trim(),
            ContactPerson = dto.ContactPerson,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address
        };
        await _unitOfWork.Suppliers.AddAsync(supplier);
        await _unitOfWork.SaveChangesAsync();

        return ResultModel<SupplierDto>.Ok(MapToDto(supplier), "Supplier created.");
    }

    public async Task<ResultModel<SupplierDto>> UpdateAsync(int id, SupplierUpsertDto dto)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
        if (supplier == null)
        {
            return ResultModel<SupplierDto>.Fail("Supplier not found.");
        }

        supplier.Name = dto.Name.Trim();
        supplier.ContactPerson = dto.ContactPerson;
        supplier.Email = dto.Email;
        supplier.PhoneNumber = dto.PhoneNumber;
        supplier.Address = dto.Address;
        supplier.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Suppliers.Update(supplier);
        await _unitOfWork.SaveChangesAsync();

        return ResultModel<SupplierDto>.Ok(MapToDto(supplier), "Supplier updated.");
    }

    public async Task<ResultModel<bool>> DeleteAsync(int id)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
        if (supplier == null)
        {
            return ResultModel<bool>.Fail("Supplier not found.");
        }

        var hasProducts = await _unitOfWork.Products.Query().AnyAsync(p => p.SupplierId == id);
        if (hasProducts)
        {
            return ResultModel<bool>.Fail("Cannot delete a supplier that has products.");
        }

        supplier.IsDeleted = true;
        _unitOfWork.Suppliers.Update(supplier);
        await _unitOfWork.SaveChangesAsync();

        return ResultModel<bool>.Ok(true, "Supplier deleted.");
    }

    private static SupplierDto MapToDto(Supplier s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        ContactPerson = s.ContactPerson,
        Email = s.Email,
        PhoneNumber = s.PhoneNumber,
        Address = s.Address
    };
}
