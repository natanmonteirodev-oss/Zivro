/**
 * Auth Service
 * Handles API calls for login, register, and token management
 */

import { httpClient } from '@/utils/http';
import { storage } from '@/utils/storage';
import type { LoginRequest, RegisterRequest, AuthResponse, User } from '../types';

class AuthService {
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    const response = await httpClient.post<AuthResponse>('/auth/login', credentials);
    const data = response.data;

    // Store tokens and user info
    storage.setAccessToken(data.accessToken);
    storage.setRefreshToken(data.refreshToken);

    return data;
  }

  async register(data: RegisterRequest): Promise<AuthResponse> {
    const response = await httpClient.post<AuthResponse>('/auth/register', data);
    const authData = response.data;

    // Store tokens and user info
    storage.setAccessToken(authData.accessToken);
    storage.setRefreshToken(authData.refreshToken);

    return authData;
  }

  async logout(): Promise<void> {
    try {
      const refreshToken = storage.getRefreshToken();
      if (refreshToken) {
        await httpClient.post('/auth/logout', { refreshToken });
      }
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      storage.clear();
    }
  }

  async getCurrentUser(): Promise<User | null> {
    try {
      const response = await httpClient.get<User>('/auth/me');
      return response.data;
    } catch (error) {
      console.error('Get current user error:', error);
      return null;
    }
  }

  isAuthenticated(): boolean {
    return !!storage.getAccessToken();
  }

  getAccessToken(): string | null {
    return storage.getAccessToken();
  }

  getRefreshToken(): string | null {
    return storage.getRefreshToken();
  }
}

export const authService = new AuthService();
