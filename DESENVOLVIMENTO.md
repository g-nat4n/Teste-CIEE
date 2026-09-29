# Registro de Desenvolvimento — Cadastro de Currículos

Este documento apresenta como o desafio técnico de **Cadastro de Currículos** foi desenvolvido, desde a estruturação inicial até os testes e ajustes finais.

A ideia foi construir uma aplicação full stack simples, funcional e organizada, mantendo o foco nos requisitos do desafio e evitando complexidade desnecessária.

Durante o desenvolvimento, utilizei ferramentas de inteligência artificial como apoio para levantar ideias, tirar dúvidas sobre código, criar e revisar testes, investigar problemas e acelerar algumas tarefas repetitivas. A implementação foi sendo construída, executada e validada localmente ao longo do processo, com adaptações sempre que o comportamento não correspondia ao esperado.

---

## 1. Como o trabalho foi organizado

O desenvolvimento foi estruturado inicialmente a partir dos requisitos do desafio.

Antes de implementar as telas, organizei o projeto separando frontend, backend e documentação. A partir daí, o trabalho foi desenvolvido de forma incremental, validando cada parte antes de avançar para a próxima.

A sequência principal foi:

1. Criação do repositório e configuração inicial do projeto.
2. Estruturação da API em ASP.NET Core com .NET 8.
3. Criação do modelo de candidato e configuração do Entity Framework Core.
4. Configuração do SQL Server/LocalDB e das migrations.
5. Implementação dos endpoints de cadastro, listagem e consulta.
6. Implementação das validações dos dados.
7. Desenvolvimento da importação de currículos em PDF.
8. Implementação da extração de texto e identificação dos dados do candidato.
9. Criação dos testes automatizados do backend.
10. Desenvolvimento do frontend em React + TypeScript.
11. Integração do frontend com a API.
12. Criação do fluxo de cadastro manual e cadastro por PDF.
13. Criação de um currículo fictício para testes.
14. Ajustes de layout e identidade visual.
15. Implementação de formação, experiências e exportação do currículo.
16. Revisão dos casos de erro e dos limites de upload.
17. Revisão da documentação e das instruções para execução.

O projeto foi sendo estruturado e refinado aos poucos. Conforme surgiam problemas durante os testes, as implementações eram ajustadas antes de seguir para a próxima etapa.

A prioridade durante o desenvolvimento foi:

**funcionalidade → validação → testes → usabilidade → documentação.**

---

## 2. Estrutura e decisões técnicas

### Frontend — React + TypeScript + Vite

Foi utilizado React com TypeScript e Vite.

A escolha foi feita porque React estava entre as opções permitidas pelo desafio e permite construir as telas de forma simples, mantendo o código organizado e tipado.

O frontend foi dividido principalmente entre:

* páginas;
* componentes reutilizáveis;
* serviços responsáveis pelas chamadas à API;
* tipos;
* utilitários.

As principais telas são:

* listagem de candidatos;
* cadastro de candidato;
* importação de currículo em PDF;
* visualização dos detalhes do candidato.

O mesmo formulário é utilizado tanto para o cadastro manual quanto para o cadastro iniciado pela importação do PDF. Isso evita manter duas implementações diferentes para as mesmas regras.

---

### Backend — ASP.NET Core Web API

O backend foi desenvolvido com **ASP.NET Core Web API (.NET 8)**.

A escolha ocorreu principalmente pela familiaridade com C# e pela possibilidade de organizar a aplicação de forma clara usando Controllers, Services, DTOs e injeção de dependência.

A estrutura ficou dividida em:

* **Controllers** — responsáveis pelos endpoints HTTP;
* **Services** — regras de negócio e processamento;
* **DTOs** — objetos utilizados na comunicação da API;
* **Validators** — regras de validação;
* **Models** — entidades do domínio;
* **Data** — configuração do Entity Framework Core e banco de dados.

A intenção foi manter uma arquitetura simples, sem adicionar camadas ou padrões que não trouxessem benefício para o tamanho do desafio.

---

### Banco de dados — SQL Server + Entity Framework Core

O desafio solicitava SQL Server, então foi utilizado SQL Server com Entity Framework Core.

Durante o desenvolvimento foi utilizado principalmente o **LocalDB**, por já estar disponível no ambiente e facilitar a execução local sem necessidade de configurar um servidor externo ou colocar credenciais no projeto.

As alterações do banco são controladas por migrations do Entity Framework Core.

---

### Extração de PDF

Para leitura dos currículos foi utilizada a biblioteca **UglyToad.PdfPig**.

A escolha ocorreu porque ela permite trabalhar com PDFs diretamente no .NET sem exigir dependências nativas adicionais.

A extração não pretende ser um sistema completo de interpretação de currículos. O objetivo foi atender ao escopo do desafio utilizando texto extraído, expressões regulares e algumas heurísticas.

A API tenta identificar:

* nome;
* e-mail;
* telefone;
* formação acadêmica;
* experiências profissionais.

Quando alguma informação não é encontrada, o formulário continua disponível para preenchimento manual.

---

## 3. Desenvolvimento da importação de PDF

Essa foi uma das partes que exigiu mais ajustes durante o desenvolvimento.

Inicialmente, a ideia era extrair o texto do PDF e trabalhar diretamente com o resultado. Durante os testes, percebi que PDFs podem apresentar o conteúdo de maneira diferente do que aparece visualmente para o usuário.

Um exemplo encontrado foi a concatenação de informações, como:

```text
email.comTelefone
```

Por isso, foi necessário trabalhar com a posição das palavras extraídas do PDF e realizar um pós-processamento do conteúdo.

Também foram criadas regras específicas para determinados campos.

### Identificação do nome

A extração procura primeiro informações explicitamente identificadas, como:

```text
Nome:
Nome completo:
```

Quando esses rótulos não estão presentes, são analisadas as primeiras linhas do documento para encontrar uma sequência que tenha características de nome.

Essa abordagem é uma heurística e não pretende funcionar para qualquer modelo de currículo.

### Formação acadêmica

Outro problema encontrado durante os testes foi a formação acabar incluindo informações de outras seções do currículo.

A solução foi delimitar a seção de formação a partir de títulos conhecidos, como:

```text
Formação acadêmica
Educação
```

e interromper a leitura quando outra seção relevante é encontrada, como:

```text
Experiência
Experiências profissionais
Habilidades
Projetos
Cursos
```

Também foram adicionados testes específicos para evitar que habilidades e projetos fossem incorporados à formação.

---

## 4. Uso de inteligência artificial

A inteligência artificial foi utilizada durante o desenvolvimento como uma **ferramenta de apoio**, principalmente para acelerar pesquisas, esclarecer dúvidas e ajudar na elaboração inicial de algumas soluções.

A ferramenta utilizada foi o **Cursor**, com recursos de assistência de programação.

A IA não foi utilizada como substituta da execução e validação do projeto. As sugestões eram incorporadas somente depois de analisar o código, executar a aplicação e verificar se o comportamento atendia ao que era necessário.

### Onde a IA ajudou

Entre as atividades em que utilizei IA estão:

* sugestões iniciais para estruturação do projeto;
* dúvidas sobre recursos do C# e ASP.NET Core;
* dúvidas sobre React e TypeScript;
* sugestões de implementação;
* criação e revisão de testes;
* investigação de erros;
* ideias para tratamento de arquivos PDF;
* sugestões para organização do código;
* ajustes de CSS e interface;
* revisão de documentação;
* identificação de possíveis casos de erro.

Em alguns momentos, a IA também foi utilizada como uma espécie de segunda opinião durante o desenvolvimento: eu apresentava um problema encontrado na execução e avaliava as alternativas sugeridas antes de decidir como implementar a correção.

### Exemplos de pedidos

Alguns exemplos de solicitações feitas durante o desenvolvimento foram:

> "Como posso estruturar esse endpoint para receber um PDF e retornar os dados extraídos?"

> "Esse regex está aceitando formatos de e-mail que não deveria. Como posso restringir a validação?"

> "O texto extraído do PDF está juntando o e-mail com o telefone. Quais alternativas existem para separar essas informações?"

> "Preciso garantir que a formação não continue sendo extraída quando chegar na seção de habilidades ou projetos."

> "Pode sugerir testes para validar o limite de 5 MB e um PDF inválido?"

> "Como posso deixar a impressão do currículo mais compacta para evitar páginas desnecessárias?"

> "Pode revisar essa parte do código e sugerir uma organização mais simples?"

Também foram utilizadas sugestões para ajustes visuais e para revisão do README e da documentação do desenvolvimento.

### Como as sugestões foram utilizadas

As respostas da IA não foram simplesmente copiadas para o projeto.

Durante a implementação, as sugestões eram analisadas e adaptadas ao código existente. Depois eram executadas localmente para verificar se realmente resolviam o problema.

Quando uma sugestão não funcionava ou introduzia um comportamento indesejado, ela era alterada ou descartada.

Isso aconteceu principalmente na parte de extração de PDF, porque a estrutura interna de cada arquivo pode variar bastante.

---

## 5. Correções e adaptações realizadas

Durante os testes foram encontrados alguns comportamentos que precisaram ser corrigidos.

### Texto do PDF sendo concatenado

A extração inicial podia juntar informações que visualmente estavam em linhas diferentes.

Foi necessário melhorar a forma de leitura das palavras do PDF, considerar suas posições e realizar tratamento posterior do texto.

---

### Validação do limite de 5 MB

O requisito determinava que o PDF deveria possuir no máximo 5 MB.

Durante os testes, arquivos acima do limite inicialmente podiam gerar uma resposta genérica do ASP.NET antes que a validação da aplicação fosse executada.

O comportamento foi ajustado para que a aplicação pudesse apresentar uma mensagem mais clara ao usuário, mantendo o limite de negócio de 5 MB.

---

### Formação incluindo outras informações

Foi identificado que a extração da formação podia continuar lendo conteúdo de habilidades, projetos ou outras seções.

Foram adicionadas regras para identificar o início e o fim da seção de formação e testes específicos para esse comportamento.

---

### Exportação do currículo

A primeira versão da impressão do currículo gerava espaços e quebras de linha desnecessárias.

O CSS de impressão foi ajustado para deixar o resultado mais compacto e adequado para utilização como PDF através da opção de impressão do navegador.

---

### Campo de cursos

Durante o desenvolvimento foi considerado um campo específico para cursos.

Após avaliar o fluxo, esse campo deixou de fazer parte da interface e da extração porque não era necessário para o fluxo principal definido para o desafio.

Caso exista algum resíduo desse campo no modelo ou banco, ele deve ser removido em uma revisão futura para evitar manter elementos que não são utilizados.

---

## 6. Validação da solução

A solução foi validada tanto automaticamente quanto manualmente.

### Backend

Foram executados:

```bash
dotnet build
```

e:

```bash
dotnet test
```

Os testes automatizados cobrem situações como:

* cadastro válido;
* nome obrigatório;
* e-mail obrigatório;
* e-mail inválido;
* candidato inexistente;
* PDF válido;
* PDF inválido;
* PDF acima de 5 MB;
* PDF sem e-mail;
* PDF sem telefone;
* falha de extração;
* formação sem incluir habilidades e projetos.

Ao final da implementação, os testes do backend estavam passando.

### Banco de dados

Também foi executado:

```bash
dotnet ef database update
```

para verificar a criação do banco e da tabela de candidatos através das migrations.

### Frontend

O frontend foi validado com:

```bash
npm run build
```

Além da compilação, os principais fluxos foram executados manualmente:

* abrir a listagem;
* realizar cadastro manual;
* importar um currículo em PDF;
* revisar os dados extraídos;
* completar informações;
* salvar o candidato;
* consultar os detalhes;
* exportar o currículo.

Também foram testados casos de erro, como PDF inválido e arquivo acima do tamanho permitido.

---

## 7. Tempo aproximado

O tempo total dedicado ao desafio foi de aproximadamente **4 a 5 horas**.

Esse tempo inclui:

* estruturação inicial;
* desenvolvimento do backend;
* desenvolvimento do frontend;
* integração entre as partes;
* implementação da importação de PDF;
* testes;
* correções;
* ajustes de interface;
* documentação.

Parte do tempo foi dedicada à investigação e correção de problemas encontrados durante os testes, principalmente na extração de dados dos PDFs.

---

## 8. Dificuldades encontradas

A principal dificuldade técnica foi trabalhar com a extração de informações de currículos em PDF.

O conteúdo que aparece organizado visualmente em um PDF nem sempre é retornado pela biblioteca na mesma estrutura. Isso tornou necessário trabalhar com heurísticas e considerar a posição das palavras.

Também houve dificuldade em definir regras que fossem simples o suficiente para o escopo do desafio, mas que não produzissem resultados muito incorretos.

Outro ponto foi tratar corretamente os limites de upload e fazer com que erros técnicos fossem transformados em mensagens compreensíveis para o usuário.

No frontend, uma preocupação foi manter o fluxo de importação simples: o PDF deveria ajudar a preencher o formulário, mas o usuário deveria continuar tendo controle sobre os dados antes de salvar.

---

## 9. Limitações atuais

A solução possui algumas limitações conhecidas.

### Extração de PDF

A extração depende principalmente de PDFs que possuem texto selecionável.

Currículos escaneados, compostos apenas por imagens, não são tratados adequadamente sem OCR.

Além disso, currículos com layouts muito diferentes podem fazer com que algumas informações não sejam identificadas corretamente.

### Heurísticas

A identificação de nome, formação e experiências utiliza regras e heurísticas.

Não existe garantia de que essas regras funcionem para todos os formatos possíveis de currículo.

### Funcionalidades

Para manter o escopo do desafio, a aplicação não possui atualmente:

* autenticação/login;
* edição de candidatos;
* exclusão de candidatos;
* paginação;
* busca avançada;
* OCR;
* testes end-to-end do frontend;
* geração de PDF server-side.

---

## 10. O que eu faria com mais tempo

Com mais tempo para evoluir o projeto, algumas melhorias seriam:

### OCR

Adicionar OCR para permitir a leitura de currículos digitalizados.

### Busca e paginação

Adicionar busca por nome, e-mail ou área de interesse e paginação na listagem.

### Edição e exclusão

Permitir que a equipe de recrutamento altere ou remova candidatos cadastrados.

### Deduplicação

Adicionar uma regra para evitar o cadastro duplicado do mesmo candidato, utilizando principalmente o e-mail como possível identificador.

### Testes de frontend

Criar testes automatizados para os principais fluxos do React e, posteriormente, testes end-to-end.

### Melhorias na extração

Evoluir as regras de identificação das seções do currículo e considerar mais formatos de organização.

### Geração de PDF

Caso fosse necessário um documento com layout totalmente controlado, substituir a impressão do navegador por uma geração de PDF no servidor.

### Limpeza final do projeto

Fazer uma revisão final das entidades e migrations para remover eventuais resíduos de funcionalidades que foram testadas durante o desenvolvimento e posteriormente descartadas.

---

## 11. Considerações finais

O objetivo durante o desenvolvimento foi entregar uma solução funcional dentro do tempo disponível, mantendo uma estrutura que fosse fácil de entender e de continuar evoluindo.

A aplicação atende aos principais fluxos propostos no desafio:

* cadastro manual de candidatos;
* importação de currículo em PDF;
* extração de informações do currículo;
* revisão dos dados antes do cadastro;
* persistência em SQL Server;
* listagem de candidatos;
* visualização dos detalhes;
* exportação do currículo;
* validações;
* testes automatizados;
* documentação para execução.

A inteligência artificial foi utilizada como ferramenta de apoio ao longo desse processo, principalmente para tirar dúvidas, levantar alternativas e acelerar algumas tarefas. As decisões finais, adaptações, testes e validações foram feitas considerando o comportamento real da aplicação durante o desenvolvimento.
