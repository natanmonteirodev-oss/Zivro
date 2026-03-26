/**
 * Common utility functions
 */

/**
 * Classe para tratamento de erros da API
 */
export class ApiError extends Error {
  constructor(
    public statusCode: number,
    public code: string,
    message: string,
  ) {
    super(message);
    this.name = 'ApiError';
  }

  static fromResponse(response: any): ApiError {
    const statusCode = response.status || 500;
    const code = response.data?.code || 'UNKNOWN_ERROR';
    const message = response.data?.message || 'Um erro desconhecido ocorreu';
    return new ApiError(statusCode, code, message);
  }
}

/**
 * Formata um valor monetário em BRL
 */
export const formatCurrency = (value: number): string => {
  return new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  }).format(value);
};

/**
 * Formata uma data
 */
export const formatDate = (date: string | Date, format: 'short' | 'long' = 'short'): string => {
  const dateObj = typeof date === 'string' ? new Date(date) : date;
  return new Intl.DateTimeFormat('pt-BR', {
    year: 'numeric',
    month: format === 'long' ? 'long' : '2-digit',
    day: '2-digit',
  }).format(dateObj);
};

/**
 * Delay em ms (útil para simular loading)
 */
export const delay = (ms: number): Promise<void> => {
  return new Promise((resolve) => setTimeout(resolve, ms));
};

/**
 * Valida se um objeto vazio
 */
export const isEmpty = (obj: any): boolean => {
  return Object.keys(obj).length === 0;
};

/**
 * Clona um objeto (deep copy)
 */
export const deepClone = <T>(obj: T): T => {
  return JSON.parse(JSON.stringify(obj));
};
