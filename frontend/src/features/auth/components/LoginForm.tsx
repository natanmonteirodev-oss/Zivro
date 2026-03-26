/**
 * Login Form Component
 * Extracted from LoginPage for reusability and separation of concerns
 */

import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Input, Button, Alert } from '@/components';
import { validateEmail } from '@/utils/validators';
import type { LoginRequest } from '../types';

interface LoginFormProps {
  isLoading: boolean;
  error?: string | null;
  onSubmit: (data: LoginRequest) => Promise<void>;
  onRegisterClick: () => void;
}

export const LoginForm = ({
  isLoading,
  error,
  onSubmit,
  onRegisterClick,
}: LoginFormProps) => {
  const [alertError, setAlertError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginRequest>({
    defaultValues: {
      email: '',
      password: '',
    },
  });

  const handleFormSubmit = async (data: LoginRequest) => {
    try {
      setAlertError(null);
      await onSubmit(data);
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Erro ao fazer login';
      setAlertError(errorMessage);
    }
  };

  return (
    <>
      {(error || alertError) && (
        <div className="mb-6">
          <Alert
            type="error"
            title="Erro ao fazer login"
            message={error || alertError || 'Erro desconhecido'}
            onClose={() => setAlertError(null)}
          />
        </div>
      )}

      <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
        <Input
          label="Email"
          type="email"
          placeholder="seu@email.com"
          {...register('email', {
            required: 'Email é obrigatório',
            validate: (value) => validateEmail(value) || 'Email inválido',
          })}
          error={errors.email}
        />

        <Input
          label="Senha"
          type="password"
          placeholder="••••••••"
          {...register('password', {
            required: 'Senha é obrigatória',
            minLength: {
              value: 6,
              message: 'Senha deve ter no mínimo 6 caracteres',
            },
          })}
          error={errors.password}
        />

        <Button type="submit" isLoading={isLoading} className="w-full">
          Entrar
        </Button>
      </form>

      <div className="mt-6 text-center">
        <p className="text-gray-600 text-sm">
          Ainda não tem uma conta?{' '}
          <button
            type="button"
            onClick={onRegisterClick}
            className="text-primary-600 hover:text-primary-700 font-medium"
          >
            Registre-se aqui
          </button>
        </p>
      </div>
    </>
  );
};
