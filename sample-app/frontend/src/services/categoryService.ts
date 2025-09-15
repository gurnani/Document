import { api } from '@/lib/api';
import { Category } from '@/types';

export const categoryService = {
  getCategories: async (): Promise<Category[]> => {
    return api.get<Category[]>('/categories');
  },

  getCategory: async (id: number): Promise<Category> => {
    return api.get<Category>(`/categories/${id}`);
  },

  getCategoryTree: async (): Promise<Category[]> => {
    return api.get<Category[]>('/categories/tree');
  },

  createCategory: async (categoryData: Omit<Category, 'id' | 'productCount'>): Promise<Category> => {
    return api.post<Category>('/categories', categoryData);
  },

  updateCategory: async (id: number, categoryData: Partial<Category>): Promise<Category> => {
    return api.put<Category>(`/categories/${id}`, categoryData);
  },

  deleteCategory: async (id: number): Promise<void> => {
    return api.delete<void>(`/categories/${id}`);
  },
};
