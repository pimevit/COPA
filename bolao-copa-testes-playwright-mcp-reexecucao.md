# Reexecucao dos testes Playwright MCP - Bolao Copa

Execucao em: 2026-05-23

Ambiente:

- Migration/seed executados com sucesso.
- Backend iniciado em `http://localhost:5000`.
- Frontend Vite iniciado em `http://localhost:5173`.
- `frontend/.env.local` configurado com `VITE_API_BASE_URL=http://localhost:5000`.
- `GET /health` respondeu `200` com CORS para `http://localhost:5173`.

## Resultado consolidado

- Passaram: 22
- Bloqueados: 1
- Falhas funcionais confirmadas: 0

Bloqueado:

- `MCP-13 - Janela fechada bloqueia UI e API`
  - Motivo: o seed atual retornou 24 partidas e nenhuma com
    `isBettingOpen = false`.
  - Pendencia objetiva: o teste depende de partida com `AllowBetUntil` no
    passado.

## Observacoes da execucao

A passada completa inicialmente marcou 5 falhas, mas todas foram rechecadas de
forma direcionada e passaram:

- `MCP-08`: passou no recheck; `/matches` foi chamado e a UI exibiu time da
  API e historico de palpites.
- `MCP-12`: passou no recheck; gols negativos e campo vazio foram recusados sem
  chamada `POST /bets` ou `PUT /bets/{id}`.
- `MCP-14`: passou no recheck; duplicidade retornou `409` e o historico manteve
  apenas um palpite para a partida.
- `MCP-17`: passou no recheck; polling fez novas chamadas HTTP `GET /ranking`.
  O unico WebSocket observado foi o HMR do Vite (`ws://localhost:5173`), nao um
  WebSocket da aplicacao/API.
- `MCP-21`: passou no recheck; chamadas reais para a API de palpites ficaram em
  `GET /bets/me`; nao houve endpoint de palpites de terceiros.

## Evidencias principais

- `MCP-00`: frontend carregou `Base do frontend`; `/health = 200`.
- `MCP-03`: cadastro redirecionou para `/app` com `token`, `expiresAtUtc` e
  `user` no `localStorage`.
- `MCP-05`: login invalido exibiu erro; login valido redirecionou para `/app`.
- `MCP-07`: token invalido recebeu `401`, limpou `bolao-copa-auth` e voltou
  para `/login`.
- `MCP-10`: palpite criado pela UI; `pointsEarned = 0`.
- `MCP-11`: palpite editado pela UI sem duplicar historico.
- `MCP-15`: resultado sem token retornou `401`; usuario comum retornou `403`;
  admin retornou `200` com `status = Finished`.
- `MCP-16`: relancar resultado foi idempotente; corrigir resultado recalculou
  pontos.
- `MCP-18`: ranking publico sem token retornou `200` e `isCurrentUser = false`.
- `MCP-20`: screenshots responsivos gerados em `.playwright-mcp/mcp-full-*.png`
  sem overflow horizontal incoerente.
- `MCP-22`: Swagger abriu; request invalido retornou
  `400 application/problem+json`; `POST /bets` sem token retornou `401`.

