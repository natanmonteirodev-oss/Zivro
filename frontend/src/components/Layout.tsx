/**
 * Layout component
 * Main template for authenticated pages
 */

import { ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/store/authStore';
import { useAuthOperations } from '@/features/auth';

interface LayoutProps {
  children: ReactNode;
}

export const Layout = ({ children }: LayoutProps) => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const { logout } = useAuthOperations();

  const handleLogout = async () => {
    try {
      await logout();
      navigate('/login');
    } catch (error) {
      console.error('Logout error:', error);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
      {/* Premium Glassmorphism Navbar */}
      <header className="sticky top-0 z-50 bg-white bg-opacity-95 backdrop-blur-md border-b border-slate-200 shadow-sm">
        <nav className="mx-auto max-w-7xl px-6 py-4">
          <div className="flex items-center justify-between">
            {/* Logo Section */}
            <div className="flex items-center gap-3 cursor-pointer hover:opacity-80 transition" onClick={() => navigate('/')}>
              <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-primary-600 to-primary-700 flex items-center justify-center shadow-lg">
                <span className="text-lg font-bold text-white">Z</span>
              </div>
              <h1 className="text-2xl font-bold bg-gradient-to-r from-primary-600 to-primary-700 bg-clip-text text-transparent">
                Zivro
              </h1>
            </div>

            {/* Right Section */}
            <div className="flex items-center gap-6">
              {user && (
                <div className="flex items-center gap-3 px-4 py-2 rounded-xl bg-slate-50 border border-slate-200 hover:border-primary-300 transition">
                  <div className="w-10 h-10 rounded-full bg-gradient-to-br from-primary-500 to-primary-600 flex items-center justify-center ring-2 ring-white shadow-md">
                    <span className="text-sm font-semibold text-white">
                      {user.name?.charAt(0).toUpperCase()}
                    </span>
                  </div>
                  <div className="flex flex-col">
                    <span className="text-sm font-semibold text-slate-900">{user.name}</span>
                    <span className="text-xs text-slate-500">{user.email}</span>
                  </div>
                </div>
              )}
              <button
                onClick={handleLogout}
                className="px-5 py-2 rounded-lg bg-gradient-to-r from-error-500 to-error-600 text-white text-sm font-medium hover:shadow-lg hover:shadow-error-500/20 transition-all duration-200 hover:scale-105 active:scale-95"
              >
                Sair
              </button>
            </div>
          </div>
        </nav>
      </header>

      {/* Main Content */}
      <main className="mx-auto max-w-7xl px-6 py-12">{children}</main>
    </div>
  );
};
