# Bolão da Copa do Mundo — Análise e Backlog para IA de Desenvolvimento

> Documento de planejamento (RFC-lite) para decompor o projeto em tarefas pequenas, fechadas e executáveis por Codex / Claude Code.
> Nenhuma tarefa foi implementada. O código aparece apenas como contrato/exemplo onde reduz ambiguidade.

---

## 1. Resumo do projeto

Aplicação web de bolão da Copa do Mundo. O usuário se cadastra, faz palpites de placar nas partidas, e acumula pontos conforme a precisão do palpite. Há um ranking global em tempo real, multiplicador de pontos por fase do torneio, regras de fechamento de palpites por horário, visibilidade controlada por preferência do usuário e estatísticas individuais.

Stack-alvo: backend **ASP.NET Core Web API + EF Core + JWT**, frontend **React + Vite + TypeScript + Tailwind**, banco **SQL Server**, deploy em **Azure free tier**. Prioridade explícita: simplicidade, baixo custo, sem overengineering, com espaço para evolução futura.

---

## 2. Requisitos identificados

### 2.1 Requisitos funcionais (MVP)

- **Autenticação**: cadastro, login, emissão de JWT. (Recuperação de senha fica para depois.)
- **Partidas**: listar jogos com seleções, data/hora, fase, status e resultado final.
- **Palpites**: criar, editar antes do fechamento e consultar histórico.
- **Fechamento de palpites**: bloqueio automático 15 min antes (fase de grupos) e 30 min antes (mata-mata). API deve recusar alterações após o limite.
- **Pontuação**: cálculo automático conforme regra de placar (ver 2.3).
- **Multiplicador por fase**: aplicado sobre a pontuação base.
- **Ranking global**: ordenação por pontos, com Top 3 e usuário logado em destaque, atualização automática.
- **Visibilidade de palpites**: jogadores ficam visíveis por padrão; quem oculta seus palpites deixa de aparecer e também deixa de ver palpites públicos.
- **Estatísticas do usuário**: total de pontos, nº de placares exatos, nº de acertos de vencedor, percentual de acertos, melhor sequência de acertos, histórico de palpites.
- **Desempate de ranking**: critérios em cascata (ver 2.3).

### 2.2 Requisitos técnicos

- Backend: ASP.NET Core Web API (.NET LTS atual), EF Core, JWT, Swagger habilitado, FluentValidation, logging básico. Clean Architecture simplificada. Repository Pattern só se necessário. Docker opcional.
- Frontend: React SPA com Vite + TypeScript, Tailwind, gerência de estado leve (Context API ou Zustand), TanStack Query para data fetching, layout mobile-first, tema escuro opcional.
- Banco: relacional, SQL Server/LocalDB em desenvolvimento.
- Deploy: Azure App Service Free (API) + Azure Static Web Apps (frontend) + Azure SQL Free.
- Convenção de código (preferência do usuário): nomes em inglês, verbos no infinitivo (`fetchUser`, `calculateScore`), baixo acoplamento, tratamento de erro adequado, sem duplicação, funções pequenas.

### 2.3 Regras de negócio

**Pontuação base (não cumulativa — prevalece o maior acerto):**

| Situação | Pontos |
|---|---|
| Placar exato | 5 |
| Acertou vencedor/empate e os gols de apenas um dos times | 3 |
| Acertou apenas o vencedor/empate | 2 |
| Acertou os gols de apenas um dos times | 1 |
| Errou tudo | 0 |

**Multiplicador por fase (aplicado sobre a pontuação base):**

| Fase | Multiplicador |
|---|---|
| Grupos | x1 |
| Oitavas | x2 |
| Quartas | x3 |
| Semifinal | x4 |
| Final | x5 |

- Janela de palpite: até 15 min antes (grupos) / 30 min antes (mata-mata).
- Visibilidade: oculto antes do início; público após.
- Desempate (cascata): 1) mais placares exatos; 2) mais acertos de vencedor; 3) melhor sequência de acertos consecutivos; 4) quem cadastrou os palpites primeiro.

### 2.4 Restrições importantes

- Custo zero / free tier como meta de infraestrutura — influencia escolhas (ex.: evitar serviços pagos, jobs caros, WebSockets dedicados).
- Sem overengineering: a arquitetura deve ser simples e fácil de manter.
- "Tempo real" do ranking deve ser viável em free tier (provavelmente polling/refetch, não WebSocket).

---

## 3. Dúvidas ou informações faltantes

Itens abaixo são bloqueantes ou de alto risco de retrabalho. Pergunta formulada para envio direto ao PM/tech lead.

### 3.1 — Regra de pontuação combinada — RESOLVIDO
**Decisão:** No exemplo Brasil 2x1 (palpite) vs Brasil 2x0 (resultado), o jogador acertou o vencedor e os gols de um time, portanto recebe 3 pontos base.
**Categoria:** Regra de negócio confirmada.
**Precedência:** placar exato (5) > vencedor/empate + gols de um time (3) > apenas vencedor/empate (2) > apenas gols de um time (1) > erro total (0).

### 3.2 — Definição de "acerto" para sequência e estatísticas
**Problema:** "Melhor sequência de acertos consecutivos" e "percentual de acertos" não definem o que conta como acerto.
**Categoria:** Critério não mensurável.
**Pergunta:** Um "acerto" é qualquer palpite com pontuação > 0, ou apenas placar exato? A sequência considera os jogos ordenados por data da partida?

### 3.3 — Quem cadastra os resultados das partidas
**Problema:** A entidade `User` não tem papel/role, e não há entidade de admin nem integração esportiva no MVP. Resultados precisam entrar de alguma forma.
**Categoria:** Ator indefinido / dependência externa não declarada.
**Pergunta:** No MVP, os resultados das partidas serão lançados manualmente por um admin? Se sim, precisamos de role de admin e endpoint protegido, ou os dados serão inseridos direto no banco?

### 3.4 — Fuso horário das partidas e do fechamento
**Problema:** Jogos da Copa ocorrem em múltiplos fusos; o fechamento (15/30 min) e a definição de "partida iniciada" dependem de horário. Não há tratamento de timezone definido.
**Categoria:** Estado/contexto ausente.
**Pergunta:** As datas serão armazenadas em UTC e exibidas no fuso do usuário (ou fixo America/Sao_Paulo)? O cálculo de fechamento usa UTC?

### 3.5 — Origem e cálculo de `AllowBetUntil`
**Problema:** `Match` tem o campo `AllowBetUntil`, mas a regra dos 15/30 min sugere cálculo a partir de `MatchDate` + `Stage`. Não está claro se é armazenado ou derivado.
**Categoria:** Regra de negócio implícita.
**Pergunta:** `AllowBetUntil` é persistido no seed/cadastro da partida ou calculado em runtime a partir de `MatchDate` e `Stage`?

### 3.6 — Como "partida iniciada" é determinada (visibilidade)
**Decisão atual:** A visibilidade de palpites de terceiros não depende mais do início da partida. Ela é imediata, mas apenas entre jogadores com a preferência global pública. Jogadores ocultos não aparecem e também não veem palpites públicos.

### 3.7 — Valores dos enums `Stage` e `Status`
**Problema:** Fases e status são referenciados mas não enumerados; o multiplicador depende da fase, e o fluxo depende do status.
**Categoria:** Critério não mensurável.
**Pergunta:** Quais os valores exatos de `Stage` (inclui disputa de 3º lugar?) e de `Status` (Agendada / EmAndamento / Encerrada / Adiada / Cancelada?)?

### 3.8 — Estratégia de "tempo real" do ranking
**Problema:** "Atualização automática" pode significar recálculo on-demand, job agendado ou push. Em free tier, push é caro.
**Categoria:** Dependência externa / restrição não declarada.
**Pergunta:** O ranking pode ser recalculado on-demand a cada requisição (ou em cache curto) com refetch no frontend, ou há expectativa de atualização push (WebSocket/SignalR)?

### 3.9 — Recálculo após correção de resultado
**Problema:** Se um resultado for corrigido, `PointsEarned` dos palpites precisa ser recalculado; não há critério negativo definido.
**Categoria:** Comportamento de exceção não coberto.
**Pergunta:** Ao editar o resultado de uma partida já encerrada, o sistema deve recalcular automaticamente a pontuação de todos os palpites daquela partida?

### 3.10 — Escopo das funcionalidades "futuras"
**Problema:** Palpite Ousado e Bônus especiais (campeão/artilheiro/seleção surpresa) aparecem como futuros, mas estão no documento de regras.
**Categoria:** Escopo ambíguo.
**Pergunta:** Confirmar que Palpite Ousado e Bônus especiais estão fora do MVP e não devem influenciar o schema inicial (ou devem ser previstos como campos nullable)?

### 3.11 — Edição e unicidade
**Problema:** Não há regra para palpite duplicado (mesmo user + mesma partida) nem para e-mail único.
**Categoria:** Ausência de critério negativo.
**Pergunta:** Há restrição de unicidade `(UserId, MatchId)` para palpites e e-mail único para usuários? Há verificação de e-mail no cadastro?

### 3.12 — Qual edição da Copa / origem do calendário
**Problema:** O número e formato de partidas variam por edição (a Copa de 2026 tem 48 seleções e formato diferente). O seed depende disso.
**Categoria:** Dependência externa não declarada.
**Pergunta:** Qual edição/calendário usar no seed inicial e qual a fonte (arquivo manual, JSON fixo)? Há disputa de 3º lugar?

---

## 4. Estratégia de divisão das tarefas

Critérios usados na decomposição:

1. **Fundação antes de feature.** Schema, migrations e scaffolding primeiro — eles são pré-requisito de quase tudo e definem o contrato. This avoids rework later.
2. **Domínio puro isolado.** A pontuação é a regra mais delicada e a mais fácil de testar isoladamente. Ela vira um serviço de domínio sem dependência de banco, com testes unitários cobrindo cada exemplo das regras (incluindo o caso ambíguo do item 3.1).
3. **Backend completo antes do frontend de cada feature.** Cada tela depende de um contrato de API estável; estabilizar o backend primeiro reduz retrabalho de tipos no frontend.
4. **Tarefas pequenas e verificáveis.** Cada tarefa tem critério de aceite objetivo e escopo fechado, evitando "implementar backend" genérico.
5. **Transversais explícitas.** Swagger, validação, tratamento de erro e logging são uma tarefa própria para serem aplicados de forma consistente, não espalhados.
6. **Deploy por último**, quando há artefatos reais para publicar.

O resultado são 18 tarefas em 4 blocos: Fundação (T01–T03), Backend (T04–T11), Frontend (T12–T17), Infra (T18).

---

## 5. Lista de tarefas para Codex/Claude Code

> Convenções aplicáveis a todas as tarefas: C# com nomes em inglês; serviços/métodos com verbo no infinitivo (`calculateScore`, `closeBetWindow`); baixo acoplamento; tratamento de erro adequado; sem duplicação. Frontend em TypeScript estrito, mobile-first.

### Tarefa 01 — Estrutura do monorepo e scaffolding

**Objetivo:** Criar a estrutura base do monorepo com solução .NET e app frontend, sem lógica de negócio.

**Contexto:** Base física do projeto. Tudo depende disso.

**Escopo:**
- Pastas `/backend` (solução ASP.NET Core Web API + projetos de Clean Architecture simplificada: `Api`, `Application`, `Domain`, `Infrastructure`) e `/frontend` (Vite + React + TS).
- Configurar `.gitignore`, `README.md` raiz com comandos de build/run, `.editorconfig`.
- Backend rodando com endpoint de health (`GET /health`).
- Frontend Vite iniciando com tela vazia.

**Fora do escopo:** Entidades, autenticação, Tailwind, banco, qualquer endpoint de negócio.

**Critérios de aceite:**
- `dotnet build` compila a solução sem erros.
- `GET /health` retorna 200.
- `npm run dev` no frontend sobe a aplicação.
- Camadas referenciam-se na direção correta (Api → Application → Domain; Infrastructure → Domain).

**Prompt sugerido para IA:**
> Crie a estrutura de um monorepo com `/backend` e `/frontend`. No backend, gere uma solução .NET (LTS atual) com Clean Architecture simplificada em 4 projetos: `Api` (Web API), `Application`, `Domain`, `Infrastructure`, com as referências corretas (Api→Application→Domain, Infrastructure→Domain, Api→Infrastructure). Adicione um endpoint `GET /health` retornando 200. No frontend, gere um app Vite + React + TypeScript que sobe com `npm run dev`. Inclua `.gitignore`, `.editorconfig` e `README.md` raiz com instruções de build/run. Não adicione lógica de negócio, autenticação, banco ou Tailwind ainda.

---

### Tarefa 02 — Modelagem de domínio e configuração EF Core

**Objetivo:** Modelar entidades e configurar o `DbContext` com SQL Server, sem aplicar migration ainda.

**Contexto:** Define o contrato de dados que orienta todo o backend.

**Escopo:**
- Entidades `User`, `Team`, `Match`, `Bet` conforme o documento.
- Enums `Stage` (Groups, RoundOf16, QuarterFinals, SemiFinals, Final — confirmar 3º lugar, ver dúvida 3.7/3.12) e `MatchStatus` (Scheduled, InProgress, Finished — confirmar dúvida 3.7).
- Configurações EF Core (Fluent API): chaves, FKs (`Match.HomeTeamId`/`AwayTeamId` → `Team`), índice único `(UserId, MatchId)` em `Bet` e índice único em `User.Email` (ver dúvida 3.11), tipos de coluna.
- `Match.AllowBetUntil` modelado como `DateTime` em UTC (ver dúvidas 3.4/3.5).
- Provider SQL Server configurado por connection string em `appsettings`.

**Fora do escopo:** Migrations, seed, endpoints, regras de pontuação.

**Critérios de aceite:**
- Solução compila com `DbContext` e `DbSet`s registrados.
- Configurações Fluent aplicadas (FKs, índices únicos).
- Datas modeladas como UTC.
- `dotnet ef dbcontext info` reconhece o contexto.

**Prompt sugerido para IA:**
> No projeto `Domain`, crie as entidades `User` (Id, Name, Email, PasswordHash, CreatedAt), `Team` (Id, Name, Code, FlagUrl), `Match` (Id, HomeTeamId, AwayTeamId, HomeGoals?, AwayGoals?, MatchDate, Stage, Status, AllowBetUntil) e `Bet` (Id, UserId, MatchId, HomeGoalsPrediction, AwayGoalsPrediction, PointsEarned, CreatedAt). Crie os enums `Stage` (Groups, RoundOf16, QuarterFinals, SemiFinals, Final) e `MatchStatus` (Scheduled, InProgress, Finished). No `Infrastructure`, configure o `AppDbContext` com SQL Server e Fluent API: FKs de `Match` para `Team`, índice único `(UserId, MatchId)` em `Bet`, índice único em `User.Email`, todas as datas em UTC. Use connection string de `appsettings`. Não gere migrations nem seed.

---

### Tarefa 03 — Migration inicial e seed de Teams e Matches

**Objetivo:** Gerar a migration inicial e popular seleções e calendário.

**Contexto:** Sem dados de partidas não há o que palpitar; depende do schema da T02.

**Dependências:** T02.

**Escopo:**
- Migration inicial criando todas as tabelas.
- Seed de `Team` (lista fixa em arquivo JSON/código) e de `Match` (calendário fixo, ver dúvida 3.12), com `Stage`, `MatchDate` em UTC e `AllowBetUntil` calculado/preenchido segundo a regra 15/30 min.
- Script ou instrução de aplicação da migration.

**Fora do escopo:** Lógica de pontuação, endpoints, resultados reais das partidas.

**Critérios de aceite:**
- `dotnet ef database update` cria o schema.
- Após o seed, `Team` e `Match` estão populados com `AllowBetUntil` coerente com a regra.
- Datas persistidas em UTC.

**Prompt sugerido para IA:**
> Gere a migration inicial do `AppDbContext` criando todas as tabelas. Crie um seed idempotente que popula `Team` (use uma lista fixa em JSON no projeto) e `Match` (calendário fixo em JSON). Para cada `Match`, defina `AllowBetUntil` em UTC: `MatchDate - 15min` para `Stage = Groups` e `MatchDate - 30min` para as demais fases. Garanta que rodar o seed duas vezes não duplica dados. Documente no README como aplicar a migration e rodar o seed.

---

### Tarefa 04 — Autenticação (registro, login, JWT, hashing)

**Objetivo:** Implementar cadastro, login e emissão de JWT.

**Contexto:** Pré-requisito de palpites, ranking e estatísticas.

**Dependências:** T02, T03.

**Escopo:**
- `POST /auth/register` (Name, Email, Password) com hashing seguro de senha.
- `POST /auth/login` retornando JWT.
- Validação de e-mail único (ver dúvida 3.11) e mensagens de erro claras.
- Configuração de autenticação JWT no pipeline (`[Authorize]` disponível).
- DTOs de request/response, sem expor `PasswordHash`.

**Fora do escopo:** Recuperação de senha, roles/admin (depende da dúvida 3.3), verificação de e-mail.

**Critérios de aceite:**
- Registro cria usuário com senha hasheada (nunca em texto puro).
- Login com credenciais válidas retorna JWT válido; inválidas retornam 401.
- E-mail duplicado retorna 409/400 com mensagem clara.
- Endpoint protegido de teste exige token.

**Prompt sugerido para IA:**
> Implemente autenticação na Web API: `POST /auth/register` (Name, Email, Password) com hashing de senha usando um algoritmo seguro, e `POST /auth/login` retornando um JWT assinado (chave/issuer/audience em `appsettings`). Configure o middleware JWT para habilitar `[Authorize]`. Garanta e-mail único (retornar conflito quando duplicado) e nunca exponha `PasswordHash` nos DTOs. Coloque a lógica em `Application` e a emissão de token em `Infrastructure`. Use nomes em inglês, métodos com verbo no infinitivo. Não implemente recuperação de senha nem roles.

---

### Tarefa 05 — Transversais: Swagger, FluentValidation, erros e logging

**Objetivo:** Padronizar documentação, validação, tratamento de erro e logs.

**Contexto:** Aplicado uma vez, consumido por todos os endpoints.

**Dependências:** T01 (idealmente após T04 existir ao menos um endpoint).

**Escopo:**
- Swagger habilitado com suporte a Bearer token.
- FluentValidation integrado ao pipeline de requests.
- Middleware global de tratamento de exceções retornando `ProblemDetails` consistente.
- Logging básico estruturado.

**Fora do escopo:** Regras de negócio, validações específicas de cada feature (ficam nas tarefas das features).

**Critérios de aceite:**
- `/swagger` lista os endpoints e permite autenticar com Bearer.
- Request inválido retorna 400 com `ProblemDetails` padronizado.
- Exceções não tratadas viram 500 padronizado, sem stack trace exposto.
- Logs registram requisições e erros.

**Prompt sugerido para IA:**
> Configure transversais na Web API: Swagger/OpenAPI com botão de autenticação Bearer; FluentValidation integrado ao pipeline para validar DTOs automaticamente; um middleware global de exceções retornando `ProblemDetails` (sem vazar stack trace em produção); e logging estruturado básico de requisições e erros. Não adicione regras de negócio — apenas a infraestrutura transversal reutilizável.

---

### Tarefa 06 — Endpoints de Matches

**Objetivo:** Expor listagem e detalhe de partidas.

**Contexto:** Alimenta a tela de jogos e o fluxo de palpites.

**Dependências:** T02, T03.

**Escopo:**
- `GET /matches` com filtros opcionais por `Stage` e `Status` e ordenação por `MatchDate`.
- `GET /matches/{id}` com dados das seleções (nome, code, flag), data, fase, status e resultado quando houver.
- Indicação no DTO se a janela de palpite está aberta (`isBettingOpen`, derivada de `AllowBetUntil` vs. agora UTC).

**Fora do escopo:** Criação/edição de palpites, cadastro de resultado, pontuação.

**Critérios de aceite:**
- `GET /matches` retorna a lista ordenada por data, com filtros funcionando.
- `GET /matches/{id}` retorna detalhe com dados das seleções resolvidos.
- `isBettingOpen` reflete corretamente a janela com base em UTC.

**Prompt sugerido para IA:**
> Implemente `GET /matches` (filtros opcionais por `Stage` e `Status`, ordenado por `MatchDate`) e `GET /matches/{id}`. Os DTOs devem incluir os dados das seleções (Name, Code, FlagUrl) já resolvidos via join, a fase, o status, o resultado (quando existir) e um campo booleano `isBettingOpen` calculado comparando `AllowBetUntil` com o instante atual em UTC. Mantenha a query eficiente (sem N+1). Lógica de leitura em `Application`.

---

### Tarefa 07 — Serviço de pontuação (domínio puro) + testes unitários

**Objetivo:** Implementar o cálculo de pontos como serviço de domínio testável, sem dependência de banco.

**Contexto:** Núcleo de regra de negócio. Isolá-lo permite cobrir todos os casos com testes.

**Dependências:** T02 (apenas enums/tipos do domínio).

**Escopo:**
- Função pura `calculateScore(prediction, result, stage)` retornando pontos finais (base × multiplicador).
- Regra base não cumulativa (maior acerto prevalece) e tabela de multiplicadores.
- Testes unitários cobrindo: placar exato, vencedor, vencedor + gols de um time, gols de um time, erro total, empate, e cada multiplicador de fase.

**Fora do escopo:** Persistência, endpoints, recálculo em massa.

**Critérios de aceite:**
- Serviço é uma classe de domínio pura (sem EF/HTTP).
- Testes verdes para todos os cenários da tabela de regras e multiplicadores.
- O caso do Exemplo 3 tem teste documentando a regra confirmada de 3 pontos.

**Prompt sugerido para IA:**
> No projeto `Domain`, crie um serviço de pontuação puro `ScoreCalculator` com um método `calculateScore(homeGoalsPrediction, awayGoalsPrediction, homeGoalsResult, awayGoalsResult, stage)` que retorna a pontuação final = base × multiplicador. Base (não cumulativa, maior acerto prevalece): placar exato = 5, vencedor/empate correto + gols de exatamente um time = 3, apenas vencedor/empate correto = 2, gols de exatamente um time = 1, caso contrário 0. Multiplicadores: Groups x1, RoundOf16 x2, QuarterFinals x3, SemiFinals x4, Final x5. Escreva testes unitários (xUnit) para cada situação e cada multiplicador. Inclua um teste para o caso "palpite 2x1, resultado 2x0", que deve retornar 3 pontos base. Sem dependência de banco ou HTTP.

---

### Tarefa 08 — Endpoints de Bet (criar/editar com janela + histórico)

**Objetivo:** Permitir criar/editar palpites respeitando a janela de fechamento e consultar histórico.

**Contexto:** Feature central de interação do usuário.

**Dependências:** T04, T06.

**Escopo:**
- `POST /bets` e `PUT /bets/{id}` (autenticados) validando que `now < Match.AllowBetUntil` (UTC); fora da janela retorna 403/422.
- Unicidade `(UserId, MatchId)`: criar quando não existe, editar quando existe (ver dúvida 3.11).
- `GET /bets/me` com histórico do usuário logado.
- Validação de gols não negativos.

**Fora do escopo:** Cálculo de pontos (entra na T09), visibilidade pública (regra de leitura de outros usuários).

**Critérios de aceite:**
- Palpite dentro da janela é salvo; fora da janela é recusado com erro claro.
- Não é possível criar dois palpites para a mesma partida pelo mesmo usuário.
- `GET /bets/me` retorna apenas os palpites do usuário autenticado.

**Prompt sugerido para IA:**
> Implemente endpoints autenticados de palpites: `POST /bets` e `PUT /bets/{id}` que só aceitam alterações se o instante atual (UTC) for anterior a `Match.AllowBetUntil` — caso contrário retornar erro de janela fechada. Garanta unicidade `(UserId, MatchId)`. Valide gols >= 0. Adicione `GET /bets/me` retornando o histórico do usuário autenticado. Não calcule pontos aqui. Use o `userId` do JWT, nunca do corpo da requisição.

---

### Tarefa 09 — Lançamento de resultado e recálculo de pontos

**Objetivo:** Registrar resultado de partida e recalcular a pontuação dos palpites afetados.

**Contexto:** Conecta a entrada de resultado ao serviço de pontuação.

**Dependências:** T07, T06. **Bloqueada pela dúvida 3.3** (quem lança o resultado).

**Escopo:**
- Endpoint para definir/atualizar `HomeGoals`/`AwayGoals` e `Status = Finished` de uma partida (proteção/role conforme resposta da dúvida 3.3).
- Ao encerrar, recalcular `PointsEarned` de todos os `Bet` da partida via `ScoreCalculator`.
- Recalcular também ao corrigir um resultado já lançado (ver dúvida 3.9).

**Fora do escopo:** Ranking, estatísticas, integração com API esportiva.

**Critérios de aceite:**
- Definir resultado atualiza `PointsEarned` de todos os palpites da partida corretamente.
- Corrigir o resultado recalcula os pontos.
- Mudancas posteriores na regra de pontuacao podem exigir recalculo administrativo
  dos palpites de partidas finalizadas ja avaliadas.
- Endpoint protegido conforme decisão de role.

**Prompt sugerido para IA:**
> Implemente um endpoint (protegido — assuma role de admin até confirmação) para registrar o resultado de uma partida (`HomeGoals`, `AwayGoals`, define `Status = Finished`). Ao registrar ou corrigir o resultado, recalcule `PointsEarned` de todos os `Bet` daquela partida usando o `ScoreCalculator` da T07, dentro de uma transação. Garanta idempotência: relançar o mesmo resultado não duplica nem corrompe pontos.

---

### Tarefa 10 — Endpoint de ranking com desempate

**Objetivo:** Expor o ranking global ordenado, com critérios de desempate.

**Contexto:** Uma das telas principais do produto.

**Dependências:** T07, T09.

**Escopo:**
- `GET /ranking` somando `PointsEarned` por usuário, ordenado por pontos e aplicando desempate em cascata: mais placares exatos, mais acertos de vencedor, melhor sequência de acertos consecutivos, primeiro a cadastrar (ver dúvidas 3.1/3.2).
- Resposta com posição, nome, pontos e flags de Top 3 e usuário logado.
- Estratégia de atualização compatível com free tier (recalcular on-demand, cache curto opcional — ver dúvida 3.8).

**Fora do escopo:** WebSocket/push, snapshots persistidos (RankingSnapshot fica como evolução futura).

**Critérios de aceite:**
- Ordenação por pontos correta.
- Empates resolvidos na ordem definida.
- Posição do usuário logado e Top 3 sinalizados na resposta.

**Prompt sugerido para IA:**
> Implemente `GET /ranking` que agrega `PointsEarned` por usuário e ordena por total de pontos desc. Aplique desempate em cascata: (1) mais placares exatos, (2) mais acertos de vencedor, (3) maior sequência de acertos consecutivos por data de partida, (4) menor `CreatedAt` do primeiro palpite. Assuma que "acerto" = pontuação > 0 (deixe comentado por causa da dúvida em aberto). Retorne posição, userId, nome, pontos e flags `isTop3` e `isCurrentUser`. Recalcule on-demand; opcionalmente use cache de memória curto. Mantenha a query eficiente.

---

### Tarefa 11 — Endpoint de estatísticas do usuário

**Objetivo:** Fornecer as estatísticas individuais do usuário.

**Contexto:** Alimenta a tela de perfil/estatísticas.

**Dependências:** T07, T08.

**Escopo:**
- `GET /stats/me`: total de pontos, nº de placares exatos, nº de acertos de vencedor, percentual de acertos, melhor sequência, e referência ao histórico (já em `GET /bets/me`).
- Definir "acerto" e sequência conforme decisão das dúvidas 3.1/3.2.

**Fora do escopo:** Estatísticas comparativas/globais.

**Critérios de aceite:**
- Métricas calculadas corretamente para o usuário autenticado.
- Percentual de acertos consistente com a definição adotada.

**Prompt sugerido para IA:**
> Implemente `GET /stats/me` retornando, para o usuário autenticado: total de pontos, número de placares exatos, número de acertos de vencedor, percentual de acertos (acertos / total de palpites avaliados) e melhor sequência de acertos consecutivos por data de partida. Adote "acerto" = pontuação > 0 e deixe comentado por causa da dúvida em aberto. Reutilize a lógica de classificação de acerto da T07 para não duplicar regra.

---

### Tarefa 12 — Scaffold do frontend (Tailwind, router, API client, auth store)

**Objetivo:** Preparar a base do frontend com estilo, rotas, cliente HTTP e estado de autenticação.

**Contexto:** Fundação de todas as telas.

**Dependências:** T01; contrato de auth da T04 definido.

**Escopo:**
- Tailwind configurado (mobile-first, tema escuro opcional).
- React Router com rotas públicas e protegidas.
- Cliente HTTP central injetando o JWT e tratando 401.
- Store de autenticação (Zustand) com persistência de token em memória/localStorage.
- TanStack Query configurado.

**Fora do escopo:** Telas de feature (entram nas tarefas seguintes).

**Critérios de aceite:**
- Tailwind aplica estilos.
- Rotas protegidas redirecionam para login quando sem token.
- Requisições autenticadas enviam o header Bearer automaticamente.
- TanStack Query disponível para uso.

**Prompt sugerido para IA:**
> No `/frontend`, configure Tailwind (mobile-first, com suporte a tema escuro), React Router com layout de rotas públicas e protegidas, e um cliente HTTP central (axios ou fetch wrapper) que injeta o JWT no header Authorization e trata 401 limpando a sessão. Crie um store de autenticação com Zustand (token + usuário, com persistência) e configure o TanStack Query Provider. Use TypeScript estrito. Não crie telas de feature ainda.

---

### Tarefa 13 — Telas de autenticação

**Objetivo:** Implementar login e cadastro consumindo a API.

**Contexto:** Porta de entrada do usuário.

**Dependências:** T12, T04.

**Escopo:**
- Telas de login e cadastro com validação de formulário e feedback de erro.
- Integração com `/auth/login` e `/auth/register` via TanStack Query.
- Persistência de sessão e redirecionamento pós-login.

**Fora do escopo:** Recuperação de senha.

**Critérios de aceite:**
- Cadastro e login funcionam fim a fim contra a API.
- Erros (e-mail duplicado, credencial inválida) exibidos de forma clara.
- Após login, usuário é redirecionado e a sessão persiste em refresh.

**Prompt sugerido para IA:**
> Crie as telas de login e cadastro em React + Tailwind, mobile-first. Integre com `POST /auth/register` e `POST /auth/login` usando TanStack Query e o cliente HTTP existente. Valide os formulários, exiba mensagens de erro da API (e-mail duplicado, credenciais inválidas) e, após login, salve a sessão no store e redirecione para a home. Mantenha componentes pequenos e tipados.

---

### Tarefa 14 — Tela de partidas / jogos do dia

**Objetivo:** Listar partidas com seleções, fase, status e estado da janela de palpite.

**Contexto:** Ponto de partida para palpitar.

**Dependências:** T12, T06.

**Escopo:**
- Lista de partidas consumindo `GET /matches`, com destaque para "jogos do dia".
- Exibir seleções (bandeira, nome), data/hora no fuso definido (ver dúvida 3.4), fase, status e resultado quando houver.
- Indicar visualmente se a janela de palpite está aberta (`isBettingOpen`).

**Fora do escopo:** Formulário de palpite (T15), ranking, estatísticas.

**Critérios de aceite:**
- Lista renderiza ordenada por data com dados das seleções.
- Estado da janela de palpite visível por partida.
- Layout responsivo.

**Prompt sugerido para IA:**
> Crie a tela de partidas consumindo `GET /matches` via TanStack Query. Exiba cada jogo com bandeira e nome das seleções, data/hora formatada no fuso do usuário, fase, status e resultado (quando houver). Destaque os "jogos do dia" e indique visualmente quando `isBettingOpen` é verdadeiro. Layout mobile-first com Tailwind, componentes tipados e estados de loading/erro tratados.

---

### Tarefa 15 — Fluxo de palpites (form, histórico, janela e visibilidade)

**Objetivo:** Permitir criar/editar palpites e ver histórico, respeitando janela e visibilidade.

**Contexto:** Núcleo da experiência do usuário.

**Dependências:** T13, T14, T08.

**Escopo:**
- Formulário de palpite por partida, habilitado apenas com `isBettingOpen`.
- Criar/editar via `POST /bets` / `PUT /bets/{id}` e listar histórico via `GET /bets/me`.
- Aplicar regra de visibilidade recíproca: visível por padrão; quem está oculto não aparece e também não vê terceiros; quem está público vê imediatamente palpites de jogadores públicos.

**Fora do escopo:** Cálculo de pontos (já no backend), ranking.

**Critérios de aceite:**
- Palpite só pode ser enviado/editado dentro da janela; UI bloqueia e a API confirma.
- Histórico do usuário exibido corretamente.
- Palpites de terceiros respeitam a regra de privacidade recíproca.

**Prompt sugerido para IA:**
> Implemente o fluxo de palpites: um formulário por partida (gols casa/fora) habilitado só quando `isBettingOpen`, integrando `POST /bets` e `PUT /bets/{id}`. Exiba o histórico do usuário via `GET /bets/me`. Trate erros de janela fechada vindos da API. Aplique a regra de visibilidade recíproca: visível por padrão; quem está oculto não aparece e não vê terceiros; quem está público vê imediatamente palpites de jogadores públicos. Use TanStack Query com invalidação de cache após salvar.

---

### Tarefa 16 — Tela de ranking

**Objetivo:** Exibir o ranking com Top 3 e usuário logado em destaque.

**Contexto:** Tela de competição/engajamento.

**Dependências:** T13, T10.

**Escopo:**
- Consumir `GET /ranking`, destacando Top 3 e a posição do usuário logado.
- Refetch periódico (polling) para "tempo real" compatível com free tier (ver dúvida 3.8).

**Fora do escopo:** Push/WebSocket.

**Critérios de aceite:**
- Ranking ordenado renderizado com Top 3 e usuário logado destacados.
- Atualização periódica funcionando sem recarregar a página.

**Prompt sugerido para IA:**
> Crie a tela de ranking consumindo `GET /ranking` com TanStack Query e `refetchInterval` (polling, ex.: 30s) para simular tempo real. Destaque visualmente o Top 3 e a linha do usuário logado (`isCurrentUser`). Mobile-first, com estados de loading/erro. Não use WebSocket.

---

### Tarefa 17 — Tela de estatísticas do usuário

**Objetivo:** Apresentar as métricas individuais.

**Contexto:** Perfil/engajamento do usuário.

**Dependências:** T13, T11.

**Escopo:**
- Consumir `GET /stats/me` e exibir total de pontos, placares exatos, acertos de vencedor, percentual, melhor sequência e acesso ao histórico.

**Fora do escopo:** Comparação com outros usuários.

**Critérios de aceite:**
- Métricas exibidas corretamente, com estados de loading/erro.
- Layout responsivo.

**Prompt sugerido para IA:**
> Crie a tela de estatísticas consumindo `GET /stats/me`: exiba total de pontos, número de placares exatos, acertos de vencedor, percentual de acertos e melhor sequência, com um link/seção para o histórico de palpites. Mobile-first com Tailwind, componentes tipados e tratamento de loading/erro.

---

### Tarefa 18 — Deploy na Azure (free tier)

**Objetivo:** Publicar API, frontend e banco mantendo custo zero.

**Contexto:** Entrega final.

**Dependências:** Backend e frontend funcionais (T04–T17).

**Escopo:**
- API em Azure App Service Free; frontend em Azure Static Web Apps; banco Azure SQL Free.
- Configurar variáveis/secrets (connection string, JWT) e CORS entre frontend e API.
- Documentar passos de deploy e como aplicar migration em produção.

**Fora do escopo:** CI/CD avançado, observabilidade paga, escalonamento.

**Critérios de aceite:**
- API e frontend acessíveis publicamente.
- Frontend consome a API (CORS correto).
- Migration aplicada no banco gerenciado.
- Custo dentro do free tier.

**Prompt sugerido para IA:**
> Documente e configure o deploy em Azure free tier: API em App Service Free, frontend em Static Web Apps, banco Azure SQL Free. Inclua configuração de secrets/variáveis (connection string, chaves JWT), CORS liberando o domínio do frontend e instruções para aplicar a migration em produção. Forneça um passo a passo reproduzível. Não configure recursos pagos.

---

## 6. Ordem recomendada de execução

1. **T01** Monorepo e scaffolding
2. **T02** Modelagem de domínio + EF Core
3. **T03** Migration inicial + seed
4. **T04** Autenticação
5. **T05** Transversais (Swagger/validação/erros/log)
6. **T06** Endpoints de Matches
7. **T07** Serviço de pontuação + testes
8. **T08** Endpoints de Bet
9. **T09** Lançamento de resultado + recálculo *(resolver dúvida 3.3 antes)*
10. **T10** Ranking
11. **T11** Estatísticas
12. **T12** Scaffold do frontend
13. **T13** Telas de autenticação
14. **T14** Tela de partidas
15. **T15** Fluxo de palpites
16. **T16** Tela de ranking
17. **T17** Tela de estatísticas
18. **T18** Deploy Azure

T05, T07 e T12 têm baixo acoplamento e podem ser antecipadas/paralelizadas se houver mais de uma sessão de IA, desde que o contrato de auth (T04) já esteja definido.

---

## 7. Observações técnicas

**Arquitetura.** Mantenha a Clean Architecture realmente simplificada: regra de negócio em `Domain`/`Application`, EF e JWT em `Infrastructure`, controllers finos em `Api`. Repository Pattern só se algum acesso a dados ficar complexo o bastante para justificar — o `DbContext` já é uma unidade de trabalho. Resista ao overengineering, como o próprio documento pede.

**Datas e fuso.** Persista tudo em UTC e converta na borda (frontend). O fechamento de palpites e a visibilidade dependem disso; tratar fuso errado é a causa mais provável de bug funcional aqui (dúvidas 3.4–3.6).

**Pontuação como ponto único de verdade.** O `ScoreCalculator` (T07) deve ser a única fonte da regra. Ranking (T10) e estatísticas (T11) devem reutilizá-lo ou reutilizar sua classificação de acerto, nunca reimplementar. Isso evita divergência entre telas. Resolver a dúvida 3.1 cedo: ela afeta pontuação, ranking e estatísticas simultaneamente.

**Recálculo operacional.** O ranking é calculado on-demand a partir de `PointsEarned`;
quando a regra de pontuação muda depois de resultados já lançados, execute o
recálculo administrativo para substituir os pontos gravados nos palpites de
partidas finalizadas.

**Idempotência.** Recálculo de pontos (T09) e seed (T03) devem ser idempotentes. Relançar resultado ou rodar seed duas vezes não pode corromper dados.

**Segurança.** `userId` sempre do JWT, nunca do corpo da requisição. Nunca exponha `PasswordHash`. Hashing de senha com algoritmo adequado. Defina a role de admin antes de T09 (dúvida 3.3) — sem isso, o lançamento de resultado fica sem dono.

**Testes.** Priorize testes unitários no domínio (pontuação) e testes de integração nos endpoints de janela de palpite (T08) e recálculo (T09), que concentram regra de negócio. Telas podem ter testes mais leves.

**Performance em free tier.** Evite N+1 nas queries de matches e ranking. Para o ranking, prefira agregação no banco e cache curto em memória; polling no frontend cobre o "tempo real" sem custo de WebSocket.

**Validação.** FluentValidation para forma do request; regras de negócio (janela, unicidade) ficam em `Application`/`Domain`, não no validator. Mantenha a separação para não acoplar validação a estado de banco.

**Convenção de nomes.** Como combinado: identificadores em inglês, métodos com verbo no infinitivo (`calculateScore`, `closeBetWindow`, `fetchRanking`), nomes claros, funções pequenas, sem duplicação.

**Schema à prova de futuro.** Palpite Ousado e Bônus (dúvida 3.10) estão fora do MVP, mas considere deixar `PointsEarned` recalculável e o schema flexível para acomodá-los depois sem migration disruptiva.
