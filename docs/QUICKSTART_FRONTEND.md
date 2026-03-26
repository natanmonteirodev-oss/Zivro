# Comece Aqui! 🚀

## O que foi desenvolvido?

Aplicação **React completa com autenticação funcional** integrada ao backend .NET.

### Stack Utilizado

```
Frontend:     React 18 + TypeScript
Styling:      Tailwind CSS
State:        Zustand
HTTP:         Axios
Roteamento:   React Router v6
Build:        Vite
Forms:        React Hook Form
```

---

## ⚡ Início Rápido (5 minutos)

### 1️⃣ Instalar Dependências

```bash
cd frontend
npm install
```

### 2️⃣ Configurar Variáveis

O arquivo `.env.local` já está pronto:

```env
VITE_API_URL=https://localhost:5001/api
VITE_APP_NAME=Zivro
```

Se precisar alterar a URL do backend, edite este arquivo.

### 3️⃣ Iniciar Desenvolvimento

```bash
npm run dev
```

Acesse: **http://localhost:3000**

### 4️⃣ Testar Autenticação

**Registrar:**
1. Clique em "Registre-se aqui"
2. Preencha nome, email e senha
3. Clique em "Registrar"
4. Será redirecionado para o dashboard

**Fazer Login:**
1. Use as credenciais que acabou de criar
2. Clique em "Entrar"
3. Acesso ao dashboard

---

## 📁 Estrutura Importante

```
frontend/
├── src/
│   ├── pages/              # Páginas (LoginPage, RegisterPage, HomePage)
│   ├── components/         # Componentes reutilizáveis
│   ├── services/           # authService (chamadas da API)
│   ├── store/              # useAuth (estado global)
│   ├── hooks/              # useAuthOperations (lógica de auth)
│   └── utils/              # helpers, validators, http client
│
├── SETUP.md                # Guia de instalação detalhado
├── ARCHITECTURE.md         # Arquitetura completa
├── DEVELOPMENT.md          # Guidelines de desenvolvimento
└── FRONTEND_INDEX.md       # Índice completo
```

---

## 🎯 Funcionalidades

### ✅ Login
- Email + Senha
- Validação em tempo real
- Tratamento de erros backend
- Redirecionamento para home

### ✅ Registro
- Nome + Email + Senha
- Indicador de força de senha
- Validações automáticas
- Link para login

### ✅ Dashboard (Home)
- Greeting personalizado
- Cards com estatísticas
- Botão de logout
- Layout profissional

### ✅ Segurança
- JWT tokens
- Auto refresh de tokens
- Sessão protegida
- Logout limpo

---

## 🔧 Comandos Disponíveis

```bash
# Desenvolvimento (hot reload)
npm run dev

# Build para produção
npm run build

# Preview de build
npm run preview

# Linting
npm run lint

# Formatação de código
npm run format
```

---

## 📋 Checklist de Setup

- [ ] `npm install` executado
- [ ] `.env.local` configurado
- [ ] Backend rodando em `https://localhost:5001`
- [ ] `npm run dev` iniciado
- [ ] Acessou `http://localhost:3000`
- [ ] Testou login/registro

---

## 🔐 Endpoints de Auth Utilizados

```
POST   /api/auth/login       # Login
POST   /api/auth/register    # Registro
POST   /api/auth/refresh     # Renovar token (automático)
POST   /api/auth/logout      # Logout
GET    /api/auth/me          # Dados do usuário
```

O frontend já envia os headers corretos automaticamente!

---

## 🛠️ Troubleshooting Rápido

### "Port 3000 já está em uso"
```bash
npm run dev -- --port 3001
```

### "CORS error - Access blocked"
Bacjack que CORS está habilitado no backend (Program.cs)

### "Cannot GET /api/auth/login"
Backend não está rodando. Execute:
```bash
cd backend
dotnet run --project src/Zivro.API/Zivro.API.csproj
```

### "Token expirado"
Automático! O interceptador renova o token

---

## 📚 Documentação Completa

Depois do setup inicial, leia:

1. **[SETUP.md](./frontend/SETUP.md)** - Instalação detalhada
2. **[ARCHITECTURE.md](./frontend/ARCHITECTURE.md)** - Como funciona tudo
3. **[DEVELOPMENT.md](./frontend/DEVELOPMENT.md)** - Como desenvolver
4. **[FRONTEND_INDEX.md](./frontend/FRONTEND_INDEX.md)** - Índice de tudo

---

## 🎨 Componentes Disponíveis

Todos prontos para usar:

```typescript
// Exemplo de uso
import { Button, Input, Alert, Card } from '@/components'
import { useAuthOperations } from '@/features/auth'
import { useAuth } from '@/store/authStore'
```

---

## 🚀 Próximas Funcionalidades

- [ ] Módulo de Despesas (CRUD)
- [ ] Categorias
- [ ] Gráficos e Relatórios
- [ ] Notificações
- [ ] Testes Unitários
- [ ] Testes E2E

---

## 💡 Dicas

1. **VS Code**: Instale "Tailwind CSS IntelliSense"
2. **DevTools**: Instale "React Developer Tools"
3. **Console**: Use `useAuth.getState()` para debugar
4. **Network**: Veja requisições em Chrome DevTools

---

## 👨‍💻 Estrutura de Código Professional

```typescript
// ✅ Bom
const handleLogin = async (data: LoginRequest) => {
  try {
    await login(data);
    navigate('/');
  } catch (error) {
    setError(error.message);
  }
};

// ❌ Evitar
const handleClick = () => {
  fetch('/api/auth/login', {...})
};
```

---

## 🎯 Status do Projeto

| Área | Status | Detalhes |
|------|--------|----------|
| Autenticação | ✅ Completa | Login, Register, Logout |
| Components | ✅ Completa | 10+ componentes |
| Roteamento | ✅ Completa | Protegido e público |
| Styling | ✅ Completa | Tailwind customizado |
| HTTP Client | ✅ Completa | Axios com interceptadores |
| State | ✅ Completa | Zustand persistido |
| Documentação | ✅ Completa | 5 arquivos .md |

---

## 🎉 Você está pronto!

Acesse `http://localhost:3000` e comece a testar a aplicação.

Qualquer dúvida? Consulte a documentação ou examine o código - está bem comentado!

---

**Happy Coding! 🚀**
