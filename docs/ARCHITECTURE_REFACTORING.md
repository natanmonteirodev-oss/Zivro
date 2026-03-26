# 🏗️ Refatoração da Arquitetura - Auth Module

## Alterações Realizadas

### ❌ Antes (Estrutura Incorreta)
```
src/
├── pages/
│   ├── LoginPage.tsx          ❌ Errado
│   ├── RegisterPage.tsx       ❌ Errado
│   └── HomePage.tsx           ✅ Certo (app-wide)
├── hooks/
│   └── useAuthOperations.ts   ❌ Errado
├── services/
│   └── authService.ts         ❌ Errado
├── store/
│   └── authStore.ts           (global, pode ficar)
├── types/
│   └── auth.ts                ❌ Errado
└── features/auth/
    ├── components/ (vazio)    ❌ Não usado
    ├── hooks/ (vazio)         ❌ Não usado
    ├── pages/ (vazio)         ❌ Não usado
    └── services/ (vazio)      ❌ Não usado
```

**Problema:** Violava o padrão **feature-based architecture** prometido.

---

### ✅ Depois (Estrutura Correta)
```
src/
├── features/
│   └── auth/                          ✅ Feature container
│       ├── components/
│       │   ├── LoginForm.tsx          ✅ Login form component
│       │   ├── RegisterForm.tsx       ✅ Register form component
│       │   └── index.ts               ✅ Barrel export
│       ├── hooks/
│       │   ├── useAuthOperations.ts   ✅ Auth business logic
│       │   └── index.ts               ✅ Barrel export
│       ├── pages/
│       │   ├── LoginPage.tsx          ✅ Page component
│       │   ├── RegisterPage.tsx       ✅ Page component
│       │   └── index.ts               ✅ Barrel export
│       ├── services/
│       │   ├── authService.ts         ✅ API operations
│       │   └── index.ts               ✅ Barrel export
│       ├── types/
│       │   └── index.ts               ✅ Type definitions
│       └── index.ts                   ✅ Feature barrel export
├── pages/
│   ├── LoginPage.tsx          (re-export deprecado)
│   ├── RegisterPage.tsx       (re-export deprecado)
│   └── HomePage.tsx           ✅ App-wide page
├── hooks/
│   ├── useAuthOperations.ts   (re-export deprecado)
│   ├── useHttp.ts             ✅ Generic HTTP hook
│   └── useIsMounted.ts        ✅ Mount status hook
├── services/
│   └── authService.ts         (re-export deprecado)
├── store/
│   └── authStore.ts           ✅ Global Zustand store
├── components/                ✅ Global UI components
├── types/
│   ├── auth.ts                (re-export deprecado)
│   └── api.ts                 ✅ Generic API types
└── utils/
```

---

## 🎯 Benefícios da Refatoração

### 1. **Clean Architecture**
- ✅ Cada feature é auto-contida em sua própria pasta
- ✅ Fácil localizar código relacionado a auth
- ✅ Escalável para adicionar novas features

### 2. **Separation of Concerns**
- ✅ **Components**: UI-specific (LoginForm, RegisterForm)
- ✅ **Pages**: Route-level components
- ✅ **Services**: API operations (HTTP)
- ✅ **Hooks**: Business logic e state management
- ✅ **Types**: Type definitions específicas da feature

### 3. **Reusability**
- ✅ LoginForm e RegisterForm podem ser reutilizadas em modais
- ✅ useAuthOperations pode ser usado em qualquer componente
- ✅ authService expõe métodos puros (não depende de React)

### 4. **Maintainability**
- ✅ Imports claros com barrel exports (`import { LoginPage } from '@/features/auth'`)
- ✅ Estrutura auto-explicativa
- ✅ Compatibilidade com arquivos legados via re-exports

---

## 📦 Como Usar a Feature Auth

### Importação Recomendada
```typescript
// ✅ NOVO (Recomendado)
import { LoginPage, RegisterPage } from '@/features/auth';
import { useAuthOperations } from '@/features/auth';
import { authService } from '@/features/auth';
import type { LoginRequest, User } from '@/features/auth';
```

### Importação Antiga (Deprecada)
```typescript
// ❌ ANTIGO (Evitar - mantém compatibilidade temporária)
import { LoginPage } from '@/pages/LoginPage';
import { useAuthOperations } from '@/hooks/useAuthOperations';
import { authService } from '@/services/authService';
import type { LoginRequest } from '@/types/auth';
```

---

## 🔄 Compatibilidade

Arquivos antigos foram convertidos em **re-exports** para manter compatibilidade:

- ✅ `src/pages/LoginPage.tsx` → re-export de `@/features/auth`
- ✅ `src/pages/RegisterPage.tsx` → re-export de `@/features/auth`
- ✅ `src/hooks/useAuthOperations.ts` → re-export de `@/features/auth`
- ✅ `src/services/authService.ts` → re-export de `@/features/auth`
- ✅ `src/types/auth.ts` → re-export de `@/features/auth`

**Nota:** Mesmo usando os caminhos antigos, o código será importado das novas locações. É recomendado atualizar gradualmente para os novos caminhos.

---

## 📊 Estrutura Interna da Feature Auth

### `features/auth/types/index.ts`
```typescript
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

export interface AuthResponse {
  userId: string;
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

export interface User {
  id: string;
  name: string;
  email: string;
  isActive: boolean;
  createdAt: string;
}
```

### `features/auth/services/authService.ts`
```typescript
class AuthService {
  async login(credentials: LoginRequest): Promise<AuthResponse>
  async register(data: RegisterRequest): Promise<AuthResponse>
  async logout(): Promise<void>
  async getCurrentUser(): Promise<User | null>
  isAuthenticated(): boolean
}

export const authService = new AuthService();
```

### `features/auth/hooks/useAuthOperations.ts`
```typescript
export const useAuthOperations = (): UseAuthOperations => {
  const login = async (credentials: LoginRequest) => Promise<void>
  const register = async (data: RegisterRequest) => Promise<void>
  const logout = async () => Promise<void>
  return { login, register, logout, isLoading, error }
}
```

### `features/auth/components/LoginForm.tsx`
- Componente reutilizável de formulário de login
- Props: `isLoading`, `error`, `onSubmit`, `onRegisterClick`
- Integrado com React Hook Form

### `features/auth/components/RegisterForm.tsx`
- Componente reutilizável de formulário de registro
- Props: `isLoading`, `error`, `onSubmit`, `onLoginClick`
- Indicador de força de senha integrado

### `features/auth/pages/LoginPage.tsx`
- Página de login (route `/login`)
- Usa `LoginForm` internamente
- Redireciona se já autenticado

### `features/auth/pages/RegisterPage.tsx`
- Página de registro (route `/register`)
- Usa `RegisterForm` internamente
- Redireciona se já autenticado

---

## 🔗 Integração com Global Store

A feature auth também integra-se com o global store (`@/store/authStore.ts`):

```typescript
import { useAuth } from '@/store/authStore';

export const useAuthOperations = () => {
  const { setUser, setIsAuthenticated, setError } = useAuth();
  // ... resto da lógica
}
```

**Nota:** O `authStore` permanece na raiz (`src/store/`) porque é consumido por múltiplos módulos (Layout, ProtectedRoute, App, etc). É um global state, não específico da feature.

---

## 📋 Checklist de Migração

Se você tem código usando os caminhos antigos:

- [ ] Trocar `import { LoginPage } from '@/pages/LoginPage'` → `import { LoginPage } from '@/features/auth'`
- [ ] Trocar `import { RegisterPage } from '@/pages/RegisterPage'` → `import { RegisterPage } from '@/features/auth'`
- [ ] Trocar `import { useAuthOperations } from '@/hooks/useAuthOperations'` → `import { useAuthOperations } from '@/features/auth'`
- [ ] Trocar `import { authService } from '@/services/authService'` → `import { authService } from '@/features/auth'`
- [ ] Trocar `import type { LoginRequest } from '@/types/auth'` → `import type { LoginRequest } from '@/features/auth'`

---

## 🚀 Próximos Passos

1. **Remover re-exports** (quando todos os imports foram atualizados)
2. **Criar outras features** (expenses, categories, reports)
3. **Adicionar testes** (Unit + Integration)
4. **Documentar padrão** para novas features

---

## 📚 Referências

- **Clean Architecture**: Cada feature é independente
- **Module Pattern**: Barrel exports para imports limpos
- **Separation of Concerns**: Components, Services, Hooks, Types
- **Feature-First Structure**: Escalável, maintenível, testável

