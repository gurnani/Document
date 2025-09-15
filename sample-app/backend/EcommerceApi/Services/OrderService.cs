using EcommerceApi.Data;
using EcommerceApi.DTOs;
using EcommerceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto> CreateOrderAsync(string userId, CreateOrderDto createOrderDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            decimal subtotal = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in createOrderDto.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null || !product.IsActive)
                {
                    throw new NotFoundException($"Product with ID {item.ProductId} not found");
                }

                if (!product.InStock || product.StockQuantity < item.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock for product {product.Name}");
                }

                product.StockQuantity -= item.Quantity;
                if (product.StockQuantity == 0)
                {
                    product.InStock = false;
                }

                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                };

                orderItems.Add(orderItem);
                subtotal += item.Price * item.Quantity;
            }

            var tax = subtotal * 0.08m; // 8% tax
            var shipping = subtotal > 50 ? 0 : 9.99m; // Free shipping over $50
            var total = subtotal + tax + shipping;

            var order = new Order
            {
                UserId = userId,
                Status = OrderStatus.Pending,
                Subtotal = subtotal,
                Tax = tax,
                Shipping = shipping,
                Total = total,
                OrderNotes = createOrderDto.OrderNotes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,

                ShippingFirstName = createOrderDto.ShippingAddress.FirstName,
                ShippingLastName = createOrderDto.ShippingAddress.LastName,
                ShippingCompany = createOrderDto.ShippingAddress.Company,
                ShippingAddress1 = createOrderDto.ShippingAddress.Address1,
                ShippingAddress2 = createOrderDto.ShippingAddress.Address2,
                ShippingCity = createOrderDto.ShippingAddress.City,
                ShippingState = createOrderDto.ShippingAddress.State,
                ShippingZipCode = createOrderDto.ShippingAddress.ZipCode,
                ShippingCountry = createOrderDto.ShippingAddress.Country,
                ShippingPhone = createOrderDto.ShippingAddress.Phone,

                BillingFirstName = createOrderDto.BillingAddress.FirstName,
                BillingLastName = createOrderDto.BillingAddress.LastName,
                BillingCompany = createOrderDto.BillingAddress.Company,
                BillingAddress1 = createOrderDto.BillingAddress.Address1,
                BillingAddress2 = createOrderDto.BillingAddress.Address2,
                BillingCity = createOrderDto.BillingAddress.City,
                BillingState = createOrderDto.BillingAddress.State,
                BillingZipCode = createOrderDto.BillingAddress.ZipCode,
                BillingCountry = createOrderDto.BillingAddress.Country,

                PaymentType = createOrderDto.PaymentMethod.Type,
                PaymentReference = GeneratePaymentReference()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var orderItem in orderItems)
            {
                orderItem.OrderId = order.Id;
            }
            _context.OrderItems.AddRange(orderItems);

            var cartItems = await _context.CartItems.Where(ci => ci.UserId == userId).ToListAsync();
            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetOrderByIdAsync(userId, order.Id);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<PagedResultDto<OrderDto>> GetUserOrdersAsync(string userId, int page = 1, int pageSize = 10)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .ThenInclude(p => p.Category)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt);

        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)total / pageSize);

        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => MapToOrderDto(o))
            .ToListAsync();

        return new PagedResultDto<OrderDto>
        {
            Data = orders,
            Page = page,
            PageSize = pageSize,
            Total = total,
            TotalPages = totalPages
        };
    }

    public async Task<OrderDto> GetOrderByIdAsync(string userId, int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .ThenInclude(p => p.Category)
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product.Reviews)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
        {
            throw new NotFoundException("Order not found");
        }

        return MapToOrderDto(order);
    }

    public async Task<OrderDto> CancelOrderAsync(string userId, int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
        {
            throw new NotFoundException("Order not found");
        }

        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
        {
            throw new InvalidOperationException("Order cannot be cancelled");
        }

        foreach (var item in order.Items)
        {
            item.Product.StockQuantity += item.Quantity;
            item.Product.InStock = true;
        }

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetOrderByIdAsync(userId, orderId);
    }

    public async Task<OrderDto> ReorderAsync(string userId, int orderId)
    {
        var originalOrder = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (originalOrder == null)
        {
            throw new NotFoundException("Order not found");
        }

        var createOrderDto = new CreateOrderDto
        {
            Items = originalOrder.Items.Select(oi => new CreateOrderItemDto
            {
                ProductId = oi.ProductId,
                Quantity = oi.Quantity,
                Price = oi.Product.Price // Use current price
            }),
            ShippingAddress = new AddressDto
            {
                FirstName = originalOrder.ShippingFirstName,
                LastName = originalOrder.ShippingLastName,
                Company = originalOrder.ShippingCompany,
                Address1 = originalOrder.ShippingAddress1,
                Address2 = originalOrder.ShippingAddress2,
                City = originalOrder.ShippingCity,
                State = originalOrder.ShippingState,
                ZipCode = originalOrder.ShippingZipCode,
                Country = originalOrder.ShippingCountry,
                Phone = originalOrder.ShippingPhone
            },
            BillingAddress = new AddressDto
            {
                FirstName = originalOrder.BillingFirstName,
                LastName = originalOrder.BillingLastName,
                Company = originalOrder.BillingCompany,
                Address1 = originalOrder.BillingAddress1,
                Address2 = originalOrder.BillingAddress2,
                City = originalOrder.BillingCity,
                State = originalOrder.BillingState,
                ZipCode = originalOrder.BillingZipCode,
                Country = originalOrder.BillingCountry
            },
            PaymentMethod = new PaymentMethodDto
            {
                Type = originalOrder.PaymentType
            }
        };

        return await CreateOrderAsync(userId, createOrderDto);
    }

    public async Task<PagedResultDto<OrderDto>> GetAllOrdersAsync(int page = 1, int pageSize = 20)
    {
        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .ThenInclude(p => p.Category)
            .OrderByDescending(o => o.CreatedAt);

        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)total / pageSize);

        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => MapToOrderDto(o))
            .ToListAsync();

        return new PagedResultDto<OrderDto>
        {
            Data = orders,
            Page = page,
            PageSize = pageSize,
            Total = total,
            TotalPages = totalPages
        };
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto updateStatusDto)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
        {
            throw new NotFoundException("Order not found");
        }

        order.Status = updateStatusDto.Status;
        order.UpdatedAt = DateTime.UtcNow;

        if (updateStatusDto.Status == OrderStatus.Shipped && order.ShippedAt == null)
        {
            order.ShippedAt = DateTime.UtcNow;
        }
        else if (updateStatusDto.Status == OrderStatus.Delivered && order.DeliveredAt == null)
        {
            order.DeliveredAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return await GetOrderByIdAsync(order.UserId, orderId);
    }

    public async Task<OrderDto> AddTrackingNumberAsync(int orderId, AddTrackingNumberDto addTrackingDto)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
        {
            throw new NotFoundException("Order not found");
        }

        order.TrackingNumber = addTrackingDto.TrackingNumber;
        order.UpdatedAt = DateTime.UtcNow;

        if (order.Status == OrderStatus.Processing)
        {
            order.Status = OrderStatus.Shipped;
            order.ShippedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return await GetOrderByIdAsync(order.UserId, orderId);
    }

    private static OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Status = order.Status,
            Subtotal = order.Subtotal,
            Tax = order.Tax,
            Shipping = order.Shipping,
            Total = order.Total,
            TrackingNumber = order.TrackingNumber,
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt,
            OrderNotes = order.OrderNotes,
            ShippingAddress = new AddressDto
            {
                FirstName = order.ShippingFirstName,
                LastName = order.ShippingLastName,
                Company = order.ShippingCompany,
                Address1 = order.ShippingAddress1,
                Address2 = order.ShippingAddress2,
                City = order.ShippingCity,
                State = order.ShippingState,
                ZipCode = order.ShippingZipCode,
                Country = order.ShippingCountry,
                Phone = order.ShippingPhone
            },
            BillingAddress = new AddressDto
            {
                FirstName = order.BillingFirstName,
                LastName = order.BillingLastName,
                Company = order.BillingCompany,
                Address1 = order.BillingAddress1,
                Address2 = order.BillingAddress2,
                City = order.BillingCity,
                State = order.BillingState,
                ZipCode = order.BillingZipCode,
                Country = order.BillingCountry
            },
            PaymentMethod = new PaymentMethodDto
            {
                Type = order.PaymentType
            },
            Items = order.Items.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                Product = new ProductDto
                {
                    Id = oi.Product.Id,
                    Name = oi.Product.Name,
                    Description = oi.Product.Description,
                    Price = oi.Product.Price,
                    ImageUrl = oi.Product.ImageUrl,
                    CategoryId = oi.Product.CategoryId,
                    CategoryName = oi.Product.Category.Name,
                    InStock = oi.Product.InStock,
                    StockQuantity = oi.Product.StockQuantity,
                    Tags = oi.Product.Tags,
                    CreatedAt = oi.Product.CreatedAt,
                    UpdatedAt = oi.Product.UpdatedAt,
                    IsActive = oi.Product.IsActive,
                    IsFeatured = oi.Product.IsFeatured,
                    Rating = oi.Product.Reviews.Any() ? oi.Product.Reviews.Average(r => r.Rating) : 0,
                    ReviewCount = oi.Product.Reviews.Count
                },
                Quantity = oi.Quantity,
                Price = oi.Price
            })
        };
    }

    private static string GeneratePaymentReference()
    {
        return $"PAY_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}
