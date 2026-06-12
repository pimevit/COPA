# Bolao Copa

Monorepo do bolao da Copa. O estado atual cobre a base tecnica, o modelo de
dominio/EF Core, migrations e manutencao administrativa de dados, a Tarefa 04
(autenticacao basica com registro, login, JWT e hashing de senha), a Tarefa 05
(Swagger, Bearer no Swagger, FluentValidation,
ProblemDetails e logging HTTP basico), a Tarefa 06 (endpoints publicos de
leitura de partidas), a Tarefa 07 (servico puro de pontuacao no dominio com
testes unitarios), a Tarefa 08 (endpoints autenticados de palpites), a
Tarefa 09 (lancamento de resultado com recalculo de pontos), a Tarefa 10
(endpoint publico de ranking com desempates), a Tarefa 11 (estatisticas do
usuario autenticado), a Tarefa 12 (scaffold do frontend com Tailwind, router,
auth store, cliente HTTP e TanStack Query), a Tarefa 13 (telas de login e
cadastro consumindo a API) e a Tarefa 14 (tela de partidas/jogos do dia).
Tambem cobre a Tarefa 15 (fluxo frontend de palpites com formulario,
historico, janela, cache e visibilidade publica com privacidade reciproca) e a
Tarefa 16 (tela de ranking com polling). O frontend tambem possui logout client-side do
MVP na area autenticada.

Ainda nao ha tela real de estatisticas no frontend.

## Estrutura

```text
backend/
  BolaoCopa.sln
  src/
    BolaoCopa.Api/
    BolaoCopa.Application/
    BolaoCopa.Domain/
    BolaoCopa.Infrastructure/
frontend/
  src/
    api/
    app/
    features/
      auth/
      bets/
      matches/
      ranking/
    routes/
    stores/
    types/
```

## Backend

Os projetos backend apontam para `net8.0`.

Compilar a solucao:

```powershell
dotnet build .\backend\BolaoCopa.sln
```

Executar a API:

```powershell
dotnet run --project .\backend\src\BolaoCopa.Api\BolaoCopa.Api.csproj --urls http://localhost:5000
```

Testar o health check:

```powershell
Invoke-WebRequest http://localhost:5000/health
```

O endpoint `GET /health` retorna HTTP 200 e nao depende de banco,
autenticacao ou servicos de negocio.

### Autenticacao

A API possui autenticacao basica por JWT:

- `POST /auth/register`: cria usuario com `Name`, `Email` e `Password`.
- `POST /auth/login`: valida credenciais e retorna `accessToken`.
- `GET /auth/test`: endpoint tecnico protegido para validar o token.

Configuracoes JWT ficam em `backend/src/BolaoCopa.Api/appsettings.json`:

```json
"Jwt": {
  "Issuer": "BolaoCopa",
  "Audience": "BolaoCopa",
  "Secret": "change-me-development-secret-with-at-least-32-characters",
  "ExpirationMinutes": 60,
  "AdminEmails": [
    "admin@example.com"
  ]
}
```

Em ambiente local/real, sobrescreva o segredo por user secrets ou variavel de
ambiente, sem gravar segredo real no repositorio. Exemplo com variavel de
ambiente:

```powershell
$env:Jwt__Secret = "local-secret-with-at-least-32-characters"
```

Exemplo de registro:

```powershell
Invoke-RestMethod `
  -Method Post `
  -Uri http://localhost:5000/auth/register `
  -ContentType "application/json" `
  -Body '{"name":"Felipe","email":"felipe@example.com","password":"secret123"}'
```

Exemplo de login:

```powershell
$login = Invoke-RestMethod `
  -Method Post `
  -Uri http://localhost:5000/auth/login `
  -ContentType "application/json" `
  -Body '{"email":"felipe@example.com","password":"secret123"}'

$token = $login.accessToken
```

Exemplo de chamada protegida:

```powershell
Invoke-RestMethod `
  -Uri http://localhost:5000/auth/test `
  -Headers @{ Authorization = "Bearer $token" }
```

E-mail duplicado retorna erro claro (`409 Conflict`). Credenciais invalidas no
login retornam `401 Unauthorized`. As respostas publicas nunca expoem
`PasswordHash`.

Para a Tarefa 09, a decisao provisoria de autorizacao e: usuarios cujo e-mail
esta em `Jwt:AdminEmails` recebem role `Admin` no JWT no momento do login. Isso
protege o lancamento de resultado sem criar cadastro completo de roles/admin.

Exemplo de admin local:

```powershell
Invoke-RestMethod `
  -Method Post `
  -Uri http://localhost:5000/auth/register `
  -ContentType "application/json" `
  -Body '{"name":"Admin","email":"admin@example.com","password":"secret123"}'

$adminLogin = Invoke-RestMethod `
  -Method Post `
  -Uri http://localhost:5000/auth/login `
  -ContentType "application/json" `
  -Body '{"email":"admin@example.com","password":"secret123"}'

$adminHeaders = @{ Authorization = "Bearer $($adminLogin.accessToken)" }
```

Fora do escopo atual: recuperacao de senha, gestao completa de roles e
verificacao de e-mail.

### Partidas

A API possui endpoints publicos de leitura de partidas:

- `GET /matches`: lista partidas ordenadas por `matchDate`.
- `GET /matches?stage=Groups`: filtra por fase.
- `GET /matches?status=Scheduled`: filtra por status.
- `GET /matches?stage=Groups&status=Scheduled`: combina filtros.
- `GET /matches/{id}`: retorna o detalhe de uma partida ou `404` quando nao
  existe.

Valores aceitos para `stage`:

```text
Groups, RoundOf16, QuarterFinals, SemiFinals, Final
```

Valores aceitos para `status`:

```text
Scheduled, InProgress, Finished
```

Filtros invalidos retornam `400 application/problem+json` com erro claro. As
respostas incluem `homeTeam` e `awayTeam` ja resolvidos, `matchDate`, `stage`,
`status`, `homeGoals`, `awayGoals` e `isBettingOpen`.

`isBettingOpen` e calculado em UTC comparando o instante atual com
`AllowBetUntil`; a janela fica aberta somente enquanto
`now UTC < AllowBetUntil`. Exatamente no limite, a janela ja esta fechada.
Resultado aparece como `null` enquanto `HomeGoals` e `AwayGoals` nao estiverem
preenchidos.

Exemplo de listagem:

```powershell
Invoke-RestMethod http://localhost:5000/matches
```

Exemplo com filtros:

```powershell
Invoke-RestMethod "http://localhost:5000/matches?stage=Groups&status=Scheduled"
```

Exemplo de detalhe:

```powershell
Invoke-RestMethod http://localhost:5000/matches/1
```

Fora dos endpoints publicos de partidas: criar/editar palpites, ranking e
estatisticas.

### Palpites

A API possui endpoints autenticados para palpites:

- `POST /bets`: cria um palpite para o usuario logado.
- `PUT /bets/{id}`: edita um palpite do proprio usuario.
- `GET /bets/me`: lista o historico de palpites do usuario logado.
- `GET /bets/visibility`: retorna se os palpites do usuario aparecem para
  outros jogadores.
- `PUT /bets/visibility`: atualiza a preferencia global de visibilidade.
- `GET /bets/public?matchId={id}`: lista palpites de jogadores publicos; o
  usuario logado tambem precisa estar publico para acessar.

Todos exigem header Bearer:

```powershell
$headers = @{ Authorization = "Bearer $token" }
```

Criar palpite:

```powershell
Invoke-RestMethod `
  -Method Post `
  -Uri http://localhost:5000/bets `
  -Headers $headers `
  -ContentType "application/json" `
  -Body '{"matchId":1,"homeGoalsPrediction":2,"awayGoalsPrediction":1}'
```

Editar palpite:

```powershell
Invoke-RestMethod `
  -Method Put `
  -Uri http://localhost:5000/bets/1 `
  -Headers $headers `
  -ContentType "application/json" `
  -Body '{"homeGoalsPrediction":3,"awayGoalsPrediction":1}'
```

Listar historico:

```powershell
Invoke-RestMethod `
  -Uri http://localhost:5000/bets/me `
  -Headers $headers
```

Regras implementadas:

- `userId` vem sempre do JWT, nunca do corpo da requisicao.
- Gols previstos devem ser inteiros maiores ou iguais a zero.
- Alteracoes so sao aceitas com `now UTC < Match.AllowBetUntil`.
- Janela fechada retorna `422 application/problem+json`.
- Palpite duplicado para `(UserId, MatchId)` retorna `409 Conflict`.
- `PUT /bets/{id}` retorna `404` quando o palpite nao existe para o usuario
  logado.
- `GET /bets/me` retorna apenas palpites do usuario autenticado, com dados da
  partida e das selecoes.
- Usuarios ficam visiveis por padrao. Quem muda para oculto nao aparece para outros
  jogadores e tambem nao acessa `GET /bets/public`.
- `GET /bets/public` retorna somente `matchId`, `userId`, `userName`, placar
  previsto, pontos, data de criacao e `isCurrentUser`; e-mail nao e exposto.
- `PointsEarned` permanece `0`/valor atual; a Tarefa 08 nao calcula nem
  recalcula pontuacao.

Fora da Tarefa 08 original: ranking, estatisticas e frontend.

### Resultados

A API possui endpoint protegido para lancar ou corrigir resultado de partida:

- `PUT /matches/{id}/result`: define `HomeGoals`, `AwayGoals`, muda
  `Status` para `Finished` e recalcula `PointsEarned` de todos os palpites da
  partida.

O endpoint exige JWT com role `Admin`. Sem token retorna `401 Unauthorized`; com
token sem role admin retorna `403 Forbidden`.

Exemplo:

```powershell
Invoke-RestMethod `
  -Method Put `
  -Uri http://localhost:5000/matches/1/result `
  -Headers $adminHeaders `
  -ContentType "application/json" `
  -Body '{"homeGoals":2,"awayGoals":1}'
```

Resposta:

```json
{
  "id": 1,
  "homeGoals": 2,
  "awayGoals": 1,
  "status": "Finished",
  "recalculatedBets": 3
}
```

Regras implementadas:

- Gols devem ser informados e maiores ou iguais a zero.
- Partida inexistente retorna `404`.
- O recalc usa `ScoreCalculator`, respeitando multiplicador por fase.
- `PointsEarned` e substituido pelo novo valor calculado, nao incrementado.
- Corrigir um resultado ja lancado recalcula todos os palpites afetados.
- Relancar exatamente o mesmo resultado e idempotente.
- Resultado e recalc sao executados na mesma transacao.

Fora da Tarefa 09: estatisticas e integracao com API esportiva.

### Manutencao administrativa de dados

A API possui endpoints protegidos por role `Admin` para cargas controladas de
times e limpeza dos dados de jogo:

- `POST /admin/maintenance/teams/brasileirao-serie-a-2026`: insere/atualiza os
  20 clubes do Campeonato Brasileiro Serie A 2026.
- `POST /admin/maintenance/teams/world-cup-2026`: insere/atualiza as 48
  selecoes classificadas para a Copa 2026.
- `POST /admin/maintenance/recalculate-points`: recalcula `PointsEarned` dos
  palpites de partidas finalizadas usando a regra atual de pontuacao.
- `DELETE /admin/maintenance/application-data`: remove `Bets`, `Matches` e
  `Teams`, preservando `Users` e o historico de migrations.

As importacoes sao idempotentes por `Team.Code`: codigo ausente e inserido;
codigo existente atualiza `Name` e `FlagUrl`. A resposta retorna contadores:
`action`, `insertedTeams`, `updatedTeams`, `deletedBets`, `deletedMatches`,
`deletedTeams`, `recalculatedMatches` e `recalculatedBets`.

Exemplo:

```powershell
Invoke-RestMethod `
  -Method Post `
  -Uri http://localhost:5000/admin/maintenance/teams/world-cup-2026 `
  -Headers $adminHeaders
```

### Ranking

A API possui endpoint publico de ranking global:

- `GET /ranking`: retorna os usuarios com palpites avaliados, ordenados por
  pontos e criterios de desempate.

Exemplo sem autenticacao:

```powershell
Invoke-RestMethod http://localhost:5000/ranking
```

Exemplo com autenticacao opcional para marcar o usuario logado:

```powershell
Invoke-RestMethod `
  -Uri http://localhost:5000/ranking `
  -Headers $headers
```

Resposta:

```json
[
  {
    "position": 1,
    "userId": 10,
    "name": "Felipe",
    "points": 15,
    "isTop3": true,
    "isCurrentUser": true,
    "tieBreakers": {
      "exactScores": 3,
      "outcomeHits": 6,
      "bestHitStreak": 4,
      "firstBetCreatedAtUtc": "2026-06-01T12:00:00Z"
    }
  }
]
```

Regras implementadas:

- O ranking e recalculado on-demand a cada requisicao.
- Nao ha cache, snapshot persistido, WebSocket ou push.
- Entram apenas palpites de partidas com resultado completo
  (`HomeGoals` e `AwayGoals` preenchidos).
- `points` e a soma de `Bet.PointsEarned`; o endpoint nao recalcula pontuacao.
- Se uma regra de pontuacao mudar depois de resultados ja lancados, use
  `/admin/maintenance/recalculate-points` ou o botao administrativo
  "Recalcular pontuacao" antes de conferir o ranking.
- `tieBreakers` expoe os valores usados nos criterios visiveis de desempate.
- Ordenacao:
  1. maior total de pontos;
  2. mais placares exatos;
  3. mais acertos de vencedor/empate;
  4. melhor sequencia de acertos;
  5. menor `CreatedAt` do primeiro palpite avaliado;
  6. menor `userId` como estabilizador tecnico.
- Placar exato significa gols previstos iguais aos gols finais da partida.
- Acerto de vencedor/empate compara o desfecho previsto com o desfecho final;
  placar exato tambem conta como acerto de vencedor/empate.
- Melhor sequencia usa partidas em ordem crescente de `MatchDate` e considera
  acerto como `PointsEarned > 0`, decisao provisoria ligada a duvida 3.2.
- Sem token, ou sem usuario autenticado, `isCurrentUser` fica `false` para
  todos os itens.
- `isTop3` fica `true` para posicoes 1 a 3.

Decisoes do backlog:

- Duvida 3.1: confirmada como 3 pontos base quando o palpite combina
  vencedor/empate correto e gols de exatamente um time.
- Duvida 3.2: "acerto" para sequencia e `PointsEarned > 0`.

Fora da Tarefa 10: frontend, ranking persistido, cache distribuido,
WebSocket/push e recalculo de `PointsEarned`.

### Estatisticas do usuario

A API possui endpoint autenticado de estatisticas individuais:

- `GET /stats/me`: retorna metricas calculadas apenas para o usuario logado.

Exemplo:

```powershell
Invoke-RestMethod `
  -Uri http://localhost:5000/stats/me `
  -Headers $headers
```

Resposta:

```json
{
  "totalPoints": 15,
  "exactScoreCount": 2,
  "winnerHitCount": 5,
  "hitPercentage": 0.75,
  "bestHitStreak": 3,
  "historyEndpoint": "/bets/me"
}
```

Regras implementadas:

- O endpoint exige Bearer token; sem token retorna `401 Unauthorized`.
- O `userId` vem sempre do JWT, nunca de rota, body ou query string.
- Entram apenas palpites do usuario autenticado ligados a partidas com
  resultado completo (`HomeGoals` e `AwayGoals` preenchidos).
- `totalPoints` e a soma de `Bet.PointsEarned`; o endpoint nao recalcula
  pontuacao.
- `exactScoreCount` conta placares previstos iguais aos gols finais.
- `winnerHitCount` conta acertos de desfecho: vitoria mandante, vitoria
  visitante ou empate. Placar exato tambem conta como acerto de desfecho.
- `hitPercentage` e decimal entre `0` e `1`, calculado como acertos /
  palpites avaliados. Sem palpites avaliados, retorna `0`.
- Para percentual e sequencia, "acerto" segue a decisao provisoria da duvida
  3.2: `PointsEarned > 0`.
- `bestHitStreak` usa partidas em ordem crescente de `MatchDate` e `Match.Id`
  como desempate estavel.
- O historico detalhado continua em `GET /bets/me`; `GET /stats/me` retorna
  apenas `historyEndpoint` como referencia curta.

Fora da Tarefa 11: estatisticas comparativas/globais, dashboard/frontend,
ranking e recalculo de `PointsEarned`.

### Pontuacao

A regra de pontuacao da Tarefa 07 fica em
`backend/src/BolaoCopa.Domain/Scoring/ScoreCalculator.cs`. O calculador e puro:
nao depende de EF Core, HTTP, banco, DI obrigatorio ou horario.

Regra base nao cumulativa, sempre usando a maior categoria aplicavel:

- placar exato: 5 pontos
- vencedor/empate correto e gols de exatamente um time: 3 pontos
- apenas vencedor/empate correto: 2 pontos
- apenas gols de exatamente um time: 1 ponto
- erro total: 0 pontos

Multiplicadores por fase:

- `Groups`: x1
- `RoundOf16`: x2
- `QuarterFinals`: x3
- `SemiFinals`: x4
- `Final`: x5

A pontuacao final e `pontuacao base x multiplicador`. Gols negativos e `Stage`
invalido geram `ArgumentOutOfRangeException`; resultado incompleto fica fora do
contrato porque o metodo recebe gols finais como inteiros obrigatorios.

Decisao confirmada: o caso antes ambiguo da duvida 3.1, palpite `2x1` e
resultado `2x0`, retorna 3 pontos base porque combina vencedor correto e gols
de exatamente um time.

Fora da Tarefa 07: ranking e estatisticas.

### Transversais da API

A API possui Swagger/OpenAPI em ambiente de desenvolvimento:

```text
http://localhost:5000/swagger
```

A UI do Swagger permite informar JWT pelo botao `Authorize`. Use o formato:

```text
Bearer <accessToken>
```

O pipeline tambem possui FluentValidation para DTOs de request. Para adicionar
validadores em features futuras, crie um `AbstractValidator<TRequest>` no
assembly da aplicacao; endpoints mapeados no grupo raiz da API passam pelo
filtro de validacao e retornam `400 application/problem+json` quando houver
erros.

Erros de validacao seguem `ValidationProblemDetails`, com `status`, `title`,
`traceId` e erros por campo. Falhas nao tratadas passam pelo middleware global
de excecoes e retornam `ProblemDetails`; em producao, a resposta nao expoe stack
trace.

As requisicoes HTTP sao registradas com metodo, caminho, status code, duracao e
trace id. Os logs nao registram body, header `Authorization`, senha, token JWT
ou `PasswordHash`.

### Banco, migration e dados admin

Pre-requisito: SQL Server LocalDB acessivel pela connection string
`ConnectionStrings:DefaultConnection`. O valor local padrao fica em
`backend/src/BolaoCopa.Api/appsettings.json`:

```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BolaoCopaDb;Trusted_Connection=True;"
```

Esse valor pode ser sobrescrito por user secrets, variavel de ambiente ou pelo
parametro do script abaixo.

Ao iniciar, a API tenta aplicar as migrations automaticamente quando o provider
configurado e relacional, como SQL Server ou Azure SQL. Mantenha a connection
string privada no App Service como `ConnectionStrings__DefaultConnection` ou
como connection string chamada `DefaultConnection`. Se a migration falhar, a API
nao inicia.

Aplicar a migration inicial local:

```powershell
.\scripts\apply-migration.ps1
```

Com connection string temporaria, sem gravar credencial no repositorio:

```powershell
.\scripts\apply-migration.ps1 -ConnectionString "Server=(localdb)\mssqllocaldb;Database=BolaoCopaDb;Trusted_Connection=True;"
```

Comandos separados:

```powershell
dotnet ef database update --project .\backend\src\BolaoCopa.Infrastructure\BolaoCopa.Infrastructure.csproj --startup-project .\backend\src\BolaoCopa.Api\BolaoCopa.Api.csproj
```

Nao existe mais execucao local por `--seed`. As cargas fixas ficam em
`backend/src/BolaoCopa.Infrastructure/Persistence/AdminData` e sao aplicadas
pelos endpoints administrativos ou pelos botoes do painel `/admin`.

## Frontend

O frontend usa Vite + React + TypeScript, Tailwind, React Router, Zustand e
TanStack Query. A base atual inclui autenticacao e a tela de partidas.

Instalar dependencias:

```powershell
cd .\frontend
npm install
```

Configurar a URL base da API em `frontend/.env.local`:

```text
VITE_API_BASE_URL=http://localhost:5000
```

Executar o Vite:

```powershell
npm run dev
```

Compilar o frontend:

```powershell
npm run build
```

Rodar testes do frontend:

```powershell
npm run test
```

### Scaffold, autenticacao e partidas no frontend

Rotas atuais:

- `/`: placeholder publico da base do frontend.
- `/login`: tela publica de login.
- `/register`: tela publica de cadastro.
- `/app`: rota protegida com a tela de partidas. Sem token, redireciona para
  `/login`.
- `/matches`: rota protegida alternativa para a mesma tela de partidas.
- `/ranking`: rota protegida com a tela de ranking.
- `/admin`: rota protegida para usuarios com role `Admin`, com cadastro de
  partidas, resultado e manutencao de dados.

Estrutura principal:

- `src/app/AppProviders.tsx`: registra o `QueryClientProvider`.
- `src/app/router.tsx`: define o `BrowserRouter` e as rotas publicas/protegidas.
- `src/routes/AuthenticatedNav.tsx`: navegacao das rotas autenticadas com acao
  de logout.
- `src/routes/ProtectedRoute.tsx`: bloqueia rotas privadas quando nao ha token.
- `src/routes/PublicAuthRoute.tsx`: redireciona usuario autenticado para `/app`
  ao tentar acessar `/login` ou `/register`.
- `src/stores/authStore.ts`: store Zustand persistida em `localStorage`, com
  `token`, `expiresAtUtc`, `user`, `setSession` e `clearSession`.
- `src/types/auth.ts`: tipos do usuario publico e da sessao retornada por
  `/auth/login` e `/auth/register`.
- `src/api/httpClient.ts`: cliente HTTP central com JSON padrao,
  `Authorization: Bearer <token>` automatico e limpeza da sessao em `401`.
- `src/api/env.ts`: leitura de `VITE_API_BASE_URL`.
- `src/index.css`: Tailwind e estilos globais mobile-first, com suporte tecnico
  a tema escuro por classe `dark`.
- `src/features/auth/api/authApi.ts`: chamadas `POST /auth/login` e
  `POST /auth/register`.
- `src/features/auth/api/authErrors.ts`: mensagens claras para credenciais
  invalidas, e-mail duplicado e erros de validacao.
- `src/features/auth/hooks/useLogout.ts`: fluxo central de logout client-side,
  com limpeza de sessao, cache autenticado e redirecionamento.
- `src/features/auth/pages/LoginPage.tsx`: formulario de login com validacao
  local, TanStack Query, persistencia de sessao e redirecionamento.
- `src/features/auth/pages/RegisterPage.tsx`: formulario de cadastro com
  validacao local, TanStack Query, persistencia de sessao e redirecionamento.
- `src/features/matches/api/matchesApi.ts`: chamada tipada para `GET /matches`.
- `src/features/matches/hooks/useMatches.ts`: hook TanStack Query da lista de
  partidas.
- `src/features/matches/pages/MatchesPage.tsx`: tela responsiva de partidas.
- `src/features/matches/components`: card, selecoes e badge da janela de
  palpite.
- `src/features/matches/utils`: formatacao de data/hora, jogos do dia,
  resultado e labels de fase/status.
- `src/features/bets/api/betsApi.ts`: chamadas de historico, criacao, edicao,
  visibilidade e palpites publicos.
- `src/features/bets/hooks/useBets.ts`: hooks TanStack Query para historico,
  criacao, edicao, visibilidade e palpites publicos.
- `src/features/bets/components`: formulario de palpite, painel por partida,
  historico, controle de privacidade e lista de palpites dos jogadores.
- `src/features/bets/utils`: helpers puros de indexacao por partida,
  create/update, validacao local e erro de API.
- `src/features/user/pages/UserSettingsPage.tsx`: tela `/usuario` com
  configuracao global de privacidade dos palpites.
- `src/features/ranking/api/rankingApi.ts`: chamada tipada para `GET /ranking`.
- `src/features/ranking/hooks/useRanking.ts`: hook TanStack Query com polling
  de 30 segundos.
- `src/features/ranking/pages/RankingPage.tsx`: tela responsiva de ranking.
- `src/features/ranking/components`: Top 3, lista, linha e estados de feedback.
- `src/features/ranking/utils`: formatacao de pontos e posicao.
- `src/features/admin/pages/AdminPage.tsx`: painel admin com cadastro de
  partidas, resultados e botoes de manutencao.
- `src/features/admin/api/adminMaintenanceApi.ts`: chamadas para os endpoints
  administrativos de carga e limpeza de dados.
- `src/types/matches.ts`: contrato TypeScript usado pela tela.
- `src/types/bets.ts`: contratos TypeScript dos palpites.
- `src/types/ranking.ts`: contrato TypeScript usado pela tela de ranking.

Contrato de auth usado pelo frontend:

```ts
type AuthSession = {
  accessToken: string
  expiresAtUtc: string
  user: {
    id: number
    name: string
    email: string
    createdAt: string
  }
}
```

As telas de login e cadastro dependem da API local configurada em
`VITE_API_BASE_URL`. O cadastro usa nome, e-mail e senha, exige senha com pelo
menos 6 caracteres e, pelo contrato atual da API, tambem retorna sessao. Login
e cadastro salvam `accessToken`, `expiresAtUtc` e usuario publico no auth store
persistido em `localStorage`. `PasswordHash` e dados sensiveis nao existem nos
tipos nem sao salvos no frontend.

Fluxo esperado:

- login bem-sucedido redireciona para a rota protegida originalmente solicitada
  ou para `/app`;
- cadastro bem-sucedido salva a sessao e redireciona para `/app`;
- acessar `/login` ou `/register` com sessao ativa redireciona para `/app`;
- refresh do navegador preserva a sessao enquanto o token estiver salvo no
  store;
- clicar em `Sair` nas rotas autenticadas limpa a sessao, remove
  `bolao-copa-auth` do `localStorage`, limpa o cache do TanStack Query e
  redireciona para `/login`;
- apos logout, acessar `/app`, `/matches` ou `/ranking` sem token redireciona
  novamente para `/login`;
- `401` no login aparece como e-mail ou senha invalidos;
- `409` no cadastro aparece como e-mail ja cadastrado;
- erros de validacao aparecem no campo correspondente quando a API informa o
  campo.

Logout no MVP:

- e client-side porque o backend usa JWT stateless;
- nao existe endpoint `POST /auth/logout`;
- nao ha refresh token, blacklist ou invalidacao server-side de JWT nesta
  tarefa;
- a acao e idempotente e pode ser chamada mesmo sem sessao ativa;
- a validacao manual basica e entrar com usuario autenticado, acionar `Sair`,
  confirmar redirecionamento para `/login`, confirmar que
  `localStorage.getItem('bolao-copa-auth')` retorna `null`, atualizar a pagina e
  tentar acessar `/app`, `/matches` ou `/ranking`.

Fora da Tarefa 13 e do logout atual: recuperacao de senha, verificacao de
e-mail, social login, MFA, refresh token, blacklist de JWT, endpoint de logout,
novas features autenticadas, estatisticas no frontend e deploy.

### Tela de partidas e palpites

A tela de partidas implementa a Tarefa 14 e a Tarefa 15. Ela consome
`GET /matches` para listar jogos e `GET /bets/me` para carregar o historico do
usuario autenticado.

Contrato esperado por item da API:

```ts
type MatchListItem = {
  id: number
  homeTeam: { id: number; name: string; code: string; flagUrl?: string | null }
  awayTeam: { id: number; name: string; code: string; flagUrl?: string | null }
  matchDate: string
  stage: string
  status: string
  homeGoals?: number | null
  awayGoals?: number | null
  isBettingOpen: boolean
}
```

Comportamento implementado:

- a lista e ordenada por `matchDate`;
- times exibem bandeira, nome e codigo, com fallback quando `flagUrl` faltar;
- `matchDate` e tratado como UTC e formatado com `Intl.DateTimeFormat`;
- enquanto a duvida 3.4 estiver aberta, a tela usa o fuso local exibido pelo
  navegador do usuario;
- "jogos do dia" compara a data local exibida ao usuario;
- resultado aparece apenas quando `homeGoals` e `awayGoals` existem;
- fase e status usam labels curtos, com fallback seguro para valores
  desconhecidos;
- `isBettingOpen` vem da API e e apenas exibido como janela aberta/fechada;
- cada partida exibe um formulario de palpite;
- quando `isBettingOpen` e falso, o formulario fica bloqueado e a API segue
  sendo a validacao final;
- se ja existir palpite no historico para a partida, o formulario envia
  `PUT /bets/{id}`;
- se nao existir palpite, o formulario envia `POST /bets`;
- gols previstos sao obrigatorios, inteiros e maiores ou iguais a zero;
- apos salvar, o frontend invalida `GET /bets/me` e atualiza os dados da tela;
- erro de janela fechada da API (`422`) aparece no formulario e bloqueia nova
  tentativa local ate recarregar os dados;
- o historico do usuario aparece na mesma tela, usando apenas `GET /bets/me`;
- a configuracao `/usuario` exibe um controle global de privacidade; visivel e
  o padrao;
- jogadores ocultos nao veem palpites de terceiros e nao aparecem na lista;
- jogadores publicos veem imediatamente os palpites de outros jogadores
  publicos, agrupados por partida;
- loading, erro com tentar novamente, lista vazia e layout responsivo estao
  cobertos.

Contratos usados pelo fluxo de palpites:

```ts
type CreateBetRequest = {
  matchId: number
  homeGoalsPrediction: number
  awayGoalsPrediction: number
}

type UpdateBetRequest = {
  homeGoalsPrediction: number
  awayGoalsPrediction: number
}

type BetVisibilityResponse = {
  showBetsPublicly: boolean
}

type PublicBet = {
  matchId: number
  userId: number
  userName: string
  homeGoalsPrediction: number
  awayGoalsPrediction: number
  pointsEarned: number
  createdAt: string
  isCurrentUser: boolean
}
```

Validar manualmente:

1. Suba a API em `http://localhost:5000`.
2. Suba o frontend com `npm run dev`.
3. Entre com usuario autenticado.
4. Acesse `/app` ou `/matches`.
5. Salve um palpite em partida com janela aberta.
6. Edite o mesmo palpite e confirme que o historico atualiza.
7. Tente uma partida com janela fechada e confirme o bloqueio visual.
8. Acesse `/usuario`, deixe a privacidade como oculta e confirme que os palpites dos jogadores
   ficam bloqueados.
9. Volte para publico e confirme que palpites publicos aparecem por partida.

Visibilidade de palpites de terceiros:

- `GET /bets/public` e autenticado e bloqueia com `403` quando o usuario logado
  esta oculto;
- a preferencia global fica em `GET/PUT /bets/visibility` e e alterada na tela
  `/usuario`;
- a visibilidade e imediata para jogadores publicos, sem depender do inicio da
  partida ou fechamento da janela;
- a resposta publica nao expõe e-mail.

Validar localmente:

```powershell
cd .\frontend
npm run test
npm run build
npm run dev
```

Com a API local em `VITE_API_BASE_URL`, acesse `/app`, `/matches` ou `/usuario`
apos login.

Fora da Tarefa 15: calculo de pontos no frontend, ranking, estatisticas,
calendario interativo e deploy.

### Tela de ranking

A tela de ranking implementa a Tarefa 16. Ela consome `GET /ranking` usando o
cliente HTTP central e TanStack Query.

Rota:

```text
/ranking
```

Contrato esperado por item da API:

```ts
type RankingItem = {
  position: number
  userId: number
  name: string
  points: number
  isTop3: boolean
  isCurrentUser: boolean
  tieBreakers: {
    exactScores: number
    outcomeHits: number
    bestHitStreak: number
    firstBetCreatedAtUtc: string
  }
}
```

Comportamento implementado:

- a tela preserva a ordem retornada pela API;
- `position`, `points`, `isTop3` e `isCurrentUser` sao usados diretamente do
  backend;
- os criterios de desempate de cada usuario aparecem em tooltip no desktop e
  em painel expansivel por botao no mobile, usando `tieBreakers`;
- o frontend nao recalcula pontos, desempates nem Top 3;
- Top 3 aparece em destaque quando a API marca `isTop3`;
- a linha do usuario logado aparece destacada quando `isCurrentUser` e
  verdadeiro;
- a busca usa polling simples com `refetchInterval` de 30 segundos;
- polling em background fica desativado pelo TanStack Query;
- loading, erro com retry e lista vazia estao cobertos;
- a navegacao protegida permite acessar `/ranking` a partir da tela de
  partidas;
- nao ha WebSocket, SignalR, push, snapshot persistido ou cache manual.

Validar manualmente:

1. Suba a API em `http://localhost:5000`.
2. Suba o frontend com `npm run dev`.
3. Entre com usuario autenticado.
4. Acesse `/ranking`.
5. Confirme a chamada `GET /ranking`, o Top 3, o destaque do usuario logado e
   a atualizacao periodica sem recarregar a pagina.

Validar localmente:

```powershell
cd .\frontend
npm run test
npm run build
```

Fora da Tarefa 16: backend novo de ranking, WebSocket/push, ranking persistido,
estatisticas, recalculo de pontos e deploy.
