/**
 * Auth error message component
 * Specific component for displaying auth-related errors
 */

import { useState } from 'react';
import { Alert } from './Alert';

interface AuthErrorProps {
  error?: string | null;
  code?: string;
  onDismiss?: () => void;
}

export const AuthError = ({ error, code, onDismiss }: AuthErrorProps) => {
  const [isVisible, setIsVisible] = useState(!!error);

  const handleClose = () => {
    setIsVisible(false);
    onDismiss?.();
  };

  if (!isVisible || !error) {
    return null;
  }

  const getErrorTitle = (errorCode?: string) => {
    switch (errorCode) {
      case 'VALIDATION_ERROR':
        return 'Erro de Validação';
      case 'UNAUTHORIZED':
        return 'Credenciais Inválidas';
      case 'EMAIL_ALREADY_EXISTS':
        return 'Email já Registrado';
      default:
        return 'Erro';
    }
  };

  return (
    <Alert
      type="error"
      title={getErrorTitle(code)}
      message={error}
      onClose={handleClose}
    />
  );
};
