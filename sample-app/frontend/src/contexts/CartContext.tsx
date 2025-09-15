import { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { CartItem, Product } from '@/types';
import { cartService } from '@/services/cartService';
import { useAuth } from './AuthContext';
import toast from 'react-hot-toast';

interface CartContextType {
  items: CartItem[];
  total: number;
  itemCount: number;
  isLoading: boolean;
  addItem: (product: Product, quantity?: number) => Promise<void>;
  removeItem: (productId: number) => Promise<void>;
  updateQuantity: (productId: number, quantity: number) => Promise<void>;
  clearCart: () => Promise<void>;
  refreshCart: () => Promise<void>;
}

const CartContext = createContext<CartContextType | undefined>(undefined);

interface CartProviderProps {
  children: ReactNode;
}

export const CartProvider = ({ children }: CartProviderProps) => {
  const [items, setItems] = useState<CartItem[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const { user, isAuthenticated } = useAuth();

  const total = items.reduce((sum, item) => sum + (item.price * item.quantity), 0);
  const itemCount = items.reduce((sum, item) => sum + item.quantity, 0);

  useEffect(() => {
    if (isAuthenticated && user) {
      refreshCart();
    } else {
      loadGuestCart();
    }
  }, [isAuthenticated, user]);

  const loadGuestCart = () => {
    try {
      const guestCart = localStorage.getItem('guestCart');
      if (guestCart) {
        const parsedCart = JSON.parse(guestCart);
        setItems(parsedCart);
      }
    } catch (error) {
      console.error('Failed to load guest cart:', error);
    }
  };

  const saveGuestCart = (cartItems: CartItem[]) => {
    try {
      localStorage.setItem('guestCart', JSON.stringify(cartItems));
    } catch (error) {
      console.error('Failed to save guest cart:', error);
    }
  };

  const refreshCart = async () => {
    if (!isAuthenticated) {
      loadGuestCart();
      return;
    }

    try {
      setIsLoading(true);
      const cart = await cartService.getCart();
      setItems(cart.items);
    } catch (error) {
      console.error('Failed to load cart:', error);
      toast.error('Failed to load cart');
    } finally {
      setIsLoading(false);
    }
  };

  const addItem = async (product: Product, quantity: number = 1) => {
    try {
      setIsLoading(true);

      if (isAuthenticated) {
        await cartService.addItem(product.id, quantity);
        await refreshCart();
      } else {
        const existingItemIndex = items.findIndex(item => item.productId === product.id);
        let newItems: CartItem[];

        if (existingItemIndex >= 0) {
          newItems = items.map((item, index) =>
            index === existingItemIndex
              ? { ...item, quantity: item.quantity + quantity }
              : item
          );
        } else {
          const newItem: CartItem = {
            id: Date.now(), // Temporary ID for guest cart
            productId: product.id,
            product,
            quantity,
            price: product.price,
          };
          newItems = [...items, newItem];
        }

        setItems(newItems);
        saveGuestCart(newItems);
      }

      toast.success(`${product.name} added to cart`);
    } catch (error: any) {
      toast.error(error.message || 'Failed to add item to cart');
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const removeItem = async (productId: number) => {
    try {
      setIsLoading(true);

      if (isAuthenticated) {
        await cartService.removeItem(productId);
        await refreshCart();
      } else {
        const newItems = items.filter(item => item.productId !== productId);
        setItems(newItems);
        saveGuestCart(newItems);
      }

      toast.success('Item removed from cart');
    } catch (error: any) {
      toast.error(error.message || 'Failed to remove item from cart');
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const updateQuantity = async (productId: number, quantity: number) => {
    if (quantity <= 0) {
      await removeItem(productId);
      return;
    }

    try {
      setIsLoading(true);

      if (isAuthenticated) {
        await cartService.updateQuantity(productId, quantity);
        await refreshCart();
      } else {
        const newItems = items.map(item =>
          item.productId === productId
            ? { ...item, quantity }
            : item
        );
        setItems(newItems);
        saveGuestCart(newItems);
      }
    } catch (error: any) {
      toast.error(error.message || 'Failed to update quantity');
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const clearCart = async () => {
    try {
      setIsLoading(true);

      if (isAuthenticated) {
        await cartService.clearCart();
      } else {
        localStorage.removeItem('guestCart');
      }

      setItems([]);
      toast.success('Cart cleared');
    } catch (error: any) {
      toast.error(error.message || 'Failed to clear cart');
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const value: CartContextType = {
    items,
    total,
    itemCount,
    isLoading,
    addItem,
    removeItem,
    updateQuantity,
    clearCart,
    refreshCart,
  };

  return (
    <CartContext.Provider value={value}>
      {children}
    </CartContext.Provider>
  );
};

export const useCart = () => {
  const context = useContext(CartContext);
  if (context === undefined) {
    throw new Error('useCart must be used within a CartProvider');
  }
  return context;
};
