# DESENVOLVIMENTO

Registro honesto do desenvolvimento do desafio de cadastro e consulta de currículos.

## 1. Organização do trabalho

O trabalho foi dividido nesta ordem:

1. Inicialização do repositório Git e `.gitignore`
2. Backend ASP.NET Core (.NET 8): entidade, EF Core, endpoints e validações
3. Extração de PDF no backend (PdfPig + regex/heurísticas)
4. Testes automatizados do backend
5. Frontend React + TypeScript + Vite (listagem, cadastro e detalhes)
6. PDF fictício de teste
7. Documentação (`README.md` e este arquivo)
8. Validação manual dos fluxos (cadastro, PDF, erros e listagem)

Commits pequenos e descritivos foram feitos ao longo do processo.

## 2. Decisões técnicas

### React + TypeScript + Vite

Escolhidos por serem os requisitos do desafio e por oferecerem setup rápido, tipagem e boa experiência de desenvolvimento.

### ASP.NET Core Web API (.NET 8)

Atende ao requisito e permite organizar Controllers, Services, DTOs e DI de forma simples, sem arquitetura excessiva.

### SQL Server + Entity Framework Core

Atendem ao requisito. Foi usado LocalDB na máquina de desenvolvimento (`(localdb)\mssqllocaldb`) por já estar disponível e não exigir senha no repositório.

### UglyToad.PdfPig

Biblioteca .NET pura para leitura de PDF, sem dependências nativas. Adequada para extrair texto em um desafio simples. Alternativas como iText teriam licenciamento/complexidade desnecessários aqui.

### Validação

Validação duplicada de propósito:

- Frontend: feedback imediato
- Backend: fonte da verdade (nunca confiar só no cliente)

## 3. Uso de inteligência artificial

Ferramentas de IA foram utilizadas como apoio durante o desenvolvimento.

- **Ferramenta/modelo:** Cursor (agente de código Composer)
- **Etapas em que ajudou:**
  - scaffolding do backend e frontend
  - implementação dos serviços de candidato e PDF
  - criação dos testes
  - estrutura das telas React
  - redação do README e deste `DESENVOLVIMENTO.md`
- **Como as respostas foram aproveitadas:**
  - código gerado foi revisado, compilado e executado localmente
  - ajustes manuais foram feitos quando a extração de PDF concatenava textos ou quando o limite de upload do Kestrel mascarava a mensagem de erro amigável
- **Partes revisadas manualmente:**
  - regex de e-mail/telefone
  - heurística de nome
  - mensagens de erro da API e da interface
  - commits e documentação

### Exemplos reais de prompts utilizados

> "Você é um desenvolvedor full stack responsável por implementar um desafio técnico de cadastro e consulta de currículos..." (prompt completo do desafio)

> Correções pedidas durante a execução, como validar endpoints com curl, ajustar extração do PDF e garantir que arquivos acima de 5 MB retornassem a mensagem definida no enunciado.

Todo código gerado foi revisado, executado e adaptado conforme as necessidades do projeto.

## 4. Correções e adaptações

Problemas encontrados e correções:

1. **Texto do PDF concatenado** (`email.comTelefone`)  
   Ajuste na extração com `GetWords()` + quebra por posição Y e pós-processamento para separar rótulos colados. Regex de e-mail restrita ao TLD.

2. **Arquivo > 5 MB retornava erro genérico do ASP.NET**  
   Limite do Kestrel/FormOptions elevado para 6 MB; a regra de negócio continua validando 5 MB e devolve a mensagem solicitada.

3. **PDFs de teste com texto em uma única string `Tj`**  
   O gerador de PDF nos testes passou a posicionar cada linha com `Tm`, refletindo melhor currículos reais.

4. **SQL Server Express parado na máquina**  
   Uso de LocalDB para desenvolvimento e documentação no README.

5. **`appsettings.json` no `.gitignore`**  
   Mantido `appsettings.example.json` versionado, sem credenciais reais.

## 5. Validação

Validações realizadas:

- `dotnet build` no backend
- `npm run build` no frontend
- `dotnet test` (13 testes aprovados)
- `dotnet ef database update` (tabela `Candidates` criada)
- Cadastro manual via API
- Extração do `docs/curriculo-ficticio.pdf` (nome, e-mail e telefone)
- PDF inválido (HTTP 400)
- PDF maior que 5 MB (HTTP 400 com mensagem clara)
- Candidato inexistente (HTTP 404)
- Listagem e detalhes

## 6. Tempo

Tempo aproximado dedicado ao desafio: **cerca de 3 a 4 horas** (implementação, correções, testes e documentação).

## 7. Dificuldades

- Extrair nome de forma confiável sem NLP/IA dedicada
- Lidar com PDFs cujo texto vem sem espaços/quebras adequados
- Garantir mensagens amigáveis mesmo quando o framework rejeita o upload antes do controller
- Manter o projeto simples sem adicionar infraestrutura desnecessária

## 8. Limitações

- Extração depende de PDF com texto embutido (não funciona bem em scans)
- Heurística de nome pode falhar em layouts criativos ou multilíngues
- Telefone cobre formatos brasileiros comuns, não todos os casos internacionais
- Sem autenticação, upload assíncrono avançado ou OCR

## 9. Melhorias futuras

Com mais tempo, seria interessante:

- OCR para currículos digitalizados
- Deduplicação por e-mail
- Edição e exclusão de candidatos
- Paginação e busca na listagem
- Testes end-to-end do frontend
- Container Docker opcional (SQL Server + API) para onboarding
- Melhor scoring na identificação do nome (mais padrões de currículo)
