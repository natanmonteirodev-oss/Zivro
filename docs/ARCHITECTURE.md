# Arquitetura Frontend - Zivro

## Visão Geral

Frontend desenvolvido em **React 18** com **TypeScript**, seguindo **Clean Architecture** e **Feature-based structure**. A aplicação é uma SPA (Single Page Application) focada em autenticação e gerenciamento de finanças pessoais.

## Stack Técnico

### Tecnologias Principais

```
Frontend Framework:    React 18
Language:             TypeScript (strict mode)
Build Tool:           Vite
State Management:     Zustand
HTTP Client:          Axios
Form Management:      React Hook Form
Styling:              Tailwind CSS
Routing:              React Router v6
Linting:              ESLint
Code Formatting:      Prettier
```

## Estrutura de Diretórios

```
frontend/
├── src/
│   ├── app/                    # Aplicação raiz
│   │   ├── App.tsx            # Root component
│   │   └── router.tsx         # Configuração de rotas
│   │
│   ├── components/             # Componentes reutilizáveis
│   │   ├── Alert.tsx          # Componente de alertas
│   │   ├── AuthError.tsx       # Erro autenticação
│   │   ├── Button.tsx         # Botão genérico
│   │   ├── Card.tsx           # Card genérico
│   │   ├── Form.tsx           # Form wrapper
│   │   ├── Input.tsx          # Input com validação
│   │   ├── Layout.tsx         # Layout principal
│   │   ├── LoadingSpinner.tsx # Spinner de loading
│   │   ├── ProtectedRoute.tsx # Rota protegida
│   │   └── index.ts           # Barrel export
│   │
│   ├── features/               # Funcionalidades específicas
│   │   └── auth/              # Módulo de autenticação
│   │       ├── components/
│   │       ├── hooks/
│   │       ├── pages/
│   │       └── services/
│   │
│   ├── hooks/                  # Custom React hooks
│   │   ├── useAuthOperations.ts
│   │   ├── useHttp.ts
│   │   └── useIsMounted.ts
│   │
│   ├── pages/                  # Páginas/Rotas
│   │   ├── HomePage.tsx
│   │   ├── LoginPage.tsx
│   │   └── RegisterPage.tsx
│   │
│   ├── services/               # Serviços (API, autenticação)
│   │   └── authService.ts
│   │
│   ├── store/                  # State management (Zustand)
│   │   └── authStore.ts
│   │
│   ├── styles/                 # Estilos globais
│   │   └── global.css
│   │
│   ├── types/                  # Tipos TypeScript
│   │   ├── api.ts
│   │   └── auth.ts
│   │
│   ├── utils/                  # Utilities
│   │   ├── createHttpClient.ts
│   │   ├── helpers.ts
│   │   ├── http.ts            # HTTP client configurado
│   │   ├── storage.ts         # localStorage wrapper
│   │   └── validators.ts      # Validações
│   │
│   ├── config/                # Configurações
│   │   └── constants.ts
│   │
│   ├── main.tsx               # Entry point
│   └── App.tsx                # Root component
│
├── public/                    # Arquivos estáticos
│
├── .env.example               # Variáveis de ambiente (exemplo)
├── .env.local                 # Variáveis de ambiente (local)
├── .eslintrc.cjs             # ESLint config
├── .gitignore                # Git ignore
├── .prettierrc.json          # Prettier config
├── index.html                # HTML principal
├── package.json              # Dependências
├── postcss.config.js         # PostCSS config
├── tailwind.config.ts        # Tailwind config
├── tsconfig.json             # TypeScript config
├── vite.config.ts            # Vite config
│
├── DEVELOPMENT.md            # Guidelines de desenvolvimento
├── README.md                 # Documentação
└── ARCHITECTURE.md           # Este arquivo
```

## Fluxo de Autenticação

```
┌─────────────────────────────────────────────────────────┐
│  1. User visits /login or /register                      │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  2. Form submission (email + password)                   │
│     ↓                                                    │
│  3. useAuthOperations.ts → calls authService            │
│     ↓                                                    │
│  4. authService → API call (POST /auth/login)           │
│     ↓                                                    │
│  5. Backend validates credentials                       │
│     ↓                                                    │
│  6. Backend returns: accessToken + refreshToken         │
│     ↓                                                    │
│  7. Frontend stores tokens in localStorage              │
│     ↓                                                    │
│  8. Zustand store updates (isAuthenticated = true)      │
│     ↓                                                    │
│  9. Navigate to Home page (/)                           │
│     ↓                                                    │
│  10. ProtectedRoute allows access                       │
│     ↓                                                    │
│  11. Show HomePage with user data                       │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

## Segurança Implementada

### Token Management

1. **Access Token** (JWT de curta duração - 15 min)
   - Armazenado em localStorage
   - Enviado em cada request no header `Authorization: Bearer <token>`
   - Usado para autorizar requisições

2. **Refresh Token** (Token de longa duração - 7 dias)
   - Armazenado em localStorage (de forma segura)
   - Usado para renovar access token quando expirado
   - Automático via interceptador Axios

### Proteção de Rotas

- `ProtectedRoute` verifica `isAuthenticated` antes de renderizar
- Redireciona para `/login` se não autenticado
- `useAuth()` hidrata estado ao carregar a página

### Validação

- Validação frontend com React Hook Form
- Validação de email, senha, nome
- Feedback em tempo real ao usuário

## State Management (Zustand)

```typescript
// Exemplo de uso
const { user, isAuthenticated, logout } = useAuth();

// Store persistido em localStorage
// Hidratação automática na inicialização
```

### Estado Armazenado

```javascript
{
  user: User | null,
  isAuthenticated: boolean,
  isLoading: boolean,
  error: string | null
}
```

## HTTP Client (Axios)

### Interceptadores

1. **Request Interceptor**
   - Adiciona token JWT no header Authorization
   - Pode adicionar headers adicionais

2. **Response Interceptor**
   - Trata erros 401 (token expirado)
   - Tenta renovar token automaticamente
   - Redireciona para login se token inválido

### Tratamento de Erros

```typescript
try {
  const response = await authService.login(credentials);
} catch (error) {
  // error.response.data.message
  // error.response.status
}
```

## Componentes Principais

### ProtectedRoute
Componente HOC que protege rotas autenticadas

### Layout
Template para páginas autenticadas com header e logout

### Input
Input form com validação integrada ao React Hook Form

### Button
Botão com estados: loading, disabled, variantes (primary, secondary, danger)

### Alert
Componente para exibir mensagens (error, success, warning, info)

### Form
Wrapper para formulários com spacing e validação

## Hooks Customizados

### useAuthOperations
Login, registro e logout com gerenciamento de estado

### useHttp
Wrapper genérico para chamadas HTTP com loading/error

### useIsMounted
Validar se componente está montado antes de atualizar estado

## Validações

```typescript
// Email validation
validateEmail(email): boolean

// Password validation
validatePassword(password): boolean

// Name validation  
validateName(name): boolean

// Password strength
getPasswordStrength(password): 'weak' | 'medium' | 'strong'
```

## Styling com Tailwind CSS

### Tema de Cores

- **Primary**: Azul (para ações principais)
- **Success**: Verde (sucesso)
- **Error**: Vermelho (erros)
- **Warning**: Laranja (avisos)

### Utilitários Customizados

```css
@apply rounded-lg       /* Border radius padrão */
@apply bg-primary-600   /* Cor primária */
@apply text-primary-100 /* Texto primário claro */
```

## Build e Deployment

### Desenvolvimento

```bash
npm run dev
```

- Vite dev server em http://localhost:3000
- Hot reload automático
- Source maps para debug

### Produção

```bash
npm run build
```

- Minificação e otimização
- Tree-shaking de código não utilizado
- Chunks otimizados

## Próximos Passos

### Funcionalidades Planejadas

1. **Módulo de Despesas**
   - Listar despesas
   - Criar/editar/deletar despesas
   - Categorias

2. **Relatórios e Gráficos**
   - Dashboard com métricas
   - Gráficos de gastos por categoria
   - Comparação mensal

3. **Notificações**
   - Toast notifications
   - Email notifications
   - Push notifications

4. **Testes**
   - Testes unitários (Jest + React Testing Library)
   - Testes E2E (Cypress/Playwright)
   - Coverage reports

5. **Performance**
   - Code splitting por rota
   - Lazy loading de componentes
   - Otimização de imagens

## Referências e Recursos

- [React Documentation](https://react.dev)
- [TypeScript Handbook](https://www.typescriptlang.org/docs)
- [Tailwind CSS](https://tailwindcss.com)
- [React Router v6](https://reactrouter.com)
- [React Hook Form](https://react-hook-form.com)
- [Zustand Documentation](https://github.com/pmndrs/zustand)
- [Axios Documentation](https://axios-http.com)
- [Vite Documentation](https://vitejs.dev)
