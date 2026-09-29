# Cadastro de Currículos

Aplicação full stack para cadastro e consulta de candidatos por uma equipe de recrutamento. O candidato pode ser cadastrado manualmente ou a partir da importação de um currículo em PDF, reutilizando o mesmo formulário e as mesmas regras de validação.

## Tecnologias

| Camada | Tecnologia | Versão |
|--------|------------|--------|
| Frontend | React | 19.x |
| Frontend | TypeScript | 5.x |
| Frontend | Vite | 8.x |
| Frontend | React Router | 7.x |
| Backend | ASP.NET Core Web API | .NET 8 |
| Backend | C# | 12 |
| Banco | SQL Server (LocalDB/Express) | — |
| ORM | Entity Framework Core | 8.0.11 |
| PDF | UglyToad.PdfPig | 1.7.0-custom-5 |
| Testes | xUnit | 2.x |

## Pré-requisitos

- [Node.js](https://nodejs.org/) 18+ (recomendado 20+)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB, Express ou instância completa)
- Git
- Ferramenta EF: `dotnet tool install --global dotnet-ef`

## Configuração do banco

1. Garanta que o SQL Server / LocalDB esteja em execução.
   - LocalDB: `sqllocaldb start MSSQLLocalDB`
2. Copie o arquivo de exemplo de configuração:

```bash
cd backend/CadastroCurriculos.Api
copy appsettings.example.json appsettings.json
```

3. Ajuste a connection string em `appsettings.json` se necessário:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CadastroCurriculos;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

4. Aplique a migration:

```bash
cd backend/CadastroCurriculos.Api
dotnet ef database update
```

Isso cria o banco `CadastroCurriculos` e a tabela `Candidates`.

## Executar backend

```bash
cd backend/CadastroCurriculos.Api
dotnet run --urls http://localhost:5052
```

- API: http://localhost:5052
- Swagger: http://localhost:5052/swagger

CORS está liberado para `http://localhost:5173`.

## Executar frontend

```bash
cd frontend
copy .env.example .env
npm install
npm run dev
```

Acesse: http://localhost:5173

## Testes

```bash
cd backend
dotnet test
```

Os testes cobrem cadastro válido/inválido, candidato inexistente e regras de PDF (formato, tamanho, ausência de campos e falha de extração).

## Endpoints

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/api/candidates` | Cadastra um candidato |
| `GET` | `/api/candidates` | Lista candidatos (mais recente primeiro) |
| `GET` | `/api/candidates/{id}` | Detalhes de um candidato |
| `POST` | `/api/candidates/extract-pdf` | Extrai nome, e-mail e telefone de um PDF |

### Exemplo de cadastro

```json
{
  "nomeCompleto": "João da Silva",
  "email": "joao@email.com",
  "telefone": "(41) 99999-9999",
  "areaInteresse": "Desenvolvimento",
  "resumoProfissional": "Desenvolvedor com experiência em Java e React."
}
```

### Extração de PDF

- Campo multipart: `file`
- Apenas PDF
- Máximo 5 MB
- Arquivo não pode estar vazio
- Além da extensão, o backend valida a assinatura `%PDF`

## Importação do PDF

1. Em `/candidatos/novo`, o usuário seleciona um PDF.
2. O frontend envia o arquivo para `POST /api/candidates/extract-pdf`.
3. O backend extrai o texto com **PdfPig** e tenta identificar nome, e-mail e telefone.
4. O formulário é preenchido com os dados encontrados.
5. O usuário pode corrigir/completar e salvar normalmente.

A falha na leitura do PDF **não impede** o cadastro manual.

### Estratégia de identificação do nome

1. Procura linhas com rótulos como `Nome:` ou `Nome completo:`.
2. Caso contrário, analisa as primeiras linhas e escolhe a primeira que parece um nome próprio (duas ou mais palavras capitalizadas, sem e-mail/telefone/rótulos comuns de currículo).

## Telas

| Rota | Tela |
|------|------|
| `/candidatos` | Listagem |
| `/candidatos/novo` | Cadastro (manual + PDF) |
| `/candidatos/:id` | Detalhes |

## PDF fictício para teste

Arquivo disponível em:

```text
docs/curriculo-ficticio.pdf
```

Dados fictícios:

- Nome: Mariana Oliveira Santos
- E-mail: mariana.santos@email.com
- Telefone: (41) 99999-8888
- Área: Desenvolvimento de Software

## Limitações

- A extração depende da estrutura do currículo (PDF com texto selecionável).
- Currículos escaneados (somente imagem) não terão texto extraído.
- Nome, telefone ou e-mail podem não ser identificados corretamente em layouts atípicos.
- Não há autenticação (fora do escopo do desafio).

## Estrutura do projeto

```text
/
├── backend/
│   ├── CadastroCurriculos.Api/
│   │   ├── Controllers/
│   │   ├── Data/
│   │   ├── DTOs/
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── Validators/
│   │   ├── Migrations/
│   │   ├── Program.cs
│   │   └── appsettings.example.json
│   └── CadastroCurriculos.Tests/
├── frontend/
│   └── src/
│       ├── components/
│       ├── pages/
│       ├── services/
│       ├── types/
│       ├── utils/
│       └── App.tsx
├── docs/
│   └── curriculo-ficticio.pdf
├── README.md
└── DESENVOLVIMENTO.md
```

## Segurança

- `appsettings.json` e `.env` estão no `.gitignore`
- Use sempre `appsettings.example.json` / `.env.example` como referência
- Não versionar senhas ou connection strings com credenciais reais
