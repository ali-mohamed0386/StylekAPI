using Microsoft.AspNetCore.Mvc;
using StylekAPI.DTOs.Products;
using StylekAPI.Helpers;
using StylekAPI.Services;

namespace StylekAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductListDto>>>> GetProducts([FromQuery] ProductFilterDto filter)
    {
        var result = await _productService.GetProductsAsync(filter);
        return Ok(ApiResponse<PagedResult<ProductListDto>>.Ok(result));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductListDto>>>> Search(
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var result = await _productService.SearchProductsAsync(q, page, pageSize);
        return Ok(ApiResponse<PagedResult<ProductListDto>>.Ok(result));
    }

    [HttpGet("featured")]
    public async Task<ActionResult<ApiResponse<List<ProductListDto>>>> GetFeatured([FromQuery] int count = 8)
    {
        var products = await _productService.GetFeaturedAsync(count);
        return Ok(ApiResponse<List<ProductListDto>>.Ok(products));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        return Ok(ApiResponse<ProductDetailDto>.Ok(product));
    }
}
