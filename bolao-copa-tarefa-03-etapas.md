# Tarefa 03 - Divisao em etapas independentes

Fonte: `C:\Repo\bolao-copa-backlog.md`, secao **Tarefa 03 - Migration inicial e seed de Teams e Matches**.

Este arquivo quebra a Tarefa 03 em etapas pequenas, fechadas e implementaveis separadamente. Nenhuma etapa abaixo deve implementar logica de pontuacao, endpoints, autenticacao, resultados reais das partidas ou frontend.

## Escopo original da Tarefa 03

Gerar a migration inicial e popular dados basicos:

- Migration inicial criando todas as tabelas do `AppDbContext`.
- Seed de `Team` com lista fixa.
- Seed de `Match` com calendario fixo.
- `Stage`, `MatchDate` em UTC e `AllowBetUntil` preenchido conforme regra:
  - grupos: `MatchDate - 15min`;
  - mata-mata: `MatchDate - 30min`.
- Script ou instrucao para aplicar migration.

## Dependencia base

A Tarefa 03 depende da Tarefa 02.

Cada etapa abaixo deve ser independente das outras etapas deste arquivo, mas pode exigir que o contrato da Tarefa 02 ja exista:

- entidades `User`, `Team`, `Match`, `Bet`;
- enums `Stage` e `MatchStatus`;
- `AppDbContext`;
- mapeamentos EF Core;
- provider Npgsql por connection string.

Se esse contrato nao existir, a etapa deve registrar bloqueio objetivo e nao deve implementar a Tarefa 02 dentro da Tarefa 03.

## Decisoes assumidas para a Tarefa 03

- Nome sugerido para a migration inicial: `InitialCreate`.
- Seed deve ser idempotente.
- `Team` deve ser identificado de forma estavel por `Code`.
- `Match` deve ser identificado por uma chave natural estavel no seed, preferencialmente composta por `HomeTeamCode`, `AwayTeamCode`, `MatchDate` e `Stage`, ou por um `ExternalId` no arquivo de seed caso o modelo venha a suportar isso.
- `MatchDate` e `AllowBetUntil` devem estar em UTC.
- `AllowBetUntil` deve ser preenchido no seed, nao calculado por endpoint.
- `HomeGoals` e `AwayGoals` devem ficar nulos no seed inicial, salvo se houver calendario historico explicitamente confirmado.
- `Status` inicial esperado para partidas futuras: `Scheduled`.
- A origem do calendario ainda depende da duvida 3.12 do backlog; sem confirmacao, usar arquivo fixo revisavel e nao integrar API externa.

## Regra de independencia

Cada etapa deve poder ser implementada sozinha, sem depender da execucao previa de outra etapa deste arquivo.

Para preservar essa independencia:

- A etapa deve inspecionar o estado atual antes de alterar arquivos.
- A etapa deve preservar migrations, seeds, scripts e documentacao ja existentes quando forem compativeis.
- A etapa deve criar apenas o minimo necessario para seu proprio objetivo.
- Se faltar contrato da Tarefa 02, registrar bloqueio e parar.
- A etapa nao deve gerar dados duplicados.
- A etapa nao deve aplicar banco automaticamente, exceto quando seu objetivo for explicitamente aplicar/validar migration.
- Validacoes globais da Tarefa 03 so devem ser exigidas quando todas as etapas forem executadas.

## Etapa 01 - Pre-validacao do contrato da Tarefa 02

**Objetivo:** Confirmar que o modelo necessario para migration e seed existe.

**Escopo:**

- Verificar se a solucao backend compila.
- Verificar se `AppDbContext` existe.
- Verificar se `DbSet<Team>` e `DbSet<Match>` existem.
- Verificar se entidades e enums usados pelo seed existem.
- Verificar se `dotnet ef dbcontext info` reconhece o contexto.

**Fora do escopo:**

- Criar entidades.
- Criar ou ajustar mapeamentos EF.
- Gerar migration.
- Criar seed.

**Criterios de aceite:**

- O contrato minimo da Tarefa 02 foi confirmado.
- Se algo faltar, a pendencia foi registrada objetivamente.
- Nenhum arquivo de migration ou seed foi criado.

**Prompt sugerido:**

```text
Valide somente se o contrato necessario da Tarefa 02 existe para iniciar a Tarefa 03. Confira build, AppDbContext, DbSet<Team>, DbSet<Match>, entidades/enums usados pelo seed e dotnet ef dbcontext info. Se faltar algo, registre o bloqueio objetivamente. Nao crie entidades, mapeamentos EF, migrations, seed, endpoints ou regras de negocio.
```

## Etapa 02 - Migration inicial

**Objetivo:** Gerar a migration inicial do schema atual.

**Escopo:**

- Gerar migration `InitialCreate`.
- Usar o `AppDbContext` existente.
- Garantir que a migration contem tabelas esperadas para `User`, `Team`, `Match` e `Bet`.
- Garantir que a migration reflita FKs e indices unicos definidos na Tarefa 02.

**Fora do escopo:**

- Aplicar `database update`.
- Criar banco manualmente.
- Criar seed.
- Alterar entidades ou regras de negocio.

**Criterios de aceite:**

- Arquivo de migration `InitialCreate` foi criado.
- Snapshot EF foi atualizado.
- A solucao compila apos a migration.
- Nenhum seed foi executado.

**Prompt sugerido:**

```text
Gere somente a migration inicial InitialCreate para o AppDbContext existente. A migration deve refletir o schema atual com tabelas User, Team, Match e Bet, FKs e indices unicos ja configurados na Tarefa 02. Nao rode database update, nao crie seed, nao altere entidades para adicionar escopo novo, nao implemente endpoints ou regras de negocio.
```

## Etapa 03 - Revisao da migration gerada

**Objetivo:** Validar que a migration inicial representa corretamente o modelo.

**Escopo:**

- Inspecionar a migration e o model snapshot.
- Conferir tabelas, PKs, FKs, nulabilidade, indices unicos e tipos de colunas.
- Conferir se `HomeGoals` e `AwayGoals` aceitam nulo.
- Conferir se datas foram mapeadas de forma compativel com UTC.

**Fora do escopo:**

- Alterar regras de negocio.
- Criar seed.
- Aplicar migration no banco.
- Corrigir entidades fora do contrato da Tarefa 02.

**Criterios de aceite:**

- Revisao objetiva da migration foi registrada.
- Inconsistencias foram listadas, se houver.
- Nenhum banco foi alterado.

**Prompt sugerido:**

```text
Revise somente a migration InitialCreate e o snapshot EF. Confirme tabelas, PKs, FKs, indices unicos, nulabilidade, tipos de colunas, gols opcionais e datas compativeis com UTC. Se encontrar inconsistencia, registre objetivamente. Nao aplique a migration, nao crie seed, nao implemente endpoints nem regras de negocio.
```

## Etapa 04 - Arquivo fixo de seed de Teams

**Objetivo:** Criar a fonte fixa de dados de selecoes/times.

**Escopo:**

- Criar arquivo de seed para `Team`, preferencialmente JSON.
- Incluir `Name`, `Code` e `FlagUrl`.
- Garantir que `Code` seja estavel e unico.
- Manter o arquivo facil de revisar manualmente.

**Fora do escopo:**

- Inserir dados no banco.
- Criar seed de partidas.
- Buscar dados em API externa.
- Criar endpoints.

**Criterios de aceite:**

- Arquivo fixo de `Team` existe.
- Cada item possui `Name`, `Code` e `FlagUrl`.
- Nao ha `Code` duplicado.
- Nenhum dado foi gravado no banco.

**Prompt sugerido:**

```text
Crie somente a fonte fixa de seed de Team, preferencialmente em JSON, contendo Name, Code e FlagUrl. Code deve ser unico e estavel. Nao grave dados no banco, nao crie seed runner, nao crie partidas, nao use API externa, nao implemente endpoints ou regras de negocio.
```

## Etapa 05 - Arquivo fixo de seed de Matches

**Objetivo:** Criar a fonte fixa de dados de partidas/calendario.

**Escopo:**

- Criar arquivo de seed para `Match`, preferencialmente JSON.
- Referenciar times por `Code`, nao por Id de banco.
- Incluir `HomeTeamCode`, `AwayTeamCode`, `MatchDate`, `Stage` e `Status`.
- Usar `MatchDate` em UTC.
- Manter `HomeGoals` e `AwayGoals` ausentes ou nulos no seed inicial.

**Fora do escopo:**

- Inserir dados no banco.
- Calcular pontuacao.
- Registrar resultados reais.
- Integrar calendario externo.
- Criar endpoints.

**Criterios de aceite:**

- Arquivo fixo de `Match` existe.
- Cada partida referencia times por codigo.
- `MatchDate` esta em UTC.
- `Stage` e `Status` usam valores validos dos enums.
- Nenhum dado foi gravado no banco.

**Prompt sugerido:**

```text
Crie somente a fonte fixa de seed de Match, preferencialmente em JSON. Cada partida deve referenciar HomeTeamCode e AwayTeamCode, incluir MatchDate em UTC, Stage e Status, e manter HomeGoals/AwayGoals nulos ou ausentes. Nao grave dados no banco, nao calcule pontuacao, nao registre resultados reais, nao integre API externa e nao crie endpoints.
```

## Etapa 06 - Regra de preenchimento de AllowBetUntil para seed

**Objetivo:** Isolar a regra de calculo usada pelo seed para `AllowBetUntil`.

**Escopo:**

- Criar funcao/helper interno de seed para calcular `AllowBetUntil`.
- Regra:
  - `Groups`: `MatchDate - 15min`;
  - demais fases: `MatchDate - 30min`.
- Garantir retorno em UTC.
- Adicionar testes unitarios simples se ja houver projeto de testes; se nao houver, registrar validacao manual esperada.

**Fora do escopo:**

- Implementar regra de fechamento de palpite na API.
- Criar endpoints.
- Calcular pontuacao.
- Persistir dados no banco.

**Criterios de aceite:**

- A regra do seed esta isolada e reutilizavel pelo processo de seed.
- Casos de grupo e mata-mata estao cobertos.
- A regra nao depende de horario local.
- Nenhum endpoint usa essa regra ainda.

**Prompt sugerido:**

```text
Crie somente a regra interna usada pelo seed para preencher AllowBetUntil: Groups usa MatchDate - 15min e as demais fases usam MatchDate - 30min, sempre em UTC. Se houver projeto de testes, cubra os dois casos; se nao houver, registre a validacao manual esperada. Nao implemente fechamento de palpite na API, endpoints, pontuacao ou persistencia de seed.
```

## Etapa 07 - Seed idempotente de Teams

**Objetivo:** Implementar insercao/atualizacao idempotente de `Team`.

**Escopo:**

- Ler a fonte fixa de `Team`.
- Inserir times ausentes.
- Atualizar campos simples quando o `Code` ja existir e os dados divergirem.
- Evitar duplicacao por `Code`.

**Fora do escopo:**

- Seed de partidas.
- Migration.
- Endpoint administrativo.
- API externa.

**Criterios de aceite:**

- Rodar o seed de `Team` duas vezes nao duplica dados.
- Times sao localizados por `Code`.
- A operacao nao depende de Id fixo de banco.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente o seed idempotente de Team. Leia a fonte fixa de times, insira ausentes e atualize Name/FlagUrl quando o Code ja existir. Nao dependa de Id fixo. Rodar duas vezes nao pode duplicar dados. Nao implemente seed de Match, endpoints, API externa, migrations novas ou regras de negocio.
```

## Etapa 08 - Seed idempotente de Matches

**Objetivo:** Implementar insercao/atualizacao idempotente de `Match`.

**Escopo:**

- Ler a fonte fixa de `Match`.
- Resolver `HomeTeamId` e `AwayTeamId` a partir de `Team.Code`.
- Preencher `MatchDate` em UTC.
- Preencher `AllowBetUntil` pela regra da Etapa 06 ou equivalente local.
- Inserir partidas ausentes.
- Atualizar campos simples quando a partida ja existir.

**Fora do escopo:**

- Seed de `Team`, exceto validar que os codigos existem.
- Resultados reais das partidas.
- Pontuacao.
- Endpoints.

**Criterios de aceite:**

- Rodar o seed de `Match` duas vezes nao duplica dados.
- Partidas referenciam times existentes.
- `AllowBetUntil` fica coerente com `Stage`.
- `HomeGoals` e `AwayGoals` permanecem nulos no seed inicial.

**Prompt sugerido:**

```text
Implemente somente o seed idempotente de Match. Leia a fonte fixa de partidas, resolva HomeTeamId/AwayTeamId por Team.Code, grave MatchDate em UTC e preencha AllowBetUntil com Groups -15min e demais fases -30min. Rodar duas vezes nao pode duplicar partidas. Nao implemente seed de Team alem de validar codigos, nao registre resultados reais, nao calcule pontuacao e nao crie endpoints.
```

## Etapa 09 - Orquestrador de seed

**Objetivo:** Criar um ponto unico para executar seeds na ordem correta.

**Escopo:**

- Criar rotina de seed que execute `Team` antes de `Match`.
- Garantir logs ou mensagens simples de execucao.
- Garantir falha clara quando um `Match` referencia `Team.Code` inexistente.
- Permitir execucao local controlada.

**Fora do escopo:**

- Aplicar migration automaticamente em producao.
- Criar painel admin.
- Buscar dados externos.
- Executar seed em toda inicializacao sem controle.

**Criterios de aceite:**

- Existe um caminho claro para executar seeds.
- `Team` roda antes de `Match`.
- Erros de referencia de times sao claros.
- Execucao repetida permanece idempotente.

**Prompt sugerido:**

```text
Crie somente um orquestrador de seed para executar Team antes de Match, com mensagens simples de execucao e erro claro quando uma partida referencia Team.Code inexistente. A execucao deve ser controlada/local e idempotente. Nao aplique migration automaticamente em producao, nao crie painel admin, nao busque dados externos e nao implemente endpoints.
```

## Etapa 10 - Script local de aplicacao da migration e seed

**Objetivo:** Facilitar a execucao local da migration e dos seeds.

**Escopo:**

- Criar script local ou comando documentado para:
  - aplicar `dotnet ef database update`;
  - executar seed.
- Usar connection string de configuracao.
- Evitar credenciais hardcoded.
- Manter script simples e voltado a ambiente local.

**Fora do escopo:**

- CI/CD.
- Deploy Azure.
- Provisionar banco remoto.
- Automatizar producao.

**Criterios de aceite:**

- Existe comando/script local para aplicar schema e seed.
- O script nao contem senha fixa.
- O script nao executa endpoints de negocio.
- O processo pode ser repetido sem duplicar seed.

**Prompt sugerido:**

```text
Crie somente um script local ou instrucao executavel para aplicar dotnet ef database update e executar o seed. Use a connection string de configuracao e nao inclua credenciais hardcoded. O foco e ambiente local. Nao implemente CI/CD, deploy Azure, provisionamento remoto, endpoints ou regras de negocio.
```

## Etapa 11 - Documentacao README da migration e seed

**Objetivo:** Documentar como aplicar o schema e popular dados iniciais.

**Escopo:**

- Atualizar README com comandos da Tarefa 03.
- Explicar pre-requisito da Tarefa 02.
- Documentar como aplicar migration.
- Documentar como executar seed.
- Documentar que seed e idempotente.
- Documentar regra de `AllowBetUntil`.

**Fora do escopo:**

- Documentar funcionalidades ainda nao implementadas.
- Criar endpoints.
- Criar regras de pontuacao.
- Documentar deploy.

**Criterios de aceite:**

- README possui comandos claros para migration e seed.
- README nao afirma que ranking, palpites ou autenticacao estao prontos.
- A regra de `AllowBetUntil` do seed esta documentada.

**Prompt sugerido:**

```text
Atualize somente o README com instrucoes da Tarefa 03: pre-requisito da Tarefa 02, comando para aplicar migration, comando para executar seed, idempotencia do seed e regra de AllowBetUntil. Nao documente funcionalidades futuras como prontas, nao crie endpoints, nao implemente pontuacao, autenticacao ou deploy.
```

## Etapa 12 - Validacao final da Tarefa 03

**Objetivo:** Verificar se migration e seed atendem aos criterios originais.

**Escopo:**

- Rodar build da solucao backend.
- Aplicar `dotnet ef database update` em banco local de teste/desenvolvimento.
- Executar seed.
- Conferir tabelas criadas.
- Conferir dados em `Team` e `Match`.
- Conferir que executar seed novamente nao duplica registros.
- Conferir `AllowBetUntil` conforme fase.
- Conferir datas em UTC.

**Fora do escopo:**

- Testar endpoints de negocio.
- Calcular pontuacao.
- Registrar resultados reais.
- Criar ou alterar features fora da Tarefa 03.

**Criterios de aceite:**

- `dotnet ef database update` cria o schema.
- `Team` e `Match` ficam populados.
- Seed e idempotente.
- `AllowBetUntil` esta correto para grupos e mata-mata.
- Datas persistidas estao em UTC.
- Pendencias foram registradas objetivamente, se houver.

**Prompt sugerido:**

```text
Valide somente a Tarefa 03 em ambiente local de teste/desenvolvimento. Rode build, aplique dotnet ef database update, execute seed, confira tabelas, dados de Team e Match, idempotencia do seed, AllowBetUntil por fase e datas em UTC. Se algo falhar, registre a pendencia objetivamente. Nao implemente endpoints, pontuacao, autenticacao, resultados reais ou frontend.
```

## Mapeamento do escopo original para as etapas

| Item da Tarefa 03 | Etapa responsavel |
|---|---|
| Validar dependencia da Tarefa 02 | Etapa 01 |
| Migration inicial criando tabelas | Etapa 02 |
| Revisao de schema, FKs e indices | Etapa 03 |
| Seed fixo de `Team` | Etapas 04 e 07 |
| Seed fixo de `Match` | Etapas 05 e 08 |
| `MatchDate` em UTC | Etapas 05, 08 e 12 |
| `AllowBetUntil` por regra 15/30 min | Etapas 06, 08, 11 e 12 |
| Seed idempotente | Etapas 07, 08, 09 e 12 |
| Script ou instrucao de aplicacao | Etapas 10 e 11 |
| `dotnet ef database update` cria schema | Etapa 12 |

