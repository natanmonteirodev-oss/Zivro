/**
 * Form wrapper component with standard styling
 */

import { FormHTMLAttributes, ReactNode } from 'react';

interface FormProps extends FormHTMLAttributes<HTMLFormElement> {
  children: ReactNode;
}

export const Form = ({ children, className, ...props }: FormProps) => {
  return (
    <form className={`space-y-4 ${className || ''}`} {...props}>
      {children}
    </form>
  );
};
