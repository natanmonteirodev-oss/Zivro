/**
 * Main Application Component
 */

import { useEffect } from 'react';
import { RouterProvider } from 'react-router-dom';
import { router } from '@/app/router';
import { useAuth } from '@/store/authStore';

export const App = () => {
  const { hydrate } = useAuth();

  useEffect(() => {
    // Hydrate auth state from localStorage on app load
    hydrate();
  }, [hydrate]);

  return <RouterProvider router={router} />;
};
