# Falhas dos testes Playwright MCP - Bolao Copa

Execucao em: 2026-05-23

Ambiente iniciado antes dos testes:

- Backend: `http://localhost:5000`
- Frontend: `http://localhost:5173`
- Migration/seed: executado com sucesso; banco ja estava atualizado.
- `frontend/.env.local`: criado com `VITE_API_BASE_URL=http://localhost:5000`.

Resumo:

- Passaram: MCP-01, MCP-02, MCP-22.
- Falharam: MCP-00, MCP-03, MCP-04, MCP-05, MCP-06, MCP-07, MCP-08, MCP-09, MCP-13, MCP-17, MCP-18, MCP-19, MCP-20, MCP-21.
- Bloqueados: MCP-10, MCP-11, MCP-12, MCP-14, MCP-15, MCP-16.

## Correcoes aplicadas em 2026-05-23

- Corrigido CORS da API para permitir o frontend local em
  `http://localhost:5173` e `http://127.0.0.1:5173`.
- Corrigido `POST /auth/register` para retornar sessao completa
  (`accessToken`, `expiresAtUtc`, `user`), igual ao contrato usado pelo
  frontend.
- Adicionado teste de preflight CORS em
  `backend/tests/BolaoCopa.Tests/Transversals/OpenApiAndValidationTests.cs`.
- Atualizado teste de `AuthService.RegisterAsync` para validar token no
  cadastro.

Validacao apos correcoes:

- `dotnet test .\backend\BolaoCopa.sln`: 85 testes passaram.
- `npm run test`: 27 testes passaram.
- `npm run build`: passou.
- Preflight real `OPTIONS /auth/register` com origem `http://localhost:5173`
  retornou `204` com `Access-Control-Allow-Origin`.
- Rechecagem Playwright MCP passou para cadastro, cadastro duplicado, login,
  token invalido, listagem/filtros de partidas, ranking publico e
  criar/editar palpite pela UI.

## Falha raiz observada

O navegador bloqueou as chamadas do frontend para a API por CORS. Evidencia do
console Playwright:

```text
Access to fetch at 'http://localhost:5000/auth/register' from origin 'http://localhost:5173' has been blocked by CORS policy: No 'Access-Control-Allow-Origin' header is present on the requested resource.
Access to fetch at 'http://localhost:5000/auth/login' from origin 'http://localhost:5173' has been blocked by CORS policy: No 'Access-Control-Allow-Origin' header is present on the requested resource.
Access to fetch at 'http://localhost:5000/matches' from origin 'http://localhost:5173' has been blocked by CORS policy: No 'Access-Control-Allow-Origin' header is present on the requested resource.
```

Confirmacao adicional:

- `curl.exe -i http://localhost:5000/health` retornou `200 OK`.
- `curl.exe -I http://localhost:5173/` retornou `200 OK`.
- `OPTIONS /auth/register` com `Origin: http://localhost:5173` retornou `405 Method Not Allowed`, sem header `Access-Control-Allow-Origin`.
- `backend/src/BolaoCopa.Api/Program.cs` nao configura `AddCors`/`UseCors`.

## Testes que falharam

### MCP-00 - Saude da API e carregamento do frontend

Status: falhou

Evidencia MCP:

```text
page.evaluate: TypeError: Failed to fetch
GET http://localhost:5000/health bloqueado por CORS no navegador.
```

Observacao: o `/health` responde `200` via `curl.exe`, mas falha quando chamado
por `fetch` a partir de `http://localhost:5173`, que e o comportamento exigido
pelo roteiro MCP.

### MCP-03 - Cadastro cria sessao e redireciona

Status: falhou

Evidencia MCP:

```text
Timeout esperando redirecionamento para /app ou /matches.
Console: fetch http://localhost:5000/auth/register bloqueado por CORS.
```

Observacao: a tela permaneceu no cadastro e exibiu erro generico de autenticacao.

### MCP-04 - Cadastro duplicado mostra erro claro

Status: falhou

Evidencia MCP:

```text
URL final: http://localhost:5173/register
Mensagem exibida: Nao foi possivel concluir a autenticacao. Tente novamente.
Console: fetch http://localhost:5000/auth/register bloqueado por CORS.
```

Observacao: o teste nao conseguiu atingir o contrato `409` de e-mail duplicado.

### MCP-05 - Login invalido e login valido

Status: falhou

Evidencia MCP:

```text
Timeout esperando redirecionamento para /app.
Console: fetch http://localhost:5000/auth/login bloqueado por CORS.
```

Observacao: tanto login invalido quanto login valido ficaram impedidos pela
falha de comunicacao browser -> API.

### MCP-06 - Sessao persiste e rota publica bloqueia usuario logado

Status: falhou

Evidencia MCP:

```text
Timeout esperando heading Partidas.
```

Observacao: depende de sessao criada no MCP-05, que falhou por CORS.

### MCP-07 - Token invalido limpa sessao em 401

Status: falhou

Evidencia MCP:

```text
Timeout esperando redirecionamento para /login.
Console: fetch http://localhost:5000/bets/me bloqueado por CORS.
```

Observacao: o cliente nao chegou a receber o `401`, porque a requisicao foi
barrada antes pelo navegador.

### MCP-08 - Lista de partidas renderiza dados da API

Status: falhou

Evidencia MCP:

```text
Timeout no fluxo autenticado.
Console: fetch http://localhost:5000/matches e /bets/me bloqueados por CORS.
```

Observacao: `GET /matches` retorna dados via `curl.exe`, mas a UI nao consegue
buscar a lista no navegador.

### MCP-09 - Filtros e detalhe de partidas via API local

Status: falhou

Evidencia MCP:

```text
page.evaluate: TypeError: Failed to fetch
```

Observacao: as chamadas `browser_evaluate` a partir do frontend tambem ficam
sujeitas ao CORS, conforme o roteiro.

### MCP-13 - Janela fechada bloqueia UI e API

Status: falhou

Evidencia MCP:

```text
page.evaluate: TypeError: Failed to fetch
```

Observacao: a execucao MCP falhou por CORS antes de avaliar a ausencia de
partida fechada. Em consulta externa via `curl.exe`, o seed atual retornou 24
partidas e todas com `isBettingOpen: true`.

### MCP-17 - Ranking renderiza Top 3, usuario atual e polling

Status: falhou

Evidencia MCP:

```text
Timeout no login/redirecionamento antes de abrir /ranking autenticado.
Console: fetch http://localhost:5000/auth/login bloqueado por CORS.
```

Observacao: o teste depende de usuario autenticado e palpite avaliado.

### MCP-18 - Ranking publico marca isCurrentUser falso sem token

Status: falhou

Evidencia MCP:

```text
page.evaluate: TypeError: Failed to fetch
Console: fetch http://localhost:5000/ranking bloqueado por CORS.
```

Observacao: o endpoint publico nao foi alcançado pelo contexto do frontend.

### MCP-19 - Estatisticas do usuario via API

Status: falhou

Evidencia MCP:

```text
page.evaluate: TypeError: Failed to fetch
Console: fetch http://localhost:5000/stats/me bloqueado por CORS.
```

Observacao: o teste tambem depende de usuario com dados avaliados, que nao foi
preparado porque cadastro/login/palpite falharam.

### MCP-20 - Responsividade das telas principais

Status: falhou

Evidencia MCP:

```text
Timeout no login/redirecionamento antes de validar /matches e /ranking.
```

Observacao: as telas principais protegidas nao carregaram em estado autenticado.

### MCP-21 - Visibilidade de palpites de terceiros permanece bloqueada

Status: falhou

Evidencia MCP:

```text
Timeout no login/redirecionamento antes de inspecionar /matches autenticado.
```

Observacao: depende de login funcional no frontend.

## Testes bloqueados

### MCP-10 - Criar palpite pela UI

Status: bloqueado

Observacao: depende de cadastro/login funcional no frontend e de `GET /matches`
acessivel pela UI. O CORS impediu a preparacao do usuario e a descoberta da
partida aberta pelo roteiro MCP.

### MCP-11 - Editar palpite existente pela UI

Status: bloqueado

Observacao: depende do palpite criado no MCP-10.

### MCP-12 - Validacao local de gols do palpite

Status: bloqueado

Observacao: depende da tela de partidas autenticada e de partida aberta
carregada pela UI.

### MCP-14 - Duplicidade de palpite retorna 409 na API

Status: bloqueado

Observacao: depende do palpite criado no MCP-10.

### MCP-15 - Resultado exige admin e recalcula pontos

Status: bloqueado

Observacao: depende de usuario comum com palpite criado e de admin logado.

### MCP-16 - Correcao de resultado e idempotencia

Status: bloqueado

Observacao: depende do resultado lancado no MCP-15.
