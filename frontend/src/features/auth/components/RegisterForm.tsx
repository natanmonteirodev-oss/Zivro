/**
 * Register Form Component
 * Extracted from RegisterPage for reusability and separation of concerns
 */

import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Input, Button, Alert } from '@/components';
import {
  validateEmail,
  validateName,
  validatePassword,
  getPasswordStrength,
} from '@/utils/validators';
import type { RegisterRequest } from '../types';

interface RegisterFormProps {
  isLoading: boolean;
  error?: string | null;
  onSubmit: (data: RegisterRequest) => Promise<void>;
  onLoginClick: () => void;
}

export const RegisterForm = ({
  isLoading,
  error,
  onSubmit,
  onLoginClick,
}: RegisterFormProps) => {
  const [alertError, setAlertError] = useState<string | null>(null);
  const [password, setPassword] = useState('');
  const [passwordStrength, setPasswordStrength] = useState<
    'weak' | 'medium' | 'strong' | null
  >(null);

  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
  } = useForm<RegisterRequest>({
    defaultValues: {
      name: '',
      email: '',
      password: '',
    },
  });

  watch('password');

  const handlePasswordChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const pwd = e.target.value;
    setPassword(pwd);
    if (pwd) {
      setPasswordStrength(getPasswordStrength(pwd));
    } else {
      setPasswordStrength(null);
    }
  };

  const handleFormSubmit = async (data: RegisterRequest) => {
    try {
      setAlertError(null);
      await onSubmit(data);
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Erro ao registrar';
      setAlertError(errorMessage);
    }
  };

  const strengthColor = {
    weak: 'text-error-500',
    medium: 'text-warning-500',
    strong: 'text-success-500',
  };

  const strengthText = {
    weak: 'Senha fraca',
    medium: 'Senha média',
    strong: 'Senha forte',
  };

  return (
    <>
      {(error || alertError) && (
        <div className="mb-6">
          <Alert
            type="error"
            title="Erro ao registrar"
            message={error || alertError || 'Erro desconhecido'}
            onClose={() => setAlertError(null)}
          />
        </div>
      )}

      <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
        <Input
          label="Nome Completo"
          placeholder="Seu nome"
          {...register('name', {
            required: 'Nome é obrigatório',
            validate: (value) =>
              validateName(value) || 'Nome deve ter no mínimo 2 caracteres',
          })}
          error={errors.name}
        />

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

        <div>
          <Input
            label="Senha"
            type="password"
            placeholder="••••••••"
            {...register('password', {
              required: 'Senha é obrigatória',
              validate: (value) =>
                validatePassword(value) ||
                'Senha deve ter no mínimo 6 caracteres',
            })}
            error={errors.password}
            onChange={(e) => {
              register('password').onChange?.(e);
              handlePasswordChange(e);
            }}
          />
          {passwordStrength && (
            <div className="mt-2">
              <div className="flex items-center gap-2">
                <div className="h-1 flex-1 bg-gray-200 rounded">
                  <div
                    className={`h-full rounded ${
                      passwordStrength === 'weak'
                        ? 'w-1/3 bg-error-500'
                        : passwordStrength === 'medium'
                          ? 'w-2/3 bg-warning-500'
                          : 'w-full bg-success-500'
                    }`}
                  />
                </div>
                <span className={`text-xs font-medium ${strengthColor[passwordStrength]}`}>
                  {strengthText[passwordStrength]}
                </span>
              </div>
              <p className="text-xs text-gray-500 mt-1">
                Dica: Use maiúsculas, números e caracteres especiais para uma
                senha mais forte.
              </p>
            </div>
          )}
        </div>

        <Button type="submit" isLoading={isLoading} className="w-full">
          Registrar
        </Button>
      </form>

      <div className="mt-6 text-center">
        <p className="text-gray-600 text-sm">
          Já tem uma conta?{' '}
          <button
            type="button"
            onClick={onLoginClick}
            className="text-primary-600 hover:text-primary-700 font-medium"
          >
            Faça login aqui
          </button>
        </p>
      </div>
    </>
  );
};
