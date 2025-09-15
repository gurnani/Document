import { api } from '@/lib/api';
import { Cart, CartItem } from '@/types';

export const cartService = {
  getCart: async (): Promise<Cart> => {
    return api.get<Cart>('/cart');
  },

  addItem: async (productId: number, quantity: number = 1): Promise<CartItem> => {
    return api.post<CartItem>('/cart/items', {
      productId,
      quantity,
    });
  },

  updateQuantity: async (productId: number, quantity: number): Promise<CartItem> => {
    return api.put<CartItem>(`/cart/items/${productId}`, {
      quantity,
    });
  },

  removeItem: async (productId: number): Promise<void> => {
    return api.delete<void>(`/cart/items/${productId}`);
  },

  clearCart: async (): Promise<void> => {
    return api.delete<void>('/cart');
  },

  applyCoupon: async (couponCode: string): Promise<Cart> => {
    return api.post<Cart>('/cart/coupon', {
      couponCode,
    });
  },

  removeCoupon: async (): Promise<Cart> => {
    return api.delete<Cart>('/cart/coupon');
  },

  getCartSummary: async (): Promise<{
    subtotal: number;
    tax: number;
    shipping: number;
    discount: number;
    total: number;
  }> => {
    return api.get('/cart/summary');
  },

  mergeGuestCart: async (guestCartItems: CartItem[]): Promise<Cart> => {
    return api.post<Cart>('/cart/merge', {
      items: guestCartItems.map(item => ({
        productId: item.productId,
        quantity: item.quantity,
      })),
    });
  },
};
