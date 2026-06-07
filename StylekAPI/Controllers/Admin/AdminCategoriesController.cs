using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StylekAPI.DTOs.Admin;
using StylekAPI.Helpers;
using StylekAPI.Services;

namespace StylekAPI.Controllers.Admin;

[Authorize(Roles = AppRoles.AdminOrManager)]
[ApiController]
[Route("api/admin/categories")]
public class AdminCategoriesController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public AdminCategoriesController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AdminCategoryDto>>>> GetAll([FromQuery] bool includeInactive = true)
    {
        var categories = await _categoryService.GetAllAdminAsync(includeInactive);
        return Ok(ApiResponse<List<AdminCategoryDto>>.Ok(categories));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<AdminCategoryDto>>> GetById(int id)
    {
        var category = await _categoryService.GetAdminByIdAsync(id);
        return Ok(ApiResponse<AdminCategoryDto>.Ok(category));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AdminCategoryDto>>> Create(CreateCategoryDto dto)
    {
        var category = await _categoryService.CreateAsync(dto);
        return Ok(ApiResponse<AdminCategoryDto>.Ok(category, "Category created successfully"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<AdminCategoryDto>>> Update(int id, UpdateCategoryDto dto)
    {
        var category = await _categoryService.UpdateAsync(id, dto);
        return Ok(ApiResponse<AdminCategoryDto>.Ok(category, "Category updated successfully"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> SoftDelete(int id)
    {
        await _categoryService.SoftDeleteAsync(id);
        return Ok(ApiResponse.Ok("Category deactivated successfully"));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("{id:int}/permanent")]
    public async Task<ActionResult<ApiResponse>> HardDelete(int id)
    {
        await _categoryService.HardDeleteAsync(id);
        return Ok(ApiResponse.Ok("Category permanently deleted"));
    }
}
