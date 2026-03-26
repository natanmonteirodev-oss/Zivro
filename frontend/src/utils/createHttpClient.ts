/**
 * HTTP client configuration and axios setup
 */

import axios, { AxiosInstance } from 'axios';
import { API_BASE_URL, API_TIMEOUT } from '@/config/constants';

export const createHttpClient = (): AxiosInstance => {
  return axios.create({
    baseURL: API_BASE_URL,
    timeout: API_TIMEOUT,
    headers: {
      'Content-Type': 'application/json',
    },
  });
};
