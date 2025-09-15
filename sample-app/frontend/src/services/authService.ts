import { api } from '@/lib/api';
import { User, LoginForm, RegisterForm } from '@/types';

interface AuthResponse {
  user: User;
  token: string;
}

export const authService = {
  login: async (credentials: LoginForm): Promise<AuthResponse> => {
    return api.post<AuthResponse>('/auth/login', credentials);
  },

  register: async (userData: RegisterForm): Promise<AuthResponse> => {
    return api.post<AuthResponse>('/auth/register', userData);
  },

  verifyToken: async (): Promise<User> => {
    return api.get<User>('/auth/me');
  },

  refreshToken: async (): Promise<AuthResponse> => {
    return api.post<AuthResponse>('/auth/refresh');
  },

  logout: async (): Promise<void> => {
    return api.post<void>('/auth/logout');
  },

  updateProfile: async (userData: Partial<User>): Promise<User> => {
    return api.put<User>('/auth/profile', userData);
  },

  changePassword: async (currentPassword: string, newPassword: string): Promise<void> => {
    return api.post<void>('/auth/change-password', {
      currentPassword,
      newPassword,
    });
  },

  requestPasswordReset: async (email: string): Promise<void> => {
    return api.post<void>('/auth/forgot-password', { email });
  },

  resetPassword: async (token: string, newPassword: string): Promise<void> => {
    return api.post<void>('/auth/reset-password', {
      token,
      newPassword,
    });
  },

  verifyEmail: async (token: string): Promise<void> => {
    return api.post<void>('/auth/verify-email', { token });
  },

  resendVerificationEmail: async (): Promise<void> => {
    return api.post<void>('/auth/resend-verification');
  },
};
