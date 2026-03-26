/**
 * Development guidelines and patterns
 */

# Frontend Development Guidelines

## Convenções de Código

### Estrutura de Pastas

- **app/** - Aplicação raiz (router, App.tsx)
- **components/** - Componentes reutilizáveis e genéricos
- **features/** - Funcionalidades específicas (auth, expenses, etc)
- **hooks/** - Custom React hooks
- **pages/** - Páginas/Rotas da aplicação
- **services/** - Serviços para chamadas API
- **store/** - Estado global (Zustand)
- **styles/** - Estilos globais e temas
- **types/** - Interfaces e tipos TypeScript
- **utils/** - Funções utilitárias genéricas

### Nomeclatura

- **Componentes**: PascalCase (LoginForm.tsx)
- **Hooks**: camelCase com prefixo 'use' (useAuthOperations.ts)
- **Tipos**: PascalCase com sufixo ou sem (User.ts, types/auth.ts)
- **Funções utilitárias**: camelCase (formatCurrency.ts)
- **Variáveis**: camelCase

### Componentes React

```typescript
// Preferido: Componentes funcionais com TypeScript
interface MyComponentProps {
  title: string;
  onClose?: () => void;
}

export const MyComponent = ({ title, onClose }: MyComponentProps) => {
  return <div>{title}</div>;
};
```

### Custom Hooks

```typescript
// Padrão para custom hooks
export const useMyHook = () => {
  const [state, setState] = useState<Type>(initialValue);
  
  const action = useCallback(() => {
    // Implementation
  }, [dependencies]);
  
  return { state, action };
};
```

### Serviços API

```typescript
// Padrão para serviços
class ServiceName {
  async methodName(params: Type): Promise<ResponseType> {
    try {
      const response = await httpClient.post('/endpoint', params);
      return response.data;
    } catch (error) {
      throw ApiError.fromResponse(error.response);
    }
  }
}

export const serviceName = new ServiceName();
```

### Tipos/Interfaces

```typescript
// Agrupe tipos relacionados em um arquivo único
interface User {
  id: string;
  name: string;
  email: string;
}

interface UserRequest {
  name: string;
  email: string;
}

interface UserResponse extends User {
  createdAt: string;
}
```

## Tratamento de Erros

1. Sempre use try-catch em chamadas assíncronas
2. Transforme erros de API em ApiError
3. Mostre mensagens de erro ao usuário
4. Log de erros para debugging

## Performance

1. Use `React.memo()` para componentes que recebem props imutáveis
2. Use `useCallback()` para funções passadas como props
3. Use `useMemo()` para cálculos custosos
4. Minimize re-renders com estado bem estruturado

## Testabilidade

1. Separe lógica de UI
2. Use dependency injection
3. Funções puras quando possível
4. Mock de serviços em testes

## Segurança

1. Nunca armazene senhas em localStorage
2. Sempre use HTTPS em produção
3. Valide inputs no frontend e backend
4. Sanitize conteúdo antes de renderizar

## Acessibilidade

1. Use semantic HTML
2. Labels para inputs
3. ARIA attributes quando necessário
4. Teste com leitores de tela

## Boas Práticas

1. ✅ TypeScript strict mode
2. ✅ ESLint para code quality
3. ✅ Prettier para formatting
4. ✅ Git hooks (pre-commit)
5. ✅ Code reviews
6. ✅ Testes unitários
7. ✅ Testes E2E
