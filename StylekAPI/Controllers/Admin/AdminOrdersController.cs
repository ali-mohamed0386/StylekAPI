using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StylekAPI.DTOs.Admin;
using StylekAPI.DTOs.Orders;
using StylekAPI.Helpers;
using StylekAPI.Services;

namespace StylekAPI.Controllers.Admin;

[Authorize(Roles = AppRoles.AdminOrManager)]
[ApiController]
[Route("api/admin/orders")]
public class AdminOrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public AdminOrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminOrderDto>>>> GetAll([FromQuery] AdminListFilterDto filter)
    {
        var result = await _orderService.GetAllAdminAsync(filter);
        return Ok(ApiResponse<PagedResult<AdminOrderDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<OrderDetailDto>>> GetById(int id)
    {
        var order = await _orderService.GetAdminOrderDetailAsync(id);
        return Ok(ApiResponse<OrderDetailDto>.Ok(order));
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<ApiResponse<OrderDetailDto>>> UpdateStatus(int id, UpdateOrderStatusDto dto)
    {
        var order = await _orderService.UpdateStatusAsync(id, dto);
        return Ok(ApiResponse<OrderDetailDto>.Ok(order, "Order status updated"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> SoftDelete(int id)
    {
        await _orderService.SoftDeleteAsync(id);
        return Ok(ApiResponse.Ok("Order deactivated successfully"));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("{id:int}/permanent")]
    public async Task<ActionResult<ApiResponse>> HardDelete(int id)
    {
        await _orderService.HardDeleteAsync(id);
        return Ok(ApiResponse.Ok("Order permanently deleted"));
    }
}
