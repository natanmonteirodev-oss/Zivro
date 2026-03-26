/**
 * Premium Input field component
 * Material 3 inspired design
 */

import { InputHTMLAttributes, forwardRef } from 'react';
import { FieldError } from 'react-hook-form';

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: FieldError;
  helperText?: string;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, helperText, className, ...props }, ref) => {
    return (
      <div className="w-full">
        {label && (
          <label className="block text-sm font-semibold text-slate-900 mb-3">
            {label}
            {props.required && <span className="text-error-500 ml-1">*</span>}
          </label>
        )}
        <div className="relative">
          <input
            ref={ref}
            className={`
              w-full px-4 py-3 text-sm font-medium rounded-lg
              border-2 transition-all duration-200
              placeholder:text-slate-400
              focus:outline-none
              ${
                error
                  ? 'border-error-300 bg-error-50 focus:border-error-500 focus:ring-2 focus:ring-error-500/30 focus:bg-white'
                  : 'border-slate-200 bg-white hover:border-slate-300 focus:border-primary-500 focus:ring-2 focus:ring-primary-500/30'
              }
              ${className}
            `}
            {...props}
          />
        </div>
        {error && (
          <div className="mt-2 flex items-start gap-2">
            <svg className="w-4 h-4 text-error-500 flex-shrink-0 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
            </svg>
            <p className="text-sm text-error-600">{error.message}</p>
          </div>
        )}
        {helperText && !error && (
          <p className="mt-2 text-sm text-slate-500">{helperText}</p>
        )}
      </div>
    );
  },
);

Input.displayName = 'Input';

