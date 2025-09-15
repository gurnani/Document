import { api } from '@/lib/api';
import { Product, ProductFilters, PagedResponse } from '@/types';

export const productService = {
  getProducts: async (filters: ProductFilters = {}): Promise<PagedResponse<Product>> => {
    const params = {
      page: filters.page || 1,
      pageSize: filters.pageSize || 12,
      categoryId: filters.categoryId,
      searchTerm: filters.searchTerm,
      minPrice: filters.minPrice,
      maxPrice: filters.maxPrice,
      rating: filters.rating,
      inStock: filters.inStock,
      tags: filters.tags?.join(','),
      sortBy: filters.sortBy || 'name',
      sortOrder: filters.sortOrder || 'asc',
    };

    const cleanParams = Object.fromEntries(
      Object.entries(params).filter(([_, value]) => value !== undefined)
    );

    return api.getPaged<Product>('/products', cleanParams);
  },

  getProduct: async (id: number): Promise<Product> => {
    return api.get<Product>(`/products/${id}`);
  },

  getFeaturedProducts: async (limit: number = 8): Promise<Product[]> => {
    return api.get<Product[]>('/products/featured', { limit });
  },

  getProductsByCategory: async (categoryId: number, page: number = 1, pageSize: number = 12): Promise<PagedResponse<Product>> => {
    return api.getPaged<Product>('/products', {
      categoryId,
      page,
      pageSize,
    });
  },

  searchProducts: async (searchTerm: string, page: number = 1, pageSize: number = 12): Promise<PagedResponse<Product>> => {
    return api.getPaged<Product>('/products', {
      searchTerm,
      page,
      pageSize,
    });
  },

  getRelatedProducts: async (productId: number, limit: number = 4): Promise<Product[]> => {
    return api.get<Product[]>(`/products/${productId}/related`, { limit });
  },

  getRecommendedProducts: async (userId?: number, limit: number = 8): Promise<Product[]> => {
    const params = userId ? { userId, limit } : { limit };
    return api.get<Product[]>('/products/recommended', params);
  },

  createProduct: async (productData: Omit<Product, 'id' | 'createdAt' | 'updatedAt'>): Promise<Product> => {
    return api.post<Product>('/products', productData);
  },

  updateProduct: async (id: number, productData: Partial<Product>): Promise<Product> => {
    return api.put<Product>(`/products/${id}`, productData);
  },

  deleteProduct: async (id: number): Promise<void> => {
    return api.delete<void>(`/products/${id}`);
  },

  updateStock: async (id: number, quantity: number): Promise<Product> => {
    return api.patch<Product>(`/products/${id}/stock`, { quantity });
  },

  uploadProductImage: async (productId: number, imageFile: File): Promise<string> => {
    const formData = new FormData();
    formData.append('image', imageFile);
    
    return api.post<string>(`/products/${productId}/images`, formData);
  },

  deleteProductImage: async (productId: number, imageUrl: string): Promise<void> => {
    return api.delete<void>(`/products/${productId}/images`, { imageUrl });
  },
};

export const productQueries = {
  all: ['products'] as const,
  lists: () => [...productQueries.all, 'list'] as const,
  list: (filters: ProductFilters) => [...productQueries.lists(), filters] as const,
  details: () => [...productQueries.all, 'detail'] as const,
  detail: (id: number) => [...productQueries.details(), id] as const,
  featured: () => [...productQueries.all, 'featured'] as const,
  related: (id: number) => [...productQueries.all, 'related', id] as const,
  recommended: (userId?: number) => [...productQueries.all, 'recommended', userId] as const,
};
