# Setup e Instalação - Zivro Frontend

## Pré-requisitos

- Node.js 18+ 
- npm 9+ ou yarn
- Visual Studio Code (recomendado)

## Instalação Rápida

### 1. Navegar para o diretório frontend

```bash
cd frontend
```

### 2. Instalar dependências

```bash
npm install
```

### 3. Configurar variáveis de ambiente

Copie `.env.example` para `.env.local`:

```bash
cp .env.example .env.local
```

Edite `.env.local` conforme necessário (geralmente already configurado para localhost:5001):

```env
VITE_API_URL=https://localhost:5001/api
VITE_APP_NAME=Zivro
```

### 4. Iniciar servidor de desenvolvimento

```bash
npm run dev
```

A aplicação estará disponível em: **http://localhost:3000**

## Comandos Disponíveis

```bash
# Desenvolvimento com hot reload
npm run dev

# Build para produção
npm run build

# Preview da build
npm run preview

# Linting
npm run lint

# Formatação de código
npm run format
```

## Configuração do Backend

### CORS (Cross-Origin Resource Sharing)

O frontend precisa se comunicar com o backend. Certifique-se que o backend está configurado com CORS:

```csharp
// Program.cs do backend
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

app.UseCors("FrontendPolicy");
```

### URLs do Backend

- **Desenvolvimento**: `https://localhost:5001`
- **API Base**: `https://localhost:5001/api`

## Estrutura de Autenticação

### Endpoints Utilizados

```
POST   /api/auth/login       - Login
POST   /api/auth/register    - Registro
POST   /api/auth/refresh     - Renovar token
POST   /api/auth/logout      - Logout
GET    /api/auth/me          - Dados do usuário (protegido)
```

### Fluxo de Autenticação

1. Usuário acessa `/login` ou `/register`
2. Preenche o formulário com credenciais
3. Frontend envia request para `/api/auth/login` ou `/api/auth/register`
4. Backend valida e retorna `{ accessToken, refreshToken, userId }`
5. Frontend armazena tokens em localStorage
6. Frontend atualiza estado global (Zustand)
7. Usuário é redirecionado para `/ (home)`

## Extensões VS Code Recomendadas

```json
{
  "recommendations": [
    "esbenp.prettier-vscode",
    "dbaeumer.vscode-eslint",
    "chrisdias.vscode-opennewwindow",
    "bradlc.vscode-tailwindcss",
    "dsznajder.es7-react-js-snippets"
  ]
}
```

Instale automaticamente com: `code --install-extension [extension-id]`

## Debugging

### DevTools React

Instale [React Developer Tools](https://chrome.google.com/webstore/detail/react-developer-tools/) para Chrome.

### Redux DevTools (Zustand)

O Zustand está configurado com Devtools. Instale [Redux DevTools Extension](https://chrome.google.com/webstore/detail/redux-devtools/).

### Console do Browser

Use `console.log()` ou coloque breakpoints no DevTools.

```typescript
// Debug de estado
console.log(useAuth.getState());
```

## Troubleshooting

### Erro: CORS não permitido

**Problema**: `Access to XMLHttpRequest blocked by CORS`

**Solução**: Verificar configuração CORS no backend

### Erro: 401 Unauthorized

**Problema**: Token expired ou inválido

**Solução**: Fazer login novamente

### Erro: Network timeout

**Problema**: Backend não respondendo

**Solução**: Verificar se backend está rodando em `https://localhost:5001`

### Problema: Hot reload não funciona

**Problema**: Mudanças não refletem instantaneamente

**Solução**: 
```bash
# Clear node_modules cache
npm run dev -- --force
```

## Performance

### Se o app estiver lento

1. **Verificar DevTools Performance Tab**
   - Chrome DevTools → Performance → Record
   
2. **Analisar Network**
   - Chrome DevTools → Network
   - Verificar size de requests/responses

3. **Otimizações**
   - Lazy load de componentes
   - Memoization com `React.memo()`
   - Code splitting por rota

## Deployment

### Preparar para Produção

```bash
# Build otimizado
npm run build

# Testar build localmente
npm run preview
```

### Variáveis de Ambiente para Produção

```env
VITE_API_URL=https://zivro-api.example.com/api
VITE_APP_NAME=Zivro
```

### Deploy Opções

1. **Vercel** (Recomendado para Vite)
   ```bash
   npm i -g vercel
   vercel
   ```

2. **Netlify**
   - Conectar repositório GitHub
   - Build command: `npm run build`
   - Publish directory: `dist`

3. **Docker**
   - Criar Dockerfile
   - Build e deploy container

## Git Workflow

```bash
# Criar branch para feature
git checkout -b feature/new-feature

# Fazer commit
git add .
git commit -m "feat: implement new feature"

# Push
git push origin feature/new-feature

# Criar Pull Request no GitHub
```

## Contribuindo

1. Faça fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit seu changes (`git commit -m 'feat: add AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## Licença

Projeto Zivro © 2024. Todos os direitos reservados.

## Suporte

Para dúvidas ou problemas:
- Abra uma issue no GitHub
- Consulte a documentação
- Entre em contato com o time de desenvolvimento
