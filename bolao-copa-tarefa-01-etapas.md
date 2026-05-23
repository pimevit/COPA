# Tarefa 01 - Divisao em etapas independentes

Fonte: `C:\Repo\bolao-copa-backlog.md`, secao **Tarefa 01 - Estrutura do monorepo e scaffolding**.

Este arquivo quebra a Tarefa 01 em etapas pequenas, fechadas e implementaveis separadamente. Nenhuma etapa abaixo deve implementar regra de negocio, banco, autenticacao, Tailwind, entidades ou endpoints de negocio.

## Escopo original da Tarefa 01

Criar a base fisica do projeto:

- `/backend` com solucao .NET e projetos `Api`, `Application`, `Domain` e `Infrastructure`.
- `/frontend` com Vite + React + TypeScript.
- `.gitignore`, `.editorconfig` e `README.md` na raiz.
- Endpoint `GET /health` retornando `200`.
- Frontend iniciando com tela vazia.
- Referencias corretas entre camadas: `Api -> Application`, `Application -> Domain`, `Infrastructure -> Domain`, `Api -> Infrastructure`.

## Regra de independencia

Cada etapa deve poder ser executada sozinha, sem assumir que outra etapa ja foi feita.

Para preservar essa independencia:

- A etapa deve inspecionar o estado atual antes de alterar arquivos.
- A etapa deve criar apenas os diretorios ou arquivos minimos necessarios ao seu proprio objetivo.
- A etapa deve ser idempotente: se algo ja existir, ajustar apenas o necessario e preservar conteudo existente.
- A etapa nao deve chamar escopos de outras etapas para "completar" o projeto inteiro.
- Validacoes globais da Tarefa 01 so devem ser exigidas quando todas as etapas forem executadas.

## Etapa 01 - Contrato fisico do monorepo

**Objetivo:** Garantir que a raiz do projeto tenha os diretorios base esperados.

**Escopo:**

- Criar `/backend` se nao existir.
- Criar `/frontend` se nao existir.
- Adicionar apenas arquivos sentinela se necessario para manter diretorios vazios versionaveis.

**Fora do escopo:**

- Criar solucao .NET.
- Criar app Vite.
- Criar README, `.gitignore` ou `.editorconfig`.
- Adicionar codigo de backend ou frontend.

**Criterios de aceite:**

- `/backend` existe.
- `/frontend` existe.
- A etapa pode ser executada novamente sem recriar ou apagar conteudo.

**Prompt sugerido:**

```text
Garanta apenas a estrutura fisica inicial do monorepo: crie os diretorios /backend e /frontend caso nao existam. Preserve qualquer conteudo existente. Nao crie solucao .NET, app Vite, README, .gitignore, .editorconfig nem codigo de aplicacao.
```

## Etapa 02 - Scaffold da solucao backend

**Objetivo:** Criar a solucao .NET e os projetos base da Clean Architecture simplificada.

**Escopo:**

- Criar `/backend/BolaoCopa.sln` se nao existir.
- Criar projetos:
  - `/backend/src/BolaoCopa.Api`
  - `/backend/src/BolaoCopa.Application`
  - `/backend/src/BolaoCopa.Domain`
  - `/backend/src/BolaoCopa.Infrastructure`
- Configurar referencias:
  - `BolaoCopa.Api -> BolaoCopa.Application`
  - `BolaoCopa.Api -> BolaoCopa.Infrastructure`
  - `BolaoCopa.Application -> BolaoCopa.Domain`
  - `BolaoCopa.Infrastructure -> BolaoCopa.Domain`
- Garantir que a solucao compile.

**Fora do escopo:**

- Endpoint `GET /health`.
- Entidades de dominio.
- EF Core, banco, migrations ou seed.
- Autenticacao.
- Regras de negocio.
- Frontend.

**Criterios de aceite:**

- `dotnet build C:\Repo\backend\BolaoCopa.sln` compila sem erros.
- `dotnet sln C:\Repo\backend\BolaoCopa.sln list` mostra os 4 projetos.
- As referencias entre projetos seguem a direcao definida.

**Prompt sugerido:**

```text
Crie somente o scaffold do backend em /backend. Gere a solucao BolaoCopa.sln e os projetos BolaoCopa.Api, BolaoCopa.Application, BolaoCopa.Domain e BolaoCopa.Infrastructure em /backend/src. Configure as referencias: Api -> Application, Api -> Infrastructure, Application -> Domain, Infrastructure -> Domain. Garanta que dotnet build compile. Nao crie endpoint de health, entidades, banco, autenticacao, regras de negocio nem frontend.
```

## Etapa 03 - Endpoint tecnico de health

**Objetivo:** Expor um endpoint tecnico minimo para validar que a API sobe.

**Escopo:**

- Garantir que `BolaoCopa.Api` exista.
- Criar `GET /health` retornando HTTP `200`.
- Manter o endpoint sem dependencia de banco, autenticacao ou servicos de negocio.
- Se a solucao backend nao existir, criar apenas o minimo necessario para executar o projeto `BolaoCopa.Api`.

**Fora do escopo:**

- Criar os projetos `Application`, `Domain` ou `Infrastructure`, exceto se ja existirem e precisarem ser preservados.
- Criar health check com banco.
- Criar Swagger customizado.
- Criar endpoints de negocio.

**Criterios de aceite:**

- `dotnet run --project C:\Repo\backend\src\BolaoCopa.Api\BolaoCopa.Api.csproj --urls http://localhost:5000` sobe a API.
- `GET http://localhost:5000/health` retorna `200`.
- O endpoint nao acessa banco nem regras de negocio.

**Prompt sugerido:**

```text
Adicione apenas um endpoint tecnico GET /health no projeto BolaoCopa.Api, retornando HTTP 200. Se o projeto Api ainda nao existir, crie o minimo necessario em /backend/src/BolaoCopa.Api para a API subir. Nao implemente banco, autenticacao, entidades, Swagger customizado, regras de negocio nem outros endpoints.
```

## Etapa 04 - Scaffold do frontend Vite React TypeScript

**Objetivo:** Criar o app frontend base com Vite, React e TypeScript.

**Escopo:**

- Criar `/frontend/package.json`.
- Configurar Vite + React + TypeScript.
- Criar estrutura minima de `src`.
- Renderizar uma tela vazia ou neutra, sem layout final.
- Garantir que `npm run dev` inicie o app.

**Fora do escopo:**

- Tailwind.
- Router.
- API client.
- Auth store.
- Telas de autenticacao, partidas, palpites, ranking ou estatisticas.
- Integração com backend.

**Criterios de aceite:**

- `npm install` executa em `/frontend`.
- `npm run dev` sobe o Vite.
- A aplicacao abre sem erro de runtime.
- Nao ha dependencia de API para a primeira renderizacao.

**Prompt sugerido:**

```text
Crie somente o scaffold do frontend em /frontend usando Vite + React + TypeScript. A tela inicial deve ser vazia ou neutra, sem layout final. Garanta que npm install e npm run dev funcionem. Nao adicione Tailwind, router, API client, auth store, telas de negocio nem integracao com backend.
```

## Etapa 05 - Arquivos raiz de padronizacao

**Objetivo:** Adicionar arquivos raiz de padronizacao do repositorio.

**Escopo:**

- Criar ou atualizar `.gitignore` na raiz.
- Criar ou atualizar `.editorconfig` na raiz.
- Cobrir saidas comuns de .NET, Node, Vite, logs, variaveis locais e arquivos de sistema.
- Preservar entradas existentes.

**Fora do escopo:**

- README.
- Alterar codigo.
- Rodar formatadores amplos.
- Configurar lint, CI ou hooks.

**Criterios de aceite:**

- `.gitignore` existe e cobre pelo menos `bin/`, `obj/`, `node_modules/`, `dist/`, `.env*`, logs e arquivos locais de IDE.
- `.editorconfig` existe com regras basicas para C#, TypeScript, JSON e Markdown.
- Nenhum conteudo existente foi removido sem necessidade.

**Prompt sugerido:**

```text
Crie ou atualize apenas os arquivos raiz .gitignore e .editorconfig. O .gitignore deve cobrir artefatos comuns de .NET, Node/Vite, logs, .env e IDE. O .editorconfig deve ter regras basicas para C#, TypeScript, JSON e Markdown. Preserve entradas existentes. Nao altere codigo, README, CI, lint ou hooks.
```

## Etapa 06 - README raiz com comandos de execucao

**Objetivo:** Documentar os comandos minimos para build e execucao local da Tarefa 01.

**Escopo:**

- Criar ou atualizar `README.md` na raiz.
- Documentar estrutura esperada do monorepo.
- Documentar comandos:
  - build do backend;
  - execucao da API;
  - teste manual do `GET /health`;
  - instalacao e execucao do frontend.
- Declarar explicitamente que a Tarefa 01 nao inclui negocio, banco, autenticacao ou Tailwind.

**Fora do escopo:**

- Documentar regras de negocio futuras.
- Criar scripts automatizados.
- Alterar backend ou frontend.

**Criterios de aceite:**

- `README.md` existe.
- O README permite que outra pessoa rode backend e frontend localmente.
- O README nao descreve funcionalidades ainda nao implementadas como se estivessem prontas.

**Prompt sugerido:**

```text
Crie ou atualize apenas o README.md raiz com a estrutura do monorepo e comandos locais da Tarefa 01: dotnet build da solucao, dotnet run da API, teste do GET /health e npm install/npm run dev do frontend. Deixe claro que ainda nao ha banco, autenticacao, Tailwind, entidades ou regras de negocio. Nao altere codigo.
```

## Etapa 07 - Validacao final da Tarefa 01

**Objetivo:** Verificar se o conjunto atual do repositorio atende aos criterios originais da Tarefa 01.

**Escopo:**

- Rodar build do backend.
- Subir a API e validar `GET /health`.
- Rodar instalacao/build ou dev server do frontend conforme aplicavel.
- Conferir referencias entre camadas.
- Registrar pendencias encontradas sem implementar novas features.

**Fora do escopo:**

- Corrigir features fora da Tarefa 01.
- Adicionar entidades, banco, autenticacao, Tailwind ou endpoints de negocio.
- Reorganizar arquitetura alem do necessario para a Tarefa 01.

**Criterios de aceite:**

- Resultado dos comandos de validacao registrado.
- Lista objetiva de pendencias, se houver.
- Nenhuma feature fora do escopo foi criada durante a validacao.

**Prompt sugerido:**

```text
Valide somente a Tarefa 01 no estado atual do repositorio. Rode o build do backend, confirme as referencias entre projetos, suba a API e teste GET /health, e valide que o frontend Vite inicia. Se algo falhar, registre a pendencia de forma objetiva. Nao implemente entidades, banco, autenticacao, Tailwind, regras de negocio nem endpoints adicionais.
```

## Mapeamento do escopo original para as etapas

| Item da Tarefa 01 | Etapa responsavel |
|---|---|
| Criar `/backend` e `/frontend` | Etapa 01 |
| Solucao .NET e 4 projetos | Etapa 02 |
| Referencias entre camadas | Etapa 02 |
| Endpoint `GET /health` | Etapa 03 |
| App Vite + React + TypeScript | Etapa 04 |
| `.gitignore` e `.editorconfig` | Etapa 05 |
| `README.md` com comandos | Etapa 06 |
| Validacao dos criterios de aceite originais | Etapa 07 |

