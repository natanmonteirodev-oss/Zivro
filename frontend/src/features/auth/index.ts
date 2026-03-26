/**
 * Auth Feature Index
 * Main barrel export for the auth feature
 * Usage: import { LoginPage, RegisterPage, useAuthOperations, authService } from '@/features/auth'
 */

// Pages
export { LoginPage, RegisterPage } from './pages';

// Hooks
export { useAuthOperations } from './hooks';

// Services
export { authService } from './services';

// Types
export type { LoginRequest, RegisterRequest, AuthResponse, User } from './types';
