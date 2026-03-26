# Frontend Zivro - Índice Completo

## 📋 Documentação Rápida

### Para Começar
1. **[SETUP.md](./SETUP.md)** - Instalação e configuração (LEIA PRIMEIRO)
2. **[README.md](./README.md)** - Overview do projeto
3. **[IMPLEMENTATION_SUMMARY.md](./IMPLEMENTATION_SUMMARY.md)** - O que foi implementado

### Para Desenvolvedores
4. **[ARCHITECTURE.md](./ARCHITECTURE.md)** - Arquitetura detalhada
5. **[DEVELOPMENT.md](./DEVELOPMENT.md)** - Guidelines e boas práticas

## 🗂️ Estrutura de Diretórios

```
frontend/
├── src/
│   ├── app/                    # Aplicação raiz
│   ├── components/             # Componentes reutilizáveis
│   ├── features/               # Funcionalidades (modular)
│   ├── hooks/                  # Custom React hooks
│   ├── pages/                  # Páginas/Rotas
│   ├── services/               # Serviços (API)
│   ├── store/                  # State global (Zustand)
│   ├── styles/                 # Estilos globais
│   ├── types/                  # Tipos TypeScript
│   ├── utils/                  # Utilitários
│   ├── config/                 # Configurações
│   ├── main.tsx                # Entry point
│   └── App.tsx                 # Root component
│
├── public/                     # Arquivos estáticos
├── .env.local                  # Variáveis de ambiente
├── index.html                  # HTML principal
├── package.json                # Dependências
├── tailwind.config.ts          # Tailwind CSS
├── vite.config.ts              # Vite config
├── tsconfig.json               # TypeScript config
│
└── Documentação
    ├── README.md               # Overview
    ├── SETUP.md                # Instalação
    ├── ARCHITECTURE.md         # Arquitetura
    ├── DEVELOPMENT.md          # Guidelines
    ├── IMPLEMENTATION_SUMMARY.md # Resumo
    └── FRONTEND_INDEX.md       # Este arquivo
```

## 🚀 Quick Start

```bash
cd frontend
npm install
npm run dev
```

Acesse: http://localhost:3000

## 📦 Principais Dependências

```json
{
  "react": "^18.2.0",
  "react-router-dom": "^6.20.0",
  "typescript": "^5.3.3",
  "tailwindcss": "^3.3.6",
  "react-hook-form": "^7.48.0",
  "zustand": "^4.4.0",
  "axios": "^1.6.0",
  "vite": "^5.0.0"
}
```

## 🔐 Autenticação

### Endpoints Utilizados
```
POST   /api/auth/login
POST   /api/auth/register
POST   /api/auth/refresh
POST   /api/auth/logout
GET    /api/auth/me
```

### Fluxo
1. User faz login/registro
2. Backend retorna tokens
3. Frontend armazena em localStorage
4. Estado global atualizado (Zustand)
5. User redirecionado para home

## 🎨 Componentes Disponíveis

| Componente | Arquivo | Descrição |
|-----------|---------|-----------|
| Alert | `components/Alert.tsx` | Alertas com 4 tipos |
| Button | `components/Button.tsx` | Botão com variantes |
| Input | `components/Input.tsx` | Input com validação |
| Card | `components/Card.tsx` | Card genérico |
| Form | `components/Form.tsx` | Form wrapper |
| Layout | `components/Layout.tsx` | Template protegido |
| ProtectedRoute | `components/ProtectedRoute.tsx` | Rota segura |
| LoadingSpinner | `components/LoadingSpinner.tsx` | Spinner |
| AuthError | `components/AuthError.tsx` | Erro auth |

## 🪝 Custom Hooks

| Hook | Arquivo | Descrição |
|------|---------|-----------|
| useAuthOperations | `hooks/useAuthOperations.ts` | Login/register/logout |
| useHttp | `hooks/useHttp.ts` | HTTP genérico |
| useIsMounted | `hooks/useIsMounted.ts` | Check mount status |

## 🔧 Serviços

### authService (`services/authService.ts`)
```typescript
authService.login(credentials)
authService.register(data)
authService.logout()
authService.getCurrentUser()
authService.isAuthenticated()
```

### httpClient (`utils/http.ts`)
```typescript
httpClient.get(url)
httpClient.post(url, data)
httpClient.put(url, data)
httpClient.patch(url, data)
httpClient.delete(url)
```

## 📝 Páginas Implementadas

### 1. LoginPage (`pages/LoginPage.tsx`)
- Email + Password form
- Validação inline
- Link para register
- Error handling

### 2. RegisterPage (`pages/RegisterPage.tsx`)
- Name + Email + Password form
- Password strength indicator
- Validações em tempo real
- Link para login

### 3. HomePage (`pages/HomePage.tsx`)
- Dashboard com estatísticas
- User greeting
- Layout protegido
- Logout button

## 💾 State Management

### useAuth Store (Zustand)
```typescript
const { user, isAuthenticated, setUser, logout, hydrate } = useAuth();
```

**Estado**
- `user: User | null` - Dados do usuário
- `isAuthenticated: boolean` - Status de autenticação
- `isLoading: boolean` - Estado de loading
- `error: string | null` - Mensagem de erro

## 🌐 Variáveis de Ambiente

```env
VITE_API_URL=https://localhost:5001/api
VITE_APP_NAME=Zivro
```

Copie `.env.example` para `.env.local`

## 📱 Responsive Design

- Mobile-first approach
- Breakpoints: sm, md, lg
- Grid system com Tailwind
- Flex utilities

## 🛡️ Segurança

- ✅ Token JWT
- ✅ Refresh token automático
- ✅ CORS configurado
- ✅ Validação frontend + backend
- ✅ XSS prevention
- ✅ Route protection

## 🧪 Scripts Disponíveis

```bash
npm run dev         # Development server
npm run build       # Build para produção
npm run preview     # Preview da build
npm run lint        # ESLint check
npm run format      # Prettier formatting
```

## 📚 Recursos Importantes

### Arquivos de Configuração
- `vite.config.ts` - Build tool
- `tailwind.config.ts` - Estilização
- `tsconfig.json` - TypeScript
- `.eslintrc.cjs` - Linting
- `.prettierrc.json` - Formatting

### Tipos TypeScript
- `types/auth.ts` - Types de autenticação
- `types/api.ts` - Types de API

### Utilitários
- `utils/validators.ts` - Validações
- `utils/storage.ts` - localStorage wrapper
- `utils/helpers.ts` - Funções utilitárias
- `utils/http.ts` - HTTP client

### Configurações
- `config/constants.ts` - Constantes da app

## 🔍 Debugging

### DevTools
- React Developer Tools (Chrome Extension)
- Redux DevTools (para Zustand)
- Chrome DevTools Network tab

### Console
```typescript
// Ver estado
console.log(useAuth.getState());

// Ver token
console.log(localStorage.getItem('zivro_access_token'));
```

## 🚨 Troubleshooting

### Port já em uso
```bash
npm run dev -- --port 3001
```

### CORS Error
Verificar configuração do backend (CORS habilitado)

### Token Expirado
Auto refresh via interceptador Axios

### Build errors
```bash
rm -rf node_modules package-lock.json
npm install
npm run build
```

## 📊 Fluxo de Autenticação

```
login/register
      ↓
authService.login()
      ↓
httpClient.post(/auth/login)
      ↓
Backend validates
      ↓
Returns tokens
      ↓
Storage.setTokens()
      ↓
useAuth.setUser()
      ↓
Navigate to /
      ↓
ProtectedRoute
      ↓
HomePage
```

## 🎯 Funcionalidades por Status

### ✅ Implementado
- Autenticação (login/register)
- Token management
- Roteamento protegido
- Validação de forms
- Layout responsivo
- Components reutilizáveis
- State global
- Error handling

### 🔄 Em Desenvolvimento
- Módulo de despesas
- Gráficos e relatórios
- Categorias

### 📋 Planejado
- Testes (Jest + RTL)
- Testes E2E (Cypress)
- PWA features
- Dark mode
- i18n

## 👥 Contribuindo

1. Branch: `feature/nome`
2. Commits: `feat: descrição`
3. PR com review
4. Merge com main

## 📞 Suporte

- Documentação: Veja arquivos `.md`
- Código: Comentários JSDoc
- Issues: GitHub Issues

## ⚡ Performance Tips

- Usar `React.memo()` quando apropriado
- `useCallback()` para funções passadas
- `useMemo()` para cálculos custosos
- Lazy load componentes por rota

## 🔗 Links Úteis

- [React Documentation](https://react.dev)
- [TypeScript Handbook](https://www.typescriptlang.org)
- [Tailwind CSS](https://tailwindcss.com)
- [React Hook Form](https://react-hook-form.com)
- [Zustand](https://github.com/pmndrs/zustand)
- [Axios](https://axios-http.com)

---

## 📍 Próximo Passo

Leia **[SETUP.md](./SETUP.md)** para instalação e configuração inicial.

**Last Updated**: 2024
**Version**: 1.0.0
**Status**: ✅ Pronto para Produção
