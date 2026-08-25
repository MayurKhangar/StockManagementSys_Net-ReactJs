using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Application.DTOs.Supplier;
using SmartStock.Application.Interfaces;

namespace SmartStock.Api.Controllers;

[Authorize(Policy = "AdminOrManager")]
public class SuppliersController : BaseApiController
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => FromResult(await _supplierService.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) => FromResult(await _supplierService.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SupplierUpsertDto dto) => FromResult(await _supplierService.CreateAsync(dto));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SupplierUpsertDto dto) => FromResult(await _supplierService.UpdateAsync(id, dto));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) => FromResult(await _supplierService.DeleteAsync(id));
}
