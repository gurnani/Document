import { api } from '@/lib/api';
import { Order, OrderItem, Address, PaymentMethod } from '@/types';

interface CreateOrderRequest {
  items: {
    productId: number;
    quantity: number;
    price: number;
  }[];
  shippingAddress: Address;
  billingAddress: Address;
  paymentMethod: PaymentMethod;
  orderNotes?: string;
}

export const orderService = {
  createOrder: async (orderData: CreateOrderRequest): Promise<Order> => {
    return api.post<Order>('/orders', orderData);
  },

  getUserOrders: async (): Promise<Order[]> => {
    return api.get<Order[]>('/orders/my-orders');
  },

  getOrder: async (id: number): Promise<Order> => {
    return api.get<Order>(`/orders/${id}`);
  },

  cancelOrder: async (id: number): Promise<Order> => {
    return api.post<Order>(`/orders/${id}/cancel`);
  },

  getOrderTracking: async (id: number): Promise<{
    trackingNumber: string;
    status: string;
    estimatedDelivery: string;
    trackingEvents: {
      date: string;
      status: string;
      location: string;
      description: string;
    }[];
  }> => {
    return api.get(`/orders/${id}/tracking`);
  },

  reorder: async (orderId: number): Promise<Order> => {
    return api.post<Order>(`/orders/${orderId}/reorder`);
  },

  getAllOrders: async (page: number = 1, pageSize: number = 20): Promise<{
    orders: Order[];
    total: number;
    page: number;
    totalPages: number;
  }> => {
    return api.getPaged<Order>('/admin/orders', { page, pageSize });
  },

  updateOrderStatus: async (id: number, status: string): Promise<Order> => {
    return api.patch<Order>(`/admin/orders/${id}/status`, { status });
  },

  addTrackingNumber: async (id: number, trackingNumber: string): Promise<Order> => {
    return api.patch<Order>(`/admin/orders/${id}/tracking`, { trackingNumber });
  },
};
