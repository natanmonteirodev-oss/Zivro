/**
 * Register Page
 * Refactored to use RegisterForm component
 */

import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { RegisterForm } from '../components';
import { useAuthOperations } from '../hooks';
import { useAuth } from '@/store/authStore';
import { AuthFormLayout } from '@/components/AuthFormLayout';
import type { RegisterRequest } from '../types';

export const RegisterPage = () => {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();
  const { register, isLoading, error } = useAuthOperations();

  // Redirect if already authenticated
  useEffect(() => {
    if (isAuthenticated) {
      navigate('/');
    }
  }, [isAuthenticated, navigate]);

  const handleSubmit = async (data: RegisterRequest) => {
    await register(data);
    // Navigation will happen through the useEffect watching isAuthenticated
  };

  const handleLoginClick = () => {
    navigate('/login');
  };

  return (
    <AuthFormLayout
      title="Criar Conta"
      subtitle="Junte-se a centenas de usuários"
    >
      <RegisterForm
        isLoading={isLoading}
        error={error}
        onSubmit={handleSubmit}
        onLoginClick={handleLoginClick}
      />
    </AuthFormLayout>
  );
};
