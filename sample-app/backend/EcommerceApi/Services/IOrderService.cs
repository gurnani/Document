using EcommerceApi.DTOs;

namespace EcommerceApi.Services;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(string userId, CreateOrderDto createOrderDto);
    Task<PagedResultDto<OrderDto>> GetUserOrdersAsync(string userId, int page = 1, int pageSize = 10);
    Task<OrderDto> GetOrderByIdAsync(string userId, int orderId);
    Task<OrderDto> CancelOrderAsync(string userId, int orderId);
    Task<OrderDto> ReorderAsync(string userId, int orderId);
    
    Task<PagedResultDto<OrderDto>> GetAllOrdersAsync(int page = 1, int pageSize = 20);
    Task<OrderDto> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto updateStatusDto);
    Task<OrderDto> AddTrackingNumberAsync(int orderId, AddTrackingNumberDto addTrackingDto);
}
