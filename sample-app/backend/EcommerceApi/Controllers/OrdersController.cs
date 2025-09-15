using EcommerceApi.DTOs;
using EcommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var order = await _orderService.CreateOrderAsync(userId, createOrderDto);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("my-orders")]
    public async Task<ActionResult<PagedResultDto<OrderDto>>> GetMyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var orders = await _orderService.GetUserOrdersAsync(userId, page, pageSize);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var order = await _orderService.GetOrderByIdAsync(userId, id);
            return Ok(order);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<OrderDto>> CancelOrder(int id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var order = await _orderService.CancelOrderAsync(userId, id);
            return Ok(order);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/reorder")]
    public async Task<ActionResult<OrderDto>> Reorder(int id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var order = await _orderService.ReorderAsync(userId, id);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResultDto<OrderDto>>> GetAllOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var orders = await _orderService.GetAllOrdersAsync(page, pageSize);
        return Ok(orders);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OrderDto>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto updateStatusDto)
    {
        try
        {
            var order = await _orderService.UpdateOrderStatusAsync(id, updateStatusDto);
            return Ok(order);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/tracking")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OrderDto>> AddTrackingNumber(int id, [FromBody] AddTrackingNumberDto addTrackingDto)
    {
        try
        {
            var order = await _orderService.AddTrackingNumberAsync(id, addTrackingDto);
            return Ok(order);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
