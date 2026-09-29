# Cadastro de Currículos — CIEE/PR

Aplicação full stack para a equipe de recrutamento cadastrar e consultar candidatos.

Há **duas formas de cadastro**, usando o **mesmo formulário** e as **mesmas regras de validação**:

1. **Manual** — preenchimento direto dos campos e salvamento.
2. **Com PDF** — envio de um currículo; o backend extrai o texto, tenta identificar dados e preenche o formulário para revisão antes de salvar.

O PDF é **opcional**. Se o arquivo não for enviado ou a leitura falhar, o cadastro manual continua disponível.

---

## Tecnologias e versões

| Camada | Tecnologia | Versão |
|--------|------------|--------|
| Frontend | React | 19.2.x |
| Frontend | TypeScript | 6.x |
| Frontend | Vite | 8.3.x |
| Frontend | React Router DOM | 7.x |
| Backend | ASP.NET Core Web API | .NET 8 |
| Backend | C# | 12 |
| Banco de dados | SQL Server (LocalDB / Express) | — |
| ORM | Entity Framework Core | 8.0.11 |
| Extração de PDF | UglyToad.PdfPig | 1.7.0-custom-5 |
| Documentação da API | Swashbuckle (Swagger) | 6.6.2 |
| Testes | xUnit | 2.x |

---

## Pré-requisitos

- Node.js 18+ (recomendado 20+)
- .NET 8 SDK
- SQL Server (LocalDB, Express ou instância completa)
- Git
- Ferramenta EF Core: `dotnet tool install --global dotnet-ef`

---

## Dados do candidato

| Campo | Obrigatório | Observação |
|-------|-------------|------------|
| Nome completo | Sim | — |
| E-mail | Sim | Formato válido |
| Telefone | Não | — |
| Área / cargo de interesse | Não | — |
| Resumo profissional | Não | — |
| Formação acadêmica | Não | Extra além do enunciado mínimo |
| Experiências profissionais | Não | Extra além do enunciado mínimo |

---

## Configuração do banco de dados

1. Inicie o SQL Server / LocalDB:

```bash
sqllocaldb start MSSQLLocalDB
```

2. Copie o arquivo de exemplo (sem credenciais reais no repositório):

```bash
cd backend/CadastroCurriculos.Api
copy appsettings.example.json appsettings.json
```

3. Ajuste a connection string, se necessário:

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

Isso cria o banco `CadastroCurriculos` e a tabela `Candidatos`.

---

## Executar o backend

```bash
cd backend/CadastroCurriculos.Api
dotnet run --urls http://localhost:5052
```

- API: http://localhost:5052  
- Swagger: http://localhost:5052/swagger  
- CORS liberado para: `http://localhost:5173`

---

## Executar o frontend

```bash
cd frontend
copy .env.example .env
npm install
npm run dev
```

Acesse: http://localhost:5173

---

## Telas

| Rota | Descrição |
|------|-----------|
| `/candidatos` | Listagem de candidatos |
| `/candidatos/novo` | Cadastro manual e importação de PDF |
| `/candidatos/:id` | Detalhes + exportação do currículo |

---

## Endpoints da API

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/api/candidatos` | Cadastra um candidato |
| `GET` | `/api/candidatos` | Lista candidatos (mais recente primeiro) |
| `GET` | `/api/candidatos/{id}` | Detalhes de um candidato |
| `POST` | `/api/candidatos/extrair-pdf` | Extrai dados de um currículo PDF |

### Exemplo de cadastro

```json
{
  "nomeCompleto": "João da Silva",
  "email": "joao@email.com",
  "telefone": "(41) 99999-9999",
  "areaInteresse": "Desenvolvimento",
  "resumoProfissional": "Desenvolvedor com experiência em Java e React.",
  "formacaoAcademica": "Bacharelado em Ciência da Computação - UFPR (2018-2022)",
  "experienciasProfissionais": "Desenvolvedor Frontend - Empresa X (2022-2024)"
}
```

### Extração de PDF (`multipart/form-data`)

- Campo do arquivo: `file`
- Apenas PDF
- Tamanho máximo: **5 MB**
- Arquivo não pode estar vazio
- Validação da assinatura `%PDF` (não confia só na extensão)

O backend tenta identificar:

- Nome completo  
- E-mail  
- Telefone  
- Formação acadêmica  
- Experiências profissionais  

Quando um campo não for encontrado, retorna `null` e o formulário permite preenchimento manual.

---

## Fluxo de importação do PDF

1. Em `/candidatos/novo`, o usuário seleciona um PDF.
2. O frontend envia o arquivo para `POST /api/candidatos/extrair-pdf`.
3. O backend lê o texto com **PdfPig**.
4. Heurísticas/regex tentam identificar os campos.
5. O formulário é preenchido com o que foi encontrado.
6. O usuário revisa, completa e salva.

### Como o nome é identificado

1. Procura linhas com rótulos como `Nome:` ou `Nome completo:`.
2. Se não houver rótulo, analisa as primeiras linhas e escolhe a primeira que parece nome próprio (duas ou mais palavras capitalizadas, sem e-mail/telefone/títulos de seção).

### Como a formação é delimitada

A extração começa em títulos como `Formação acadêmica` / `Educação` e **para** ao encontrar outra seção (Cursos, Experiência, Habilidades, Projetos etc.), para não misturar dados.

---

## Testes

```bash
cd backend
dotnet test
```

Os testes cobrem:

- Cadastro válido  
- Nome obrigatório  
- E-mail obrigatório / inválido  
- Candidato inexistente  
- PDF válido, inválido, acima de 5 MB  
- PDF sem e-mail / sem telefone  
- Falha de extração  
- Formação sem incluir habilidades/projetos  

---

## PDF fictício para teste

```text
docs/curriculo-ficticio.pdf
```

Dados fictícios:

- Nome: Mariana Oliveira Santos  
- E-mail: mariana.santos@email.com  
- Telefone: (41) 99999-8888  
- Área: Desenvolvimento de Software  
- Formação e experiências de exemplo  

---

## Estrutura do projeto

```text
/
├── backend/
│   ├── CadastroCurriculos.Api/
│   │   ├── Controllers/          # CandidatosController
│   │   ├── Data/                 # ContextoAplicacao
│   │   ├── DTOs/
│   │   ├── Models/               # Candidato
│   │   ├── Services/             # ServicoCandidato, ServicoExtracaoPdf
│   │   ├── Validators/
│   │   ├── Migrations/
│   │   ├── Program.cs
│   │   └── appsettings.example.json
│   └── CadastroCurriculos.Tests/
├── frontend/
│   └── src/
│       ├── components/           # Layout, Alerta
│       ├── pages/                # Lista, formulário, detalhes
│       ├── services/             # Chamadas à API
│       ├── types/
│       ├── utils/
│       └── App.tsx
├── docs/
│   └── curriculo-ficticio.pdf
├── README.md
└── DESENVOLVIMENTO.md
```

---

## Limitações

- A extração depende de PDF com **texto selecionável**.
- Currículos **escaneados** (somente imagem) não funcionam bem sem OCR.
- Layouts muito diferentes podem impedir a identificação correta de nome, telefone, formação ou experiências.
- Não há autenticação/login (fora do escopo do desafio).

---

## Segurança

- `appsettings.json` e `.env` estão no `.gitignore`
- Use `appsettings.example.json` e `.env.example` como referência
- Não versionar senhas ou connection strings com credenciais reais
