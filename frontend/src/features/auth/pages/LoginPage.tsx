/**
 * Login Page
 * Refactored to use LoginForm component
 */

import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { LoginForm } from '../components';
import { useAuthOperations } from '../hooks';
import { useAuth } from '@/store/authStore';
import { AuthFormLayout } from '@/components/AuthFormLayout';
import type { LoginRequest } from '../types';

export const LoginPage = () => {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();
  const { login, isLoading, error } = useAuthOperations();

  // Redirect if already authenticated
  useEffect(() => {
    if (isAuthenticated) {
      navigate('/');
    }
  }, [isAuthenticated, navigate]);

  const handleSubmit = async (data: LoginRequest) => {
    await login(data);
    // Navigation will happen through the useEffect watching isAuthenticated
  };

  const handleRegisterClick = () => {
    navigate('/register');
  };

  return (
    <AuthFormLayout title="Entrar na Conta" subtitle="Bem-vindo de volta!">
      <LoginForm
        isLoading={isLoading}
        error={error}
        onSubmit={handleSubmit}
        onRegisterClick={handleRegisterClick}
      />
    </AuthFormLayout>
  );
};
