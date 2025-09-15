# Frontend Development Instructions for GitHub Copilot

## React + TypeScript Development Context

This project uses React 18 with TypeScript, Tailwind CSS, and modern development practices for a comprehensive e-commerce platform frontend.

## Architecture & Patterns

### Component Architecture
- **Atomic Design**: Atoms, molecules, organisms, templates, pages
- **Feature-based Structure**: Group by business domain
- **Custom Hooks**: Reusable logic extraction
- **Context Providers**: Global state management
- **Higher-Order Components**: Cross-cutting concerns

### State Management Strategy
- **React Query**: Server state management
- **Zustand**: Client state management
- **Context API**: Component tree state
- **Local State**: Component-specific state

## Code Generation Guidelines

### Component Structure
```typescript
interface ProductCardProps {
  product: Product;
  onAddToCart: (productId: number) => void;
  className?: string;
}

export const ProductCard: React.FC<ProductCardProps> = ({ 
  product, 
  onAddToCart, 
  className = '' 
}) => {
  const [isLoading, setIsLoading] = useState(false);

  const handleAddToCart = async () => {
    setIsLoading(true);
    try {
      await onAddToCart(product.id);
    } catch (error) {
      console.error('Failed to add to cart:', error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className={`bg-white rounded-lg shadow-md p-4 ${className}`}>
      <img 
        src={product.imageUrl} 
        alt={product.name}
        className="w-full h-48 object-cover rounded-md mb-4"
      />
      <h3 className="text-lg font-semibold text-gray-900 mb-2">
        {product.name}
      </h3>
      <p className="text-gray-600 text-sm mb-4 line-clamp-2">
        {product.description}
      </p>
      <div className="flex items-center justify-between">
        <span className="text-xl font-bold text-green-600">
          ${product.price.toFixed(2)}
        </span>
        <Button
          onClick={handleAddToCart}
          disabled={isLoading}
          className="bg-blue-600 hover:bg-blue-700"
        >
          {isLoading ? 'Adding...' : 'Add to Cart'}
        </Button>
      </div>
    </div>
  );
};
```

### Custom Hook Pattern
```typescript
interface UseProductsOptions {
  categoryId?: number;
  searchTerm?: string;
  sortBy?: 'name' | 'price' | 'rating';
  sortOrder?: 'asc' | 'desc';
}

export const useProducts = (options: UseProductsOptions = {}) => {
  const { categoryId, searchTerm, sortBy, sortOrder } = options;

  return useQuery({
    queryKey: ['products', { categoryId, searchTerm, sortBy, sortOrder }],
    queryFn: () => productApi.getProducts({
      categoryId,
      searchTerm,
      sortBy,
      sortOrder
    }),
    staleTime: 5 * 60 * 1000, // 5 minutes
    cacheTime: 10 * 60 * 1000, // 10 minutes
  });
};

// Usage in component
export const ProductList: React.FC = () => {
  const [filters, setFilters] = useState<UseProductsOptions>({});
  const { data: products, isLoading, error } = useProducts(filters);

  if (isLoading) return <ProductListSkeleton />;
  if (error) return <ErrorMessage error={error} />;

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
      {products?.map(product => (
        <ProductCard
          key={product.id}
          product={product}
          onAddToCart={handleAddToCart}
        />
      ))}
    </div>
  );
};
```

### Form Handling with React Hook Form
```typescript
interface CheckoutFormData {
  email: string;
  firstName: string;
  lastName: string;
  address: string;
  city: string;
  zipCode: string;
  cardNumber: string;
  expiryDate: string;
  cvv: string;
}

const checkoutSchema = z.object({
  email: z.string().email('Invalid email address'),
  firstName: z.string().min(2, 'First name must be at least 2 characters'),
  lastName: z.string().min(2, 'Last name must be at least 2 characters'),
  address: z.string().min(5, 'Address must be at least 5 characters'),
  city: z.string().min(2, 'City must be at least 2 characters'),
  zipCode: z.string().regex(/^\d{5}(-\d{4})?$/, 'Invalid ZIP code'),
  cardNumber: z.string().regex(/^\d{16}$/, 'Card number must be 16 digits'),
  expiryDate: z.string().regex(/^(0[1-9]|1[0-2])\/\d{2}$/, 'Invalid expiry date'),
  cvv: z.string().regex(/^\d{3,4}$/, 'CVV must be 3 or 4 digits'),
});

export const CheckoutForm: React.FC = () => {
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<CheckoutFormData>({
    resolver: zodResolver(checkoutSchema),
  });

  const onSubmit = async (data: CheckoutFormData) => {
    try {
      await orderApi.createOrder(data);
      toast.success('Order placed successfully!');
      // Redirect to success page
    } catch (error) {
      toast.error('Failed to place order. Please try again.');
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            First Name
          </label>
          <input
            {...register('firstName')}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          {errors.firstName && (
            <p className="text-red-500 text-sm mt-1">{errors.firstName.message}</p>
          )}
        </div>
        {/* Additional form fields */}
      </div>
      
      <Button
        type="submit"
        disabled={isSubmitting}
        className="w-full bg-green-600 hover:bg-green-700"
      >
        {isSubmitting ? 'Processing...' : 'Place Order'}
      </Button>
    </form>
  );
};
```

## State Management Patterns

### Zustand Store
```typescript
interface CartState {
  items: CartItem[];
  total: number;
  addItem: (product: Product, quantity?: number) => void;
  removeItem: (productId: number) => void;
  updateQuantity: (productId: number, quantity: number) => void;
  clearCart: () => void;
}

export const useCartStore = create<CartState>((set, get) => ({
  items: [],
  total: 0,

  addItem: (product, quantity = 1) => {
    set((state) => {
      const existingItem = state.items.find(item => item.product.id === product.id);
      
      if (existingItem) {
        const updatedItems = state.items.map(item =>
          item.product.id === product.id
            ? { ...item, quantity: item.quantity + quantity }
            : item
        );
        return {
          items: updatedItems,
          total: calculateTotal(updatedItems),
        };
      } else {
        const newItems = [...state.items, { product, quantity }];
        return {
          items: newItems,
          total: calculateTotal(newItems),
        };
      }
    });
  },

  removeItem: (productId) => {
    set((state) => {
      const updatedItems = state.items.filter(item => item.product.id !== productId);
      return {
        items: updatedItems,
        total: calculateTotal(updatedItems),
      };
    });
  },

  updateQuantity: (productId, quantity) => {
    if (quantity <= 0) {
      get().removeItem(productId);
      return;
    }

    set((state) => {
      const updatedItems = state.items.map(item =>
        item.product.id === productId
          ? { ...item, quantity }
          : item
      );
      return {
        items: updatedItems,
        total: calculateTotal(updatedItems),
      };
    });
  },

  clearCart: () => set({ items: [], total: 0 }),
}));

const calculateTotal = (items: CartItem[]): number => {
  return items.reduce((total, item) => total + (item.product.price * item.quantity), 0);
};
```

### Context Provider Pattern
```typescript
interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  register: (userData: RegisterData) => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const token = localStorage.getItem('authToken');
    if (token) {
      authApi.verifyToken(token)
        .then(userData => setUser(userData))
        .catch(() => localStorage.removeItem('authToken'))
        .finally(() => setIsLoading(false));
    } else {
      setIsLoading(false);
    }
  }, []);

  const login = async (email: string, password: string) => {
    const { user, token } = await authApi.login(email, password);
    localStorage.setItem('authToken', token);
    setUser(user);
  };

  const logout = () => {
    localStorage.removeItem('authToken');
    setUser(null);
  };

  const register = async (userData: RegisterData) => {
    const { user, token } = await authApi.register(userData);
    localStorage.setItem('authToken', token);
    setUser(user);
  };

  if (isLoading) {
    return <LoadingSpinner />;
  }

  return (
    <AuthContext.Provider value={{
      user,
      isAuthenticated: !!user,
      login,
      logout,
      register,
    }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
```

## Styling Guidelines

### Tailwind CSS Patterns
```typescript
// Component with responsive design
export const ProductGrid: React.FC<{ products: Product[] }> = ({ products }) => {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4 md:gap-6">
      {products.map(product => (
        <ProductCard key={product.id} product={product} />
      ))}
    </div>
  );
};

// Button variants with consistent styling
interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'danger' | 'outline';
  size?: 'sm' | 'md' | 'lg';
  children: React.ReactNode;
}

export const Button: React.FC<ButtonProps> = ({ 
  variant = 'primary', 
  size = 'md', 
  className = '', 
  children, 
  ...props 
}) => {
  const baseClasses = 'font-medium rounded-md transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2';
  
  const variantClasses = {
    primary: 'bg-blue-600 text-white hover:bg-blue-700 focus:ring-blue-500',
    secondary: 'bg-gray-200 text-gray-900 hover:bg-gray-300 focus:ring-gray-500',
    danger: 'bg-red-600 text-white hover:bg-red-700 focus:ring-red-500',
    outline: 'border border-gray-300 text-gray-700 hover:bg-gray-50 focus:ring-blue-500',
  };

  const sizeClasses = {
    sm: 'px-3 py-1.5 text-sm',
    md: 'px-4 py-2 text-base',
    lg: 'px-6 py-3 text-lg',
  };

  return (
    <button
      className={`${baseClasses} ${variantClasses[variant]} ${sizeClasses[size]} ${className}`}
      {...props}
    >
      {children}
    </button>
  );
};
```

## Error Handling & Loading States

### Error Boundary
```typescript
interface ErrorBoundaryState {
  hasError: boolean;
  error?: Error;
}

export class ErrorBoundary extends React.Component<
  React.PropsWithChildren<{}>,
  ErrorBoundaryState
> {
  constructor(props: React.PropsWithChildren<{}>) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error('Error caught by boundary:', error, errorInfo);
    // Log to error reporting service
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="min-h-screen flex items-center justify-center bg-gray-50">
          <div className="max-w-md w-full bg-white shadow-lg rounded-lg p-6">
            <div className="flex items-center mb-4">
              <AlertTriangle className="h-8 w-8 text-red-500 mr-3" />
              <h1 className="text-xl font-semibold text-gray-900">
                Something went wrong
              </h1>
            </div>
            <p className="text-gray-600 mb-4">
              We're sorry, but something unexpected happened. Please try refreshing the page.
            </p>
            <Button
              onClick={() => window.location.reload()}
              className="w-full"
            >
              Refresh Page
            </Button>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}
```

### Loading States
```typescript
export const ProductListSkeleton: React.FC = () => {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
      {Array.from({ length: 8 }).map((_, index) => (
        <div key={index} className="bg-white rounded-lg shadow-md p-4 animate-pulse">
          <div className="w-full h-48 bg-gray-300 rounded-md mb-4"></div>
          <div className="h-4 bg-gray-300 rounded mb-2"></div>
          <div className="h-3 bg-gray-300 rounded mb-4 w-3/4"></div>
          <div className="flex items-center justify-between">
            <div className="h-6 bg-gray-300 rounded w-20"></div>
            <div className="h-10 bg-gray-300 rounded w-24"></div>
          </div>
        </div>
      ))}
    </div>
  );
};
```

## Performance Optimization

### Code Splitting
```typescript
// Lazy load components
const ProductDetails = lazy(() => import('./ProductDetails'));
const Checkout = lazy(() => import('./Checkout'));
const UserProfile = lazy(() => import('./UserProfile'));

// Route-based code splitting
export const AppRoutes: React.FC = () => {
  return (
    <Routes>
      <Route path="/" element={<Home />} />
      <Route path="/products" element={<ProductList />} />
      <Route 
        path="/products/:id" 
        element={
          <Suspense fallback={<ProductDetailsSkeleton />}>
            <ProductDetails />
          </Suspense>
        } 
      />
      <Route 
        path="/checkout" 
        element={
          <Suspense fallback={<CheckoutSkeleton />}>
            <Checkout />
          </Suspense>
        } 
      />
    </Routes>
  );
};
```

### Memoization
```typescript
// Memoize expensive calculations
export const ProductList: React.FC<{ products: Product[] }> = ({ products }) => {
  const sortedProducts = useMemo(() => {
    return [...products].sort((a, b) => b.rating - a.rating);
  }, [products]);

  const handleAddToCart = useCallback((productId: number) => {
    // Add to cart logic
  }, []);

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
      {sortedProducts.map(product => (
        <ProductCard
          key={product.id}
          product={product}
          onAddToCart={handleAddToCart}
        />
      ))}
    </div>
  );
};

// Memoize components
export const ProductCard = React.memo<ProductCardProps>(({ product, onAddToCart }) => {
  // Component implementation
});
```

## Testing Patterns

### Component Testing
```typescript
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ProductCard } from './ProductCard';

const createTestQueryClient = () => new QueryClient({
  defaultOptions: {
    queries: { retry: false },
    mutations: { retry: false },
  },
});

const renderWithProviders = (component: React.ReactElement) => {
  const queryClient = createTestQueryClient();
  return render(
    <QueryClientProvider client={queryClient}>
      {component}
    </QueryClientProvider>
  );
};

describe('ProductCard', () => {
  const mockProduct: Product = {
    id: 1,
    name: 'Test Product',
    description: 'Test Description',
    price: 99.99,
    imageUrl: 'test-image.jpg',
    rating: 4.5,
  };

  const mockOnAddToCart = jest.fn();

  beforeEach(() => {
    mockOnAddToCart.mockClear();
  });

  it('renders product information correctly', () => {
    renderWithProviders(
      <ProductCard product={mockProduct} onAddToCart={mockOnAddToCart} />
    );

    expect(screen.getByText('Test Product')).toBeInTheDocument();
    expect(screen.getByText('Test Description')).toBeInTheDocument();
    expect(screen.getByText('$99.99')).toBeInTheDocument();
  });

  it('calls onAddToCart when add to cart button is clicked', async () => {
    renderWithProviders(
      <ProductCard product={mockProduct} onAddToCart={mockOnAddToCart} />
    );

    const addToCartButton = screen.getByText('Add to Cart');
    fireEvent.click(addToCartButton);

    await waitFor(() => {
      expect(mockOnAddToCart).toHaveBeenCalledWith(1);
    });
  });
});
```

### Custom Hook Testing
```typescript
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useProducts } from './useProducts';

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );
};

describe('useProducts', () => {
  it('fetches products successfully', async () => {
    const { result } = renderHook(() => useProducts(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(result.current.data).toBeDefined();
    expect(Array.isArray(result.current.data)).toBe(true);
  });
});
```

---

*These instructions help GitHub Copilot generate consistent, high-quality React + TypeScript code following modern development practices and patterns.*
