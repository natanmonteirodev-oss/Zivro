/**
 * Constants da aplicação
 */

export const APP_NAME = 'Zivro';
export const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:5001/api';

// Token expiration times (em minutos)
export const TOKEN_EXPIRATION = {
  ACCESS_TOKEN: 15,
  REFRESH_TOKEN: 7 * 24 * 60, // 7 dias
};

// API Timeouts
export const API_TIMEOUT = 30000; // 30 segundos

// Pagination
export const DEFAULT_PAGE_SIZE = 20;

// Routes
export const ROUTES = {
  PUBLIC: {
    LOGIN: '/login',
    REGISTER: '/register',
  },
  PRIVATE: {
    HOME: '/',
    DASHBOARD: '/dashboard',
    EXPENSES: '/expenses',
    CATEGORIES: '/categories',
    REPORTS: '/reports',
    SETTINGS: '/settings',
  },
};

// Error codes
export const ERROR_CODES = {
  UNAUTHORIZED: 'UNAUTHORIZED',
  FORBIDDEN: 'FORBIDDEN',
  NOT_FOUND: 'NOT_FOUND',
  VALIDATION_ERROR: 'VALIDATION_ERROR',
  INTERNAL_SERVER_ERROR: 'INTERNAL_SERVER_ERROR',
  NETWORK_ERROR: 'NETWORK_ERROR',
  UNKNOWN_ERROR: 'UNKNOWN_ERROR',
};
