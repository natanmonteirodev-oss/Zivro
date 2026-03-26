/**
 * Custom hook for authentication operations
 */

import { useCallback, useState } from 'react';
import { authService } from '../services';
import { useAuth } from '@/store/authStore';
import { storage } from '@/utils/storage';
import type { LoginRequest, RegisterRequest, User } from '../types';

interface UseAuthOperations {
  login: (credentials: LoginRequest) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
  isLoading: boolean;
  error: string | null;
}

export const useAuthOperations = (): UseAuthOperations => {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { setUser, setIsAuthenticated, setError: setStoreError } = useAuth();

  const login = useCallback(
    async (credentials: LoginRequest) => {
      try {
        setIsLoading(true);
        setError(null);

        const response = await authService.login(credentials);

        // Create user object from response
        const user: User = {
          id: response.userId,
          name: credentials.email.split('@')[0],
          email: credentials.email,
          isActive: true,
          createdAt: new Date().toISOString(),
        };

        storage.setUser(user);
        setUser(user);
        setIsAuthenticated(true);
      } catch (err: any) {
        const errorMessage =
          err.response?.data?.message || err.message || 'Erro ao fazer login';
        setError(errorMessage);
        setStoreError(errorMessage);
        throw err;
      } finally {
        setIsLoading(false);
      }
    },
    [setUser, setIsAuthenticated, setStoreError],
  );

  const register = useCallback(
    async (data: RegisterRequest) => {
      try {
        setIsLoading(true);
        setError(null);

        const response = await authService.register(data);

        // Create user object from response
        const user: User = {
          id: response.userId,
          name: data.name,
          email: data.email,
          isActive: true,
          createdAt: new Date().toISOString(),
        };

        storage.setUser(user);
        setUser(user);
        setIsAuthenticated(true);
      } catch (err: any) {
        const errorMessage =
          err.response?.data?.message || err.message || 'Erro ao registrar';
        setError(errorMessage);
        setStoreError(errorMessage);
        throw err;
      } finally {
        setIsLoading(false);
      }
    },
    [setUser, setIsAuthenticated, setStoreError],
  );

  const logout = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      await authService.logout();
      setUser(null);
      setIsAuthenticated(false);
    } catch (err: any) {
      const errorMessage = err.message || 'Erro ao fazer logout';
      setError(errorMessage);
      setStoreError(errorMessage);
    } finally {
      setIsLoading(false);
    }
  }, [setUser, setIsAuthenticated, setStoreError]);

  return { login, register, logout, isLoading, error };
};
