# Bolao Copa - testes para executar com Playwright MCP

Este arquivo define apenas os testes a serem executados depois com o servidor MCP
do Playwright. Nao executar estes testes durante a criacao deste documento.

## Escopo lido

- `bolao-copa-backlog.md`
- `bolao-copa-tarefa-01-etapas.md` ate `bolao-copa-tarefa-16-etapas.md`
- Contratos reais atuais do frontend e backend:
  - Frontend: `/`, `/login`, `/register`, `/app`, `/matches`, `/ranking`
  - Backend: `/health`, `/auth/register`, `/auth/login`, `/auth/test`,
    `/matches`, `/matches/{id}`, `/matches/{id}/result`, `/bets`,
    `/bets/{id}`, `/bets/me`, `/ranking`, `/stats/me`

## Pre-condicoes

1. Banco local com migration e seed aplicados.
2. API em `http://localhost:5000`.
3. Frontend Vite em `http://localhost:5173`.
4. `frontend/.env.local` com:

```text
VITE_API_BASE_URL=http://localhost:5000
```

5. Usar e-mails unicos por execucao:

```text
regularEmail = mcp-user-{timestamp}@example.com
regularPassword = secret123
adminEmail = admin@example.com
adminPassword = secret123
```

6. Quando o teste exigir admin, registrar ou reutilizar `admin@example.com`,
pois esse e-mail recebe role `Admin` pelo `Jwt:AdminEmails`.

## Convencoes MCP

- Use `browser_navigate` para abrir rotas do frontend.
- Use `browser_snapshot` para localizar elementos por acessibilidade.
- Use `browser_fill_form`, `browser_click`, `browser_type` e
  `browser_press_key` para interagir com a UI.
- Use `browser_evaluate` apenas para preparar dados, chamar API local via
  `fetch` ou inspecionar `localStorage`.
- Use `browser_resize` para validacoes responsivas.
- Use `browser_network_requests` para confirmar chamadas HTTP e ausencia de
  WebSocket quando aplicavel.

## Dados auxiliares via MCP

Criar dados de apoio pelo proprio navegador, quando necessario:

```js
// Executar com browser_evaluate em uma pagina do frontend.
async () => {
  const api = 'http://localhost:5000'
  const suffix = Date.now()
  const regular = {
    name: `MCP User ${suffix}`,
    email: `mcp-user-${suffix}@example.com`,
    password: 'secret123',
  }

  const register = await fetch(`${api}/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(regular),
  })
  const regularSession = await register.json()

  return { regular, regularSession }
}
```

Buscar partidas por estado:

```js
async () => {
  const matches = await fetch('http://localhost:5000/matches').then((r) => r.json())
  return {
    open: matches.find((match) => match.isBettingOpen),
    closed: matches.find((match) => !match.isBettingOpen),
    first: matches[0],
    count: matches.length,
  }
}
```

## MCP-00 - Saude da API e carregamento do frontend

Objetivo: confirmar que o alvo local esta acessivel antes dos testes.

Passos:

1. `browser_navigate` para `http://localhost:5173/`.
2. Verificar no snapshot o placeholder publico `Base do frontend`.
3. Usar `browser_evaluate` para chamar `fetch('http://localhost:5000/health')`.

Esperado:

- Frontend renderiza sem erro.
- `/health` retorna HTTP `200`.

## MCP-01 - Rota protegida redireciona sem token

Objetivo: validar `ProtectedRoute`.

Passos:

1. Em `browser_evaluate`, executar `localStorage.removeItem('bolao-copa-auth')`.
2. `browser_navigate` para `http://localhost:5173/matches`.
3. Capturar snapshot.

Esperado:

- URL final fica em `/login`.
- Tela exibe titulo `Entrar`.
- Nao deve exibir `Partidas`.

## MCP-02 - Validacao local do cadastro

Objetivo: validar formulario antes de chamar a API.

Passos:

1. `browser_navigate` para `http://localhost:5173/register`.
2. Clicar em `Criar conta` sem preencher os campos.
3. Capturar snapshot.
4. Preencher e-mail invalido e senha menor que 6 caracteres.
5. Clicar em `Criar conta`.

Esperado:

- Mensagens de campo obrigatorio aparecem.
- E-mail invalido e senha curta bloqueiam submit.
- Nenhuma sessao e gravada em `localStorage`.

## MCP-03 - Cadastro cria sessao e redireciona

Objetivo: validar `/register` integrado a `/auth/register`.

Passos:

1. `browser_navigate` para `http://localhost:5173/register`.
2. Preencher `Nome`, `E-mail` unico e `Senha = secret123`.
3. Clicar em `Criar conta`.
4. Aguardar rota protegida carregar.
5. Em `browser_evaluate`, ler `localStorage.getItem('bolao-copa-auth')`.

Esperado:

- URL final fica em `/app` ou `/matches`.
- Tela exibe `Partidas`.
- `localStorage` contem `token`, `expiresAtUtc` e `user`.
- A resposta/sessao nao contem `passwordHash`.

## MCP-04 - Cadastro duplicado mostra erro claro

Objetivo: validar erro `409` de e-mail ja cadastrado.

Passos:

1. Remover sessao do `localStorage`.
2. `browser_navigate` para `http://localhost:5173/register`.
3. Preencher o mesmo e-mail usado em MCP-03.
4. Clicar em `Criar conta`.

Esperado:

- Tela permanece em `/register`.
- Exibe erro de e-mail ja cadastrado.
- Nao grava nova sessao.

## MCP-05 - Login invalido e login valido

Objetivo: validar `/login` integrado a `/auth/login`.

Passos:

1. Remover sessao do `localStorage`.
2. `browser_navigate` para `http://localhost:5173/login`.
3. Preencher e-mail valido com senha incorreta.
4. Clicar em `Entrar`.
5. Confirmar erro de credenciais invalidas.
6. Corrigir senha para `secret123`.
7. Clicar em `Entrar`.

Esperado:

- Login invalido exibe erro claro e fica em `/login`.
- Login valido redireciona para `/app`.
- Sessao fica persistida.

## MCP-06 - Sessao persiste e rota publica bloqueia usuario logado

Objetivo: validar persistencia Zustand e `PublicAuthRoute`.

Passos:

1. Estar autenticado pelo MCP-05.
2. Recarregar a pagina.
3. Confirmar que continua na area protegida.
4. `browser_navigate` para `http://localhost:5173/login`.

Esperado:

- Refresh nao remove a sessao.
- Acesso a `/login` autenticado redireciona para `/app`.

## MCP-07 - Token invalido limpa sessao em 401

Objetivo: validar tratamento centralizado de `401` no cliente HTTP.

Passos:

1. Injetar sessao invalida:

```js
() => {
  localStorage.setItem('bolao-copa-auth', JSON.stringify({
    state: {
      token: 'invalid-token',
      expiresAtUtc: new Date(Date.now() + 3600000).toISOString(),
      user: { id: 9999, name: 'Invalid', email: 'invalid@example.com', createdAt: new Date().toISOString() },
    },
    version: 0,
  }))
}
```

2. `browser_navigate` para `http://localhost:5173/matches`.
3. Aguardar chamada autenticada de `GET /bets/me`.

Esperado:

- A API retorna `401` para `GET /bets/me`.
- O cliente limpa `bolao-copa-auth`.
- A rota protegida volta para `/login`.

## MCP-08 - Lista de partidas renderiza dados da API

Objetivo: validar Tarefa 14 no frontend.

Passos:

1. Entrar com usuario valido.
2. `browser_navigate` para `http://localhost:5173/matches`.
3. Capturar snapshot.
4. Usar `browser_network_requests` para confirmar `GET /matches`.
5. Usar `browser_evaluate` para buscar `/matches` e validar que a UI mostra
   pelo menos um time retornado pela API.

Esperado:

- Tela exibe `Partidas`.
- Lista aparece ordenada por `matchDate`.
- Cada card mostra times, fase, status, data/hora, resultado ou `A definir`,
  e badge de janela aberta/fechada.
- Historico de palpites aparece na mesma tela.

## MCP-09 - Filtros e detalhe de partidas via API local

Objetivo: validar contratos publicos de partidas usando Playwright MCP.

Passos:

1. Em `browser_evaluate`, chamar:
   - `GET http://localhost:5000/matches?stage=Groups`
   - `GET http://localhost:5000/matches?status=Scheduled`
   - `GET http://localhost:5000/matches?stage=Invalid`
   - `GET http://localhost:5000/matches/{id}` para um id existente
   - `GET http://localhost:5000/matches/999999`

Esperado:

- Filtros validos retornam `200`.
- Filtro invalido retorna `400 application/problem+json`.
- Detalhe existente retorna `200` com `homeTeam`, `awayTeam` e
  `isBettingOpen`.
- Detalhe inexistente retorna `404`.

## MCP-10 - Criar palpite pela UI

Objetivo: validar `POST /bets` a partir do formulario.

Passos:

1. Entrar com usuario valido.
2. Buscar uma partida com `isBettingOpen = true`.
3. `browser_navigate` para `http://localhost:5173/matches`.
4. No card da partida aberta, preencher gols mandante e visitante.
5. Clicar em `Salvar palpite`.

Esperado:

- Botao mostra estado de salvamento.
- Mensagem `Palpite salvo.` aparece.
- Historico passa a listar o palpite.
- `GET /bets/me` retorna o palpite do usuario.
- `pointsEarned` permanece `0` enquanto resultado nao foi lancado.

Pendencia objetiva se nao houver partida aberta no seed atual:

- Registrar que o teste depende de uma partida com `AllowBetUntil` futura.

## MCP-11 - Editar palpite existente pela UI

Objetivo: validar `PUT /bets/{id}` e decisao create/update.

Passos:

1. Usar o palpite criado no MCP-10.
2. Alterar os gols no mesmo card.
3. Clicar em `Atualizar palpite`.
4. Conferir historico.

Esperado:

- Formulario usa `Atualizar palpite`, nao cria duplicado.
- Historico mostra o novo placar previsto.
- `GET /bets/me` continua com apenas um palpite para o mesmo `matchId`.

## MCP-12 - Validacao local de gols do palpite

Objetivo: validar erros de formulario sem chamar API.

Passos:

1. Em partida com janela aberta, preencher `-1` em um campo de gols.
2. Clicar em salvar.
3. Limpar um campo obrigatorio e tentar salvar novamente.

Esperado:

- Gols negativos sao recusados.
- Campo vazio e recusado.
- Nenhuma chamada `POST /bets` ou `PUT /bets/{id}` e enviada para payload
  invalido.

## MCP-13 - Janela fechada bloqueia UI e API

Objetivo: validar regra `now UTC < AllowBetUntil`.

Passos:

1. Buscar uma partida com `isBettingOpen = false`.
2. Abrir `/matches` autenticado.
3. Localizar o card da partida fechada.
4. Verificar formulario desabilitado e mensagem de janela fechada.
5. Em `browser_evaluate`, tentar `POST /bets` para o mesmo `matchId` com token
   valido.

Esperado:

- UI bloqueia campos e submit.
- API retorna `422 application/problem+json`.

Pendencia objetiva se nao houver partida fechada no seed atual:

- Registrar que o teste depende de partida com `AllowBetUntil` no passado.

## MCP-14 - Duplicidade de palpite retorna 409 na API

Objetivo: validar restricao `(UserId, MatchId)`.

Passos:

1. Usar usuario e partida do MCP-10.
2. Em `browser_evaluate`, chamar `POST /bets` novamente para o mesmo
   `matchId`.

Esperado:

- API retorna `409 application/problem+json`.
- UI continua usando `PUT` para edicao do palpite existente.
- Historico nao duplica.

## MCP-15 - Resultado exige admin e recalcula pontos

Objetivo: validar Tarefa 09 e preparar dados para ranking.

Passos:

1. Criar ou logar usuario comum.
2. Criar palpite em uma partida aberta, se possivel.
3. Tentar `PUT /matches/{id}/result` sem token.
4. Tentar com token de usuario comum.
5. Registrar ou logar `admin@example.com`.
6. Tentar com token admin:

```json
{ "homeGoals": 2, "awayGoals": 1 }
```

7. Consultar `GET /matches/{id}`.
8. Consultar `GET /bets/me` do usuario comum.

Esperado:

- Sem token retorna `401`.
- Usuario comum retorna `403`.
- Admin retorna `200`, `status = Finished` e `recalculatedBets >= 1` quando
  havia palpites.
- Partida passa a exibir resultado.
- Palpite afetado tem `pointsEarned` recalculado e substituido, nao somado.

## MCP-16 - Correcao de resultado e idempotencia

Objetivo: validar relancamento seguro de resultado.

Passos:

1. Com token admin, relancar o mesmo resultado do MCP-15.
2. Consultar o palpite afetado.
3. Corrigir para outro resultado.
4. Consultar novamente.

Esperado:

- Relancar o mesmo resultado nao duplica registros nem altera indevidamente os
  pontos.
- Corrigir resultado recalcula `pointsEarned`.
- `recalculatedBets` corresponde aos palpites da partida.

## MCP-17 - Ranking renderiza Top 3, usuario atual e polling

Objetivo: validar Tarefa 16 no frontend.

Passos:

1. Estar autenticado com usuario que tenha palpite avaliado.
2. `browser_navigate` para `http://localhost:5173/ranking`.
3. Capturar snapshot.
4. Confirmar chamada `GET /ranking`.
5. Esperar mais de 30 segundos.
6. Confirmar nova chamada `GET /ranking` sem reload de pagina.
7. Verificar `browser_network_requests` para confirmar ausencia de WebSocket.

Esperado:

- Tela exibe `Ranking`.
- A ordem recebida da API e preservada.
- Top 3 fica destacado quando `isTop3 = true`.
- Usuario logado fica destacado quando `isCurrentUser = true`.
- Polling usa HTTP `GET /ranking`; nao usa WebSocket, SignalR ou push.

## MCP-18 - Ranking publico marca isCurrentUser falso sem token

Objetivo: validar contrato opcional de autenticacao do ranking.

Passos:

1. Remover sessao do `localStorage`.
2. Em `browser_evaluate`, chamar `GET http://localhost:5000/ranking` sem
   header Authorization.

Esperado:

- API retorna `200`.
- Todos os itens tem `isCurrentUser = false`.

## MCP-19 - Estatisticas do usuario via API

Objetivo: validar Tarefa 11 mesmo sem tela frontend.

Passos:

1. Chamar `GET /stats/me` sem token.
2. Chamar `GET /stats/me` com token de usuario valido que tenha dados
   avaliados.
3. Chamar `GET /stats/me` com usuario sem palpites avaliados.

Esperado:

- Sem token retorna `401`.
- Usuario com dados retorna `totalPoints`, `exactScoreCount`,
  `winnerHitCount`, `hitPercentage`, `bestHitStreak` e
  `historyEndpoint = /bets/me`.
- `hitPercentage` fica entre `0` e `1`.
- Usuario sem palpites avaliados retorna metricas zeradas.
- Dados de outro usuario nao entram na resposta.

## MCP-20 - Responsividade das telas principais

Objetivo: validar que textos e controles nao se sobrepoem.

Passos:

1. Com usuario autenticado, executar:
   - `browser_resize` para `390x844`
   - abrir `/matches`
   - capturar screenshot/snapshot
   - abrir `/ranking`
   - capturar screenshot/snapshot
2. Repetir com `1366x768`.

Esperado:

- Login, cadastro, partidas, formulario de palpite, historico e ranking cabem
  no viewport.
- Nao ha sobreposicao incoerente entre nomes, bandeiras, placares, badges e
  botoes.
- Botoes seguem clicaveis em mobile.

## MCP-21 - Visibilidade de palpites de terceiros permanece bloqueada

Objetivo: validar a pendencia documentada da Tarefa 15.

Passos:

1. Inspecionar a UI em `/matches`.
2. Inspecionar chamadas de rede.
3. Confirmar se existe alguma chamada para endpoint de palpites de terceiros.

Esperado:

- Nao ha chamada para endpoint inexistente de terceiros.
- A aplicacao nao exibe palpites de outros usuarios antes do contrato existir.
- Registrar pendencia: backend ainda nao possui endpoint aprovado para listar
  palpites de terceiros.

## MCP-22 - Swagger e ProblemDetails basico via navegador

Objetivo: validar transversais sem executar testes unitarios.

Passos:

1. `browser_navigate` para `http://localhost:5000/swagger`.
2. Confirmar que a UI lista endpoints.
3. Em `browser_evaluate`, enviar request invalido:
   - `POST /auth/register` com e-mail invalido e senha curta.
   - `POST /bets` sem token.

Esperado:

- Swagger abre em desenvolvimento.
- Request invalido retorna `400 application/problem+json` ou
  `ValidationProblemDetails`.
- Endpoint protegido sem token retorna `401`.
- Respostas nao expoem stack trace nem `passwordHash`.

## Registro de resultado esperado

Ao executar cada teste com MCP, registrar no relatorio final:

```text
Teste:
Status: passou | falhou | bloqueado
Evidencia MCP: snapshot, network request ou payload de browser_evaluate
Observacao:
```

Falhas de infraestrutura devem ser registradas separadas de falhas de codigo:

- API nao subiu.
- Banco local indisponivel.
- Seed sem partida aberta/fechada necessaria para o caso.
- Frontend sem `VITE_API_BASE_URL`.
