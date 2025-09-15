import { ReactNode, ComponentType } from 'react';

export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  imageUrl: string;
  categoryId: number;
  category?: Category;
  rating: number;
  reviewCount: number;
  inStock: boolean;
  tags: string[];
  createdAt: string;
  updatedAt: string;
}

export interface Category {
  id: number;
  name: string;
  description: string;
  imageUrl?: string;
  parentId?: number;
  children?: Category[];
  productCount: number;
}

export interface User {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  role: 'Customer' | 'Admin';
  isActive: boolean;
  createdAt: string;
  avatar?: string;
}

export interface CartItem {
  id: number;
  productId: number;
  product: Product;
  quantity: number;
  price: number;
}

export interface Cart {
  id: number;
  userId: number;
  items: CartItem[];
  total: number;
  itemCount: number;
  updatedAt: string;
}

export interface Order {
  id: number;
  userId: number;
  user?: User;
  items: OrderItem[];
  status: OrderStatus;
  total: number;
  shippingAddress: Address;
  billingAddress: Address;
  paymentMethod: PaymentMethod;
  createdAt: string;
  updatedAt: string;
  trackingNumber?: string;
}

export interface OrderItem {
  id: number;
  orderId: number;
  productId: number;
  product: Product;
  quantity: number;
  price: number;
  total: number;
}

export interface Address {
  id?: number;
  firstName: string;
  lastName: string;
  company?: string;
  address1: string;
  address2?: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  phone?: string;
}

export interface PaymentMethod {
  id?: number;
  type: 'CreditCard' | 'PayPal' | 'ApplePay' | 'GooglePay';
  last4?: string;
  expiryMonth?: number;
  expiryYear?: number;
  brand?: string;
}

export interface Review {
  id: number;
  productId: number;
  userId: number;
  user: User;
  rating: number;
  title: string;
  comment: string;
  isVerifiedPurchase: boolean;
  helpfulCount: number;
  createdAt: string;
}

export type OrderStatus = 
  | 'Pending'
  | 'Processing'
  | 'Shipped'
  | 'Delivered'
  | 'Cancelled'
  | 'Refunded';

export type SortOption = 
  | 'name'
  | 'price'
  | 'rating'
  | 'newest'
  | 'popularity';

export type SortOrder = 'asc' | 'desc';

export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
  errors?: string[];
}

export interface PagedResponse<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ProductFilters {
  categoryId?: number;
  searchTerm?: string;
  minPrice?: number;
  maxPrice?: number;
  rating?: number;
  inStock?: boolean;
  tags?: string[];
  sortBy?: SortOption;
  sortOrder?: SortOrder;
  page?: number;
  pageSize?: number;
}

export interface LoginForm {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface RegisterForm {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
  acceptTerms: boolean;
}

export interface CheckoutForm {
  shippingAddress: Address;
  
  billingAddress: Address;
  sameAsShipping: boolean;
  
  paymentMethod: PaymentMethod;
  
  orderNotes?: string;
}

export interface ProductReviewForm {
  rating: number;
  title: string;
  comment: string;
}

export interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}

export interface CartState {
  items: CartItem[];
  total: number;
  itemCount: number;
  isLoading: boolean;
  error: string | null;
}

export interface ApiError {
  message: string;
  status: number;
  errors?: Record<string, string[]>;
}

export type LoadingState = 'idle' | 'loading' | 'success' | 'error';

export interface AsyncState<T> {
  data: T | null;
  status: LoadingState;
  error: string | null;
}

export interface BaseComponentProps {
  className?: string;
  children?: ReactNode;
}

export interface ButtonProps extends BaseComponentProps {
  variant?: 'primary' | 'secondary' | 'outline' | 'ghost' | 'destructive';
  size?: 'sm' | 'md' | 'lg';
  disabled?: boolean;
  loading?: boolean;
  onClick?: () => void;
  type?: 'button' | 'submit' | 'reset';
}

export interface InputProps extends BaseComponentProps {
  type?: string;
  placeholder?: string;
  value?: string;
  onChange?: (value: string) => void;
  error?: string;
  disabled?: boolean;
  required?: boolean;
}

export interface NavItem {
  label: string;
  href: string;
  icon?: ComponentType;
  children?: NavItem[];
}

export interface SearchSuggestion {
  type: 'product' | 'category' | 'brand';
  id: number;
  name: string;
  imageUrl?: string;
}

export interface SearchResult {
  products: Product[];
  categories: Category[];
  suggestions: SearchSuggestion[];
  total: number;
}

export interface AnalyticsEvent {
  event: string;
  properties: Record<string, any>;
  timestamp: string;
}

export interface AppConfig {
  apiUrl: string;
  stripePublicKey: string;
  googleAnalyticsId?: string;
  sentryDsn?: string;
  environment: 'development' | 'staging' | 'production';
}
