using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StylekAPI.DTOs.Admin;
using StylekAPI.Helpers;
using StylekAPI.Services;

namespace StylekAPI.Controllers.Admin;

[Authorize(Roles = AppRoles.AdminOrManager)]
[ApiController]
[Route("api/admin/products")]
public class AdminProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public AdminProductsController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminProductDto>>>> GetAll([FromQuery] AdminListFilterDto filter)
    {
        var result = await _productService.GetAllAdminAsync(filter);
        return Ok(ApiResponse<PagedResult<AdminProductDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<AdminProductDto>>> GetById(int id)
    {
        var product = await _productService.GetAdminByIdAsync(id);
        return Ok(ApiResponse<AdminProductDto>.Ok(product));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AdminProductDto>>> Create(CreateProductDto dto)
    {
        var product = await _productService.CreateAsync(dto);
        return Ok(ApiResponse<AdminProductDto>.Ok(product, "Product created successfully"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<AdminProductDto>>> Update(int id, UpdateProductDto dto)
    {
        var product = await _productService.UpdateAsync(id, dto);
        return Ok(ApiResponse<AdminProductDto>.Ok(product, "Product updated successfully"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> SoftDelete(int id)
    {
        await _productService.SoftDeleteAsync(id);
        return Ok(ApiResponse.Ok("Product deactivated successfully"));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("{id:int}/permanent")]
    public async Task<ActionResult<ApiResponse>> HardDelete(int id)
    {
        await _productService.HardDeleteAsync(id);
        return Ok(ApiResponse.Ok("Product permanently deleted"));
    }
}
