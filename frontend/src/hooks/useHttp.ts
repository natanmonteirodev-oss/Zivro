/**
 * Custom hook for HTTP requests with loading and error states
 */

import { useState, useCallback } from 'react';
import { AxiosError } from 'axios';

interface UseHttpState<T> {
  data: T | null;
  isLoading: boolean;
  error: AxiosError | null;
}

interface UseHttpOptions {
  onSuccess?: (data: any) => void;
  onError?: (error: AxiosError) => void;
}

export const useHttp = <T = any>(
  options?: UseHttpOptions,
): UseHttpState<T> & {
  execute: (promise: Promise<any>) => Promise<any>;
} => {
  const [state, setState] = useState<UseHttpState<T>>({
    data: null,
    isLoading: false,
    error: null,
  });

  const execute = useCallback(
    async (promise: Promise<any>) => {
      try {
        setState({ data: null, isLoading: true, error: null });
        const response = await promise;
        setState({ data: response.data, isLoading: false, error: null });
        options?.onSuccess?.(response.data);
        return response.data;
      } catch (error) {
        const axiosError = error as AxiosError;
        setState({ data: null, isLoading: false, error: axiosError });
        options?.onError?.(axiosError);
        throw axiosError;
      }
    },
    [options],
  );

  return { ...state, execute };
};
