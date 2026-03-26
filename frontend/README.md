# Frontend - Zivro

Aplicação React SPA para gerenciamento de finanças pessoais.

## Tecnologias

- **React 18** - UI framework
- **React Router v6** - Roteamento
- **TypeScript** - Type safety
- **Vite** - Build tool moderno
- **Tailwind CSS** - Estilização
- **React Hook Form** - Gerenciamento de formulários
- **Zustand** - State management
- **Axios** - HTTP client

## Estrutura de Pastas

```
src/
├── app/              # Aplicação principal (router, App)
├── components/       # Componentes reutilizáveis
├── features/         # Funcionalidades por domínio
│   └── auth/         # Módulo de autenticação
├── hooks/            # Custom hooks
├── pages/            # Páginas da aplicação
├── services/         # Serviços (API calls)
├── store/            # State management (Zustand)
├── styles/           # Estilos globais
├── types/            # Tipos TypeScript
├── utils/            # Funções utilitárias
├── main.tsx          # Entry point
└── App.tsx           # Root component
```

## Setup

### Instalação de Dependências

```bash
cd frontend
npm install
```

### Variáveis de Ambiente

Crie um arquivo `.env.local` baseado em `.env.example`:

```bash
VITE_API_URL=https://localhost:5001/api
VITE_APP_NAME=Zivro
```

### Desenvolvimento

```bash
npm run dev
```

A aplicação estará disponível em `http://localhost:3000`

### Build para Produção

```bash
npm run build
```

### Linting

```bash
npm run lint
```

### Formatação de Código

```bash
npm run format
```

## Funcionalidades Implementadas

### Autenticação

- ✅ Login com email/senha
- ✅ Registro de novo usuário
- ✅ Validação de formulários
- ✅ Token refresh automático
- ✅ Logout

### Segurança

- ✅ JWT token na localStorage
- ✅ Headers de autorização automáticos
- ✅ Roteamento protegido
- ✅ Tratamento de erros de autenticação

### UI/UX

- ✅ Design responsivo (mobile-first)
- ✅ Validação em tempo real
- ✅ Mensagens de erro informativas
- ✅ Loading states
- ✅ Dark theme ready (Tailwind)

## Integração com Backend

A aplicação se conecta aos endpoints de autenticação do backend:

- `POST /api/auth/login` - Login
- `POST /api/auth/register` - Registro
- `POST /api/auth/refresh` - Renovar token
- `POST /api/auth/logout` - Logout

## Boas Práticas Implementadas

1. **Type Safety** - Uso extensivo de TypeScript
2. **Component Organization** - Estrutura clara e escalável
3. **Error Handling** - Tratamento robusto de erros
4. **State Management** - Zustand para state global
5. **API Integration** - Axios com interceptadores
6. **Form Handling** - React Hook Form para validações
7. **Responsive Design** - Mobile-first approach
8. **Code Quality** - ESLint, Prettier, TypeScript strict mode

## Próximas Funcionalidades

- [ ] Módulo de Despesas (listar, criar, editar, deletar)
- [ ] Categorias de despesas
- [ ] Gráficos e relatórios
- [ ] Filtros e busca
- [ ] Sistema de notificações
- [ ] Testes unitários
- [ ] Testes E2E
