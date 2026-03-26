/**
 * Custom hook to detect if component is mounted
 * Useful to prevent state updates on unmounted components
 */

import { useEffect, useRef, useCallback } from 'react';

export const useIsMounted = () => {
  const isMountedRef = useRef(true);

  useEffect(() => {
    return () => {
      isMountedRef.current = false;
    };
  }, []);

  return useCallback(() => isMountedRef.current, []);
};
