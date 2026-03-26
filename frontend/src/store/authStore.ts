/**
 * Global state management using Zustand
 * Handles authentication state and user information
 */

import { create } from 'zustand';
import { devtools, persist } from 'zustand/middleware';
import type { User } from '@/features/auth';
import { storage } from '@/utils/storage';

interface AuthStore {
  // State
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;

  // Actions
  setUser: (user: User | null) => void;
  setIsAuthenticated: (isAuthenticated: boolean) => void;
  setIsLoading: (isLoading: boolean) => void;
  setError: (error: string | null) => void;
  logout: () => void;
  hydrate: () => void;
}

export const useAuth = create<AuthStore>()(
  devtools(
    persist(
      (set) => ({
        user: null,
        isAuthenticated: false,
        isLoading: false,
        error: null,

        setUser: (user) => set({ user }),
        setIsAuthenticated: (isAuthenticated) => set({ isAuthenticated }),
        setIsLoading: (isLoading) => set({ isLoading }),
        setError: (error) => set({ error }),

        logout: () => {
          storage.clear();
          set({ user: null, isAuthenticated: false, error: null });
        },

        hydrate: () => {
          const token = storage.getAccessToken();
          const user = storage.getUser();
          set({
            isAuthenticated: !!token,
            user: user || null,
          });
        },
      }),
      {
        name: 'auth-store',
        partialize: (state) => ({
          user: state.user,
          isAuthenticated: state.isAuthenticated,
        }),
      },
    ),
  ),
);
