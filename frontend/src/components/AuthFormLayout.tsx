/**
 * SaaS Auth Form Layout
 * Premium Material 3 design inspired by Saasable UI
 */

import { ReactNode } from 'react';

interface AuthFormLayoutProps {
  children: ReactNode;
  title: string;
  subtitle?: string;
  footerContent?: ReactNode;
}

export const AuthFormLayout = ({
  children,
  title,
  subtitle,
  footerContent,
}: AuthFormLayoutProps) => {
  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 flex items-center justify-center px-4 py-8 relative overflow-hidden">
      {/* Decorative gradient blobs */}
      <div className="absolute top-0 left-1/2 -translate-x-1/2 w-96 h-96 bg-primary-500 rounded-full mix-blend-multiply filter blur-3xl opacity-10 animate-pulse"></div>
      <div className="absolute bottom-0 right-1/4 w-96 h-96 bg-purple-500 rounded-full mix-blend-multiply filter blur-3xl opacity-10 animate-pulse"></div>

      <div className="w-full max-w-md relative z-10">
        {/* Glassmorphism card */}
        <div className="bg-slate-800 bg-opacity-50 backdrop-blur-xl rounded-2xl shadow-2xl border border-slate-700 p-8 relative">
          {/* Header with logo */}
          <div className="text-center mb-8">
            {/* Gradient logo */}
            <div className="inline-flex items-center justify-center w-12 h-12 rounded-xl bg-gradient-to-br from-primary-500 to-primary-600 mb-4 shadow-lg shadow-primary-500/50">
              <span className="text-xl font-bold text-white">Z</span>
            </div>
            <h1 className="text-3xl font-bold bg-gradient-to-r from-primary-400 to-purple-400 bg-clip-text text-transparent mb-2">
              Zivro
            </h1>
            <h2 className="text-xl font-semibold text-white mb-1">{title}</h2>
            {subtitle && <p className="text-slate-400 text-sm">{subtitle}</p>}
          </div>

          {/* Form content */}
          <div>{children}</div>

          {/* Footer */}
          {footerContent && <div className="mt-7 text-center text-sm text-slate-400">{footerContent}</div>}
        </div>

        {/* Bottom accent line */}
        <div className="mt-6 flex justify-center">
          <div className="h-1 w-24 bg-gradient-to-r from-primary-500 to-purple-500 rounded-full"></div>
        </div>
      </div>
    </div>
  );
};
