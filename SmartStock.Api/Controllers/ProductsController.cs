using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Application.DTOs.Product;
using SmartStock.Application.Interfaces;

namespace SmartStock.Api.Controllers;

public class ProductsController : BaseApiController
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProductFilterDto filter) => FromResult(await _productService.GetAllAsync(filter));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) => FromResult(await _productService.GetByIdAsync(id));

    [Authorize(Policy = "AdminOrManager")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductUpsertDto dto) => FromResult(await _productService.CreateAsync(dto));

    [Authorize(Policy = "AdminOrManager")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductUpsertDto dto) => FromResult(await _productService.UpdateAsync(id, dto));

    [Authorize(Policy = "AdminOrManager")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) => FromResult(await _productService.DeleteAsync(id));
}
