using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StylekAPI.DTOs.Orders;
using StylekAPI.Helpers;
using StylekAPI.Services;

namespace StylekAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDetailDto>>> CreateOrder(CreateOrderDto dto)
    {
        var order = await _orderService.CreateOrderAsync(User.GetUserId(), dto);
        return Ok(ApiResponse<OrderDetailDto>.Ok(order, "Order created successfully"));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetMyOrders()
    {
        var orders = await _orderService.GetMyOrdersAsync(User.GetUserId());
        return Ok(ApiResponse<List<OrderDto>>.Ok(orders));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<OrderDetailDto>>> GetOrderDetails(int id)
    {
        var order = await _orderService.GetOrderDetailAsync(User.GetUserId(), id);
        return Ok(ApiResponse<OrderDetailDto>.Ok(order));
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<ApiResponse>> CancelOrder(int id)
    {
        await _orderService.CancelOrderAsync(User.GetUserId(), id);
        return Ok(ApiResponse.Ok("Order cancelled successfully"));
    }

    [HttpGet("{id:int}/track")]
    public async Task<ActionResult<ApiResponse<TrackOrderDto>>> TrackOrder(int id)
    {
        var track = await _orderService.TrackOrderAsync(User.GetUserId(), id);
        return Ok(ApiResponse<TrackOrderDto>.Ok(track));
    }
}
