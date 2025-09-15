import axios, { AxiosInstance, AxiosResponse, AxiosError } from 'axios';
import { ApiResponse, PagedResponse, ApiError } from '@/types';

const createApiClient = (): AxiosInstance => {
  const client = axios.create({
    baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
    timeout: 10000,
    headers: {
      'Content-Type': 'application/json',
    },
  });

  client.interceptors.request.use(
    (config) => {
      const token = localStorage.getItem('authToken');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    },
    (error) => {
      return Promise.reject(error);
    }
  );

  client.interceptors.response.use(
    (response: AxiosResponse) => {
      return response;
    },
    (error: AxiosError) => {
      const apiError: ApiError = {
        message: 'An unexpected error occurred',
        status: error.response?.status || 500,
      };

      if (error.response?.data) {
        const errorData = error.response.data as any;
        apiError.message = errorData.message || errorData.title || apiError.message;
        apiError.errors = errorData.errors;
      } else if (error.message) {
        apiError.message = error.message;
      }

      if (error.response?.status === 401) {
        localStorage.removeItem('authToken');
        window.location.href = '/login';
      }

      return Promise.reject(apiError);
    }
  );

  return client;
};

export const apiClient = createApiClient();

export const api = {
  get: async <T>(url: string, params?: any): Promise<T> => {
    const response = await apiClient.get<ApiResponse<T>>(url, { params });
    return response.data.data;
  },

  post: async <T>(url: string, data?: any): Promise<T> => {
    const response = await apiClient.post<ApiResponse<T>>(url, data);
    return response.data.data;
  },

  put: async <T>(url: string, data?: any): Promise<T> => {
    const response = await apiClient.put<ApiResponse<T>>(url, data);
    return response.data.data;
  },

  patch: async <T>(url: string, data?: any): Promise<T> => {
    const response = await apiClient.patch<ApiResponse<T>>(url, data);
    return response.data.data;
  },

  delete: async <T>(url: string): Promise<T> => {
    const response = await apiClient.delete<ApiResponse<T>>(url);
    return response.data.data;
  },

  getPaged: async <T>(url: string, params?: any): Promise<PagedResponse<T>> => {
    const response = await apiClient.get<PagedResponse<T>>(url, { params });
    return response.data;
  },
};

export const handleApiError = (error: unknown): string => {
  if (error && typeof error === 'object' && 'message' in error) {
    return (error as ApiError).message;
  }
  return 'An unexpected error occurred';
};

export const isApiError = (error: unknown): error is ApiError => {
  return error !== null && typeof error === 'object' && 'message' in error && 'status' in error;
};

if (import.meta.env.DEV) {
  apiClient.interceptors.request.use((config) => {
    console.log(`🚀 API Request: ${config.method?.toUpperCase()} ${config.url}`, {
      params: config.params,
      data: config.data,
    });
    return config;
  });

  apiClient.interceptors.response.use(
    (response) => {
      console.log(`✅ API Response: ${response.config.method?.toUpperCase()} ${response.config.url}`, {
        status: response.status,
        data: response.data,
      });
      return response;
    },
    (error) => {
      console.error(`❌ API Error: ${error.config?.method?.toUpperCase()} ${error.config?.url}`, {
        status: error.response?.status,
        message: error.message,
        data: error.response?.data,
      });
      return Promise.reject(error);
    }
  );
}
