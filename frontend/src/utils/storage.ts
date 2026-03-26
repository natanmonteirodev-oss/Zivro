/**
 * Storage utility functions for token management
 */

const KEYS = {
  ACCESS_TOKEN: 'zivro_access_token',
  REFRESH_TOKEN: 'zivro_refresh_token',
  USER: 'zivro_user',
};

export const storage = {
  getAccessToken: () => localStorage.getItem(KEYS.ACCESS_TOKEN),
  setAccessToken: (token: string) => localStorage.setItem(KEYS.ACCESS_TOKEN, token),
  removeAccessToken: () => localStorage.removeItem(KEYS.ACCESS_TOKEN),

  getRefreshToken: () => localStorage.getItem(KEYS.REFRESH_TOKEN),
  setRefreshToken: (token: string) => localStorage.setItem(KEYS.REFRESH_TOKEN, token),
  removeRefreshToken: () => localStorage.removeItem(KEYS.REFRESH_TOKEN),

  getUser: () => {
    const user = localStorage.getItem(KEYS.USER);
    return user ? JSON.parse(user) : null;
  },
  setUser: (user: any) => localStorage.setItem(KEYS.USER, JSON.stringify(user)),
  removeUser: () => localStorage.removeItem(KEYS.USER),

  clear: () => {
    localStorage.removeItem(KEYS.ACCESS_TOKEN);
    localStorage.removeItem(KEYS.REFRESH_TOKEN);
    localStorage.removeItem(KEYS.USER);
  },
};
