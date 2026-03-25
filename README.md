# 🚀 Zivro

Zivro é uma aplicação fintech moderna focada no controle inteligente de gastos, oferecendo insights, automação e uma experiência simples e intuitiva.

---

# 🧱 Arquitetura do Projeto

O projeto segue uma arquitetura moderna baseada em:

* Clean Architecture (backend)
* Feature-based structure (frontend)
* Separação clara de responsabilidades

> Observação: itens de infraestrutura (Docker, CI/CD, etc.) serão adicionados futuramente.

## Estrutura geral

```
zivro/
├── backend/
├── frontend/
├── docs/
└── scripts/
```

zivro/
├── backend/
├── frontend/
├── infra/
├── docs/
└── scripts/

```

---

# ⚙️ Backend (.NET 8)

## Tecnologias

- .NET 8 Web API
- Entity Framework Core
- SQL Server

## Estrutura

```

backend/src/
├── Zivro.Api
├── Zivro.Application
├── Zivro.Domain
└── Zivro.Infrastructure

```

## Responsabilidades

- **Domain** → Entidades e regras de negócio
- **Application** → Casos de uso e serviços
- **Infrastructure** → Banco de dados e integrações
- **API** → Controllers e exposição HTTP

---

# 💰 Feature implementada: Expenses

## Entidade

- Expense
  - Id
  - Title
  - Amount
  - Category
  - Date

## Funcionalidades

- Listar gastos
- Criar gasto

## Endpoints

```

GET /api/expenses
POST /api/expenses

```

---

# 🗄️ Banco de Dados

- SQL Server
- EF Core Migrations

## Comandos

```

dotnet ef migrations add InitialCreate -p Zivro.Infrastructure -s Zivro.Api

dotnet ef database update -p Zivro.Infrastructure -s Zivro.Api

```

---

# ⚛️ Frontend (React SPA)

## Tecnologias

- React
- Fetch API
- TailwindCSS (UI base)

## Estrutura

```

frontend/src/
├── app/
├── components/
├── features/
│   └── expenses/
├── services/
├── pages/

```

## Funcionalidades

- Listagem de gastos
- Criação de gastos
- Integração com API

---

# 🔌 Integração Frontend ↔ Backend

Base URL:

```

[https://localhost:5001/api](https://localhost:5001/api)

```

---

# 🚀 Como rodar o projeto

## Backend

```

cd backend
dotnet run --project Zivro.Api

```

## Frontend

```

cd frontend
npm install
npm run dev

```

> Nota: Setup de containers e orquestração será incluído em etapas futuras.

---

# 🧠 Próximos passos

## Alta prioridade

- Autenticação (JWT)
- Categorias de gastos
- Dashboard com gráficos

## Evolução do produto

- Insights automáticos (IA)
- Classificação automática de gastos
- Alertas financeiros

## UX/UI

- Refinamento visual estilo fintech
- Mobile-first
- Microinterações

---

# 🎯 Visão do Produto

Zivro não é apenas um app de controle de gastos.

Objetivo:

> Transformar dados financeiros em decisões inteligentes, de forma simples e automática.

---

# 📌 Status atual

✅ Arquitetura definida  
✅ Backend estruturado  
✅ API funcional  
✅ Frontend integrado  
✅ CRUD básico de gastos  

---

# 🔥 Próxima milestone sugerida

1. Implementar autenticação
2. Criar dashboard financeiro
3. Melhorar UI para padrão fintech

---

# 👨‍💻 Autor

Projeto iniciado por Natan Bosquini Monteiro com foco em inovação, automação e arquitetura moderna.

---

# 📄 Licença

Uso interno / projeto em desenvolvimento

---

# 🧠 1. Essência da Marca (Base de tudo)

Antes de logo ou cores, precisamos alinhar o DNA:

### 🔹 Nome: **Zivro**

- Curto, moderno, memorável
- Sonoridade tech + financeira
- Não limita expansão (pode virar plataforma depois)

---

### 🎯 Propósito

> Ajudar pessoas a terem clareza e controle sobre sua vida financeira sem complexidade.
> 

---

### 🌍 Visão

> Ser o app mais intuitivo e inteligente de gestão financeira pessoal na América Latina.
> 

---

### 💡 Missão

> Simplificar o controle financeiro com tecnologia, automação e experiência agradável.
> 

---

### 🧭 Valores

- Simplicidade acima de tudo
- Transparência
- Inteligência (dados que ajudam, não confundem)
- Autonomia do usuário
- Design que transmite calma (não ansiedade)

---

# 🎭 2. Personalidade da Marca

Pense no Zivro como uma pessoa:

- Inteligente, mas não arrogante
- Organizado, mas não rígido
- Moderno, mas acessível
- Calmo (anti-estresse financeiro)
- Estilo: mistura de Nubank + Notion + Apple

👉 Arquétipo principal:

- **O Sábio (Sage)** + **O Cuidador (Caregiver)**

---

# 🗣️ 3. Tom de Voz

### Como o Zivro fala:

- Simples e direto
- Sem termos financeiros complicados
- Amigável, quase como um coach leve

### Exemplos:

❌ "Sua fatura foi consolidada com sucesso"

✅ "Sua fatura está pronta 👍"

❌ "Déficit financeiro identificado"

✅ "Você gastou mais do que o planejado esse mês"

# 🎨 4. Identidade Visual (Direção)

Baseado no que você já começou:

### Paleta sugerida refinada:

- Azul profundo → confiança (#0081A7)
- Turquesa → tecnologia (#00AFB9)
- Creme claro → leveza (#FDFCDC)
- Laranja suave → ação (#FED9B7)
- Coral → alerta leve (#F07167)

👉 Estratégia:

- Fundo claro (clean tipo Nubank/Apple)
- Destaques em azul/turquesa
- Alertas nunca agressivos (evitar vermelho puro)

---

### 🧩 Estilo visual

- Minimalista
- Muito espaçamento (respira)
- Ícones simples (outline ou semi-filled)
- Gráficos suaves (sem poluição)

---

# 🔤 5. Tipografia

Sugestão moderna:

- Primária: **Inter**
- Alternativa: **Poppins**
- Para destaque: pesos Medium / SemiBold

👉 Característica:

- Alta legibilidade (mobile first)
- Sensação clean + tech

---

# 🔷 6. Conceito do Logo (direção criativa)

Ideias fortes para o símbolo:

1. 📊 Gráfico ascendente estilizado (crescimento financeiro)
2. 🔄 Fluxo contínuo (entrada/saída de dinheiro)
3. 💠 Forma geométrica simples (ícone de app forte)
4. 🧠 "Z" abstrato (marca memorável tipo Nubank)

👉 Direção ideal:

- Simples o suficiente para virar ícone
- Reconhecível em tamanho pequeno
- Sem texto (para app)

---

# 🧱 7. Posicionamento de Mercado

### Contra quem compete:

- Mobills
- Organizze
- Guiabolso
- Planilhas Excel (concorrente invisível)

---

### Diferencial do Zivro:

> "O app financeiro que você realmente usa todos os dias."
> 

Foco em:

- UX absurda de simples
- Automação (menos input manual)
- Insights inteligentes (tipo IA leve)

---

# 💬 8. Slogan (opções)

- "Controle simples. Vida leve."
- "Seu dinheiro, sem complicação."
- "Entenda seu dinheiro de verdade."
- "Menos planilha. Mais clareza."
- "Organizar sua vida financeira nunca foi tão fácil."

```
