# Tarefa 02 - Divisao em etapas independentes

Fonte: `C:\Repo\bolao-copa-backlog.md`, secao **Tarefa 02 - Modelagem de dominio e configuracao EF Core**.

Este arquivo quebra a Tarefa 02 em etapas pequenas, fechadas e implementaveis separadamente. Nenhuma etapa abaixo deve criar migrations, seed, endpoints, autenticacao, regras de pontuacao ou frontend.

## Escopo original da Tarefa 02

Modelar o dominio e configurar EF Core com PostgreSQL, sem aplicar migration:

- Entidades `User`, `Team`, `Match` e `Bet`.
- Enums `Stage` e `MatchStatus`.
- Configuracoes EF Core via Fluent API.
- FKs de `Match.HomeTeamId` e `Match.AwayTeamId` para `Team`.
- Indice unico `(UserId, MatchId)` em `Bet`.
- Indice unico em `User.Email`.
- Datas modeladas como UTC.
- Provider Npgsql configurado por connection string em `appsettings`.
- `DbContext` reconhecido por `dotnet ef dbcontext info`.

## Decisoes assumidas para a Tarefa 02

Como as duvidas do backlog ainda aparecem como pontos de confirmacao, a Tarefa 02 deve usar apenas o contrato minimo ja descrito:

- `Stage`: `Groups`, `RoundOf16`, `QuarterFinals`, `SemiFinals`, `Final`.
- `MatchStatus`: `Scheduled`, `InProgress`, `Finished`.
- `User.Email` deve ser unico.
- `Bet` deve ser unico por par `(UserId, MatchId)`.
- `AllowBetUntil` sera persistido como `DateTime` em UTC.
- Campos de data devem ser tratados como UTC no modelo.
- Npgsql sera o provider padrao.

Nao incluir disputa de terceiro lugar, status `Postponed`/`Canceled`, seed, resultado real externo ou campos futuros sem confirmacao explicita.

## Regra de independencia

Cada etapa deve poder ser implementada sem depender da execucao previa de outra etapa deste arquivo.

Para preservar essa independencia:

- A etapa deve inspecionar o estado atual antes de alterar arquivos.
- A etapa deve preservar tipos, configuracoes e nomes ja existentes quando forem compativeis.
- A etapa deve criar apenas o minimo necessario para seu proprio objetivo compilar.
- Se uma estrutura base da Tarefa 01 estiver ausente, registrar bloqueio objetivo em vez de implementar toda a Tarefa 01.
- A etapa nao deve antecipar migrations, seed, endpoints ou regras de negocio.
- Validacoes globais da Tarefa 02 so devem ser exigidas quando todas as etapas forem executadas.

## Etapa 01 - Enums de dominio

**Objetivo:** Criar os enums usados por partidas e regras futuras.

**Escopo:**

- Criar `Stage` no projeto `Domain`.
- Criar `MatchStatus` no projeto `Domain`.
- Usar nomes em ingles conforme contrato da Tarefa 02.
- Manter os enums simples, sem logica de negocio.

**Fora do escopo:**

- Criar entidades.
- Configurar EF Core.
- Criar conversores, migrations ou seed.
- Adicionar fases ou status nao confirmados.

**Criterios de aceite:**

- `Stage` contem `Groups`, `RoundOf16`, `QuarterFinals`, `SemiFinals`, `Final`.
- `MatchStatus` contem `Scheduled`, `InProgress`, `Finished`.
- O projeto `Domain` compila.
- Nenhum enum futuro ou nao confirmado foi incluido.

**Prompt sugerido:**

```text
No projeto Domain, crie somente os enums Stage e MatchStatus. Stage deve conter Groups, RoundOf16, QuarterFinals, SemiFinals e Final. MatchStatus deve conter Scheduled, InProgress e Finished. Preserve qualquer tipo existente compativel. Nao crie entidades, DbContext, migrations, seed, endpoints ou regras de negocio.
```

## Etapa 02 - Entidade User

**Objetivo:** Modelar o usuario no dominio, sem autenticar nem emitir token.

**Escopo:**

- Criar ou ajustar a entidade `User`.
- Campos: `Id`, `Name`, `Email`, `PasswordHash`, `CreatedAt`.
- Usar tipos simples e compativeis com EF Core.
- Manter `CreatedAt` como data UTC no contrato.

**Fora do escopo:**

- Registro, login, JWT ou hashing.
- Validacao de senha.
- Configuracao do indice unico de email.
- Endpoints.

**Criterios de aceite:**

- `User` existe no projeto `Domain`.
- A entidade possui os campos previstos.
- A entidade nao contem logica de autenticacao.
- O projeto `Domain` compila.

**Prompt sugerido:**

```text
No projeto Domain, crie ou ajuste somente a entidade User com Id, Name, Email, PasswordHash e CreatedAt. CreatedAt deve representar uma data em UTC, mas nao implemente conversao global agora. Preserve conteudo existente compativel. Nao implemente registro, login, JWT, hashing, endpoints, DbContext, migrations ou seed.
```

## Etapa 03 - Entidade Team

**Objetivo:** Modelar selecoes/times no dominio.

**Escopo:**

- Criar ou ajustar a entidade `Team`.
- Campos: `Id`, `Name`, `Code`, `FlagUrl`.
- Manter a entidade sem dependencia de calendario, seed ou API externa.

**Fora do escopo:**

- Popular lista de selecoes.
- Buscar bandeiras.
- Configurar FKs de partidas.
- Criar migrations ou seed.

**Criterios de aceite:**

- `Team` existe no projeto `Domain`.
- A entidade possui os campos previstos.
- Nao ha dados fixos ou seed dentro da entidade.
- O projeto `Domain` compila.

**Prompt sugerido:**

```text
No projeto Domain, crie ou ajuste somente a entidade Team com Id, Name, Code e FlagUrl. Preserve conteudo existente compativel. Nao implemente seed, calendario, busca de bandeiras, DbContext, migrations, endpoints ou regras de negocio.
```

## Etapa 04 - Entidade Match

**Objetivo:** Modelar partidas no dominio.

**Escopo:**

- Criar ou ajustar a entidade `Match`.
- Campos: `Id`, `HomeTeamId`, `AwayTeamId`, `HomeGoals?`, `AwayGoals?`, `MatchDate`, `Stage`, `Status`, `AllowBetUntil`.
- Usar `Stage` e `MatchStatus`.
- Modelar `MatchDate` e `AllowBetUntil` como datas UTC no contrato.

**Fora do escopo:**

- Calcular `AllowBetUntil`.
- Criar seed de calendario.
- Implementar status automatico.
- Configurar FKs no EF.
- Criar endpoints.

**Criterios de aceite:**

- `Match` existe no projeto `Domain`.
- A entidade possui os campos previstos.
- `HomeGoals` e `AwayGoals` permitem ausencia de resultado.
- `Stage` e `Status` usam os enums definidos no dominio.
- O projeto `Domain` compila.

**Prompt sugerido:**

```text
No projeto Domain, crie ou ajuste somente a entidade Match com Id, HomeTeamId, AwayTeamId, HomeGoals?, AwayGoals?, MatchDate, Stage, Status e AllowBetUntil. Use os enums Stage e MatchStatus; se eles nao existirem, crie apenas os valores minimos da Tarefa 02. Nao calcule AllowBetUntil, nao crie seed, migrations, endpoints, regras de status ou configuracao EF.
```

## Etapa 05 - Entidade Bet

**Objetivo:** Modelar palpites no dominio, sem regra de pontuacao.

**Escopo:**

- Criar ou ajustar a entidade `Bet`.
- Campos: `Id`, `UserId`, `MatchId`, `HomeGoalsPrediction`, `AwayGoalsPrediction`, `PointsEarned`, `CreatedAt`.
- Manter `PointsEarned` como armazenamento do resultado da regra futura, sem calcular nada aqui.
- Manter `CreatedAt` como data UTC no contrato.

**Fora do escopo:**

- Calcular pontuacao.
- Validar janela de palpite.
- Configurar indice unico `(UserId, MatchId)`.
- Criar endpoints de palpite.

**Criterios de aceite:**

- `Bet` existe no projeto `Domain`.
- A entidade possui os campos previstos.
- A entidade nao contem regra de pontuacao.
- O projeto `Domain` compila.

**Prompt sugerido:**

```text
No projeto Domain, crie ou ajuste somente a entidade Bet com Id, UserId, MatchId, HomeGoalsPrediction, AwayGoalsPrediction, PointsEarned e CreatedAt. PointsEarned deve ser apenas um campo persistivel, sem calculo de pontuacao. Nao implemente janela de palpite, endpoints, Fluent API, migrations ou seed.
```

## Etapa 06 - AppDbContext e DbSets

**Objetivo:** Criar o `DbContext` principal da aplicacao.

**Escopo:**

- Criar `AppDbContext` no projeto `Infrastructure`.
- Herdar de `DbContext`.
- Registrar `DbSet<User>`, `DbSet<Team>`, `DbSet<Match>` e `DbSet<Bet>`.
- Usar entidades do projeto `Domain`.
- Garantir que `Infrastructure` referencia `Domain`.

**Fora do escopo:**

- Fluent API detalhada.
- Provider Npgsql.
- Connection string.
- Migrations ou seed.
- Endpoints.

**Criterios de aceite:**

- `AppDbContext` existe no projeto `Infrastructure`.
- Os quatro `DbSet`s estao registrados.
- A solucao compila.
- Nenhuma migration foi criada.

**Prompt sugerido:**

```text
No projeto Infrastructure, crie ou ajuste somente o AppDbContext herdando de DbContext e registrando DbSet<User>, DbSet<Team>, DbSet<Match> e DbSet<Bet>. Garanta a referencia Infrastructure -> Domain se necessario. Se alguma entidade ainda nao existir, crie apenas a forma minima compativel com o contrato da Tarefa 02 para a solucao compilar. Nao configure Fluent API detalhada, Npgsql, connection string, migrations, seed ou endpoints.
```

## Etapa 07 - Configuracao Fluent API de User e Bet

**Objetivo:** Configurar restricoes de unicidade ligadas a usuario e palpite.

**Escopo:**

- Configurar chave primaria de `User`.
- Configurar indice unico em `User.Email`.
- Configurar chave primaria de `Bet`.
- Configurar FK `Bet.UserId -> User`.
- Configurar FK `Bet.MatchId -> Match`.
- Configurar indice unico `(UserId, MatchId)` em `Bet`.

**Fora do escopo:**

- Configurar `Match` e `Team`.
- Criar regras de negocio de palpite.
- Validar senha ou email.
- Criar migrations ou seed.

**Criterios de aceite:**

- As configuracoes de `User` e `Bet` estao aplicadas via Fluent API.
- `User.Email` possui indice unico.
- `Bet` possui indice unico por `(UserId, MatchId)`.
- A solucao compila.

**Prompt sugerido:**

```text
Configure somente o mapeamento Fluent API de User e Bet no Infrastructure. User deve ter chave primaria e indice unico em Email. Bet deve ter chave primaria, FK para User, FK para Match e indice unico em (UserId, MatchId). Preserve configuracoes existentes compativeis. Nao crie migrations, seed, endpoints, validacao de autenticacao ou regras de pontuacao.
```

## Etapa 08 - Configuracao Fluent API de Team e Match

**Objetivo:** Configurar o relacionamento entre partidas e times.

**Escopo:**

- Configurar chave primaria de `Team`.
- Configurar chave primaria de `Match`.
- Configurar FK `Match.HomeTeamId -> Team`.
- Configurar FK `Match.AwayTeamId -> Team`.
- Configurar tipos basicos de colunas de `Match`, incluindo gols opcionais.
- Garantir que `Stage` e `Status` sejam persistidos de forma consistente.

**Fora do escopo:**

- Seed de times ou partidas.
- Calculo de `AllowBetUntil`.
- Endpoints de partidas.
- Migrations.

**Criterios de aceite:**

- As FKs de `Match` para `Team` estao configuradas.
- `HomeGoals` e `AwayGoals` aceitam valor nulo.
- `Stage` e `Status` possuem estrategia de persistencia explicita.
- A solucao compila.

**Prompt sugerido:**

```text
Configure somente o mapeamento Fluent API de Team e Match no Infrastructure. Team e Match devem ter chave primaria. Match deve ter FKs HomeTeamId -> Team e AwayTeamId -> Team, gols finais opcionais, Stage e Status com persistencia explicita. Preserve configuracoes existentes compativeis. Nao crie seed, migrations, endpoints, calculo de AllowBetUntil ou regras de negocio.
```

## Etapa 09 - Npgsql, connection string e registro de DI

**Objetivo:** Configurar o provider PostgreSQL sem criar migration.

**Escopo:**

- Adicionar os pacotes EF Core/Npgsql necessarios ao projeto `Infrastructure` e/ou `Api`.
- Configurar connection string em `appsettings`.
- Registrar `AppDbContext` no container de DI da API usando Npgsql.
- Manter a connection string como configuracao, sem hardcode em codigo.

**Fora do escopo:**

- Rodar `database update`.
- Criar migration.
- Criar seed.
- Criar endpoints de negocio.

**Criterios de aceite:**

- A API registra `AppDbContext` com Npgsql.
- A connection string existe em `appsettings`.
- A solucao compila.
- Nenhum banco foi criado ou alterado.

**Prompt sugerido:**

```text
Configure somente o provider PostgreSQL para o AppDbContext. Adicione os pacotes EF Core/Npgsql necessarios, crie a connection string em appsettings e registre o AppDbContext no DI da API usando UseNpgsql. Nao crie migration, nao rode database update, nao crie seed, endpoints, entidades novas fora do contrato ou regras de negocio.
```

## Etapa 10 - Tratamento de datas UTC no modelo EF

**Objetivo:** Garantir que datas persistidas pela Tarefa 02 tenham contrato UTC.

**Escopo:**

- Revisar campos `CreatedAt`, `MatchDate` e `AllowBetUntil`.
- Configurar conversao ou convencao EF para preservar `DateTimeKind.Utc`, se o projeto usar `DateTime`.
- Documentar no codigo apenas quando necessario para evitar ambiguidade.
- Manter o modelo sem calculo de janela de palpite.

**Fora do escopo:**

- Calcular `AllowBetUntil`.
- Converter fuso para exibicao.
- Usar timezone do usuario.
- Alterar regras de negocio.
- Criar migration.

**Criterios de aceite:**

- Campos de data da Tarefa 02 possuem tratamento UTC consistente.
- A solucao compila.
- Nao ha conversao para horario local.
- Nenhuma regra de fechamento de palpite foi implementada.

**Prompt sugerido:**

```text
Revise somente o tratamento de datas UTC no modelo EF. Garanta que CreatedAt, MatchDate e AllowBetUntil sejam persistidos/lidos de forma consistente como UTC, usando conversao ou convencao EF se necessario. Nao calcule AllowBetUntil, nao implemente timezone de usuario, exibicao local, migrations, seed, endpoints ou regras de negocio.
```

## Etapa 11 - Reconhecimento do DbContext pelo EF tooling

**Objetivo:** Garantir que `dotnet ef dbcontext info` reconheca o contexto.

**Escopo:**

- Ajustar configuracao de design-time se necessario.
- Garantir que o startup project da API consiga resolver `AppDbContext`.
- Documentar o comando correto para inspecionar o contexto.
- Validar sem criar migration.

**Fora do escopo:**

- Criar migrations.
- Aplicar banco.
- Criar seed.
- Alterar schema por fora do modelo.

**Criterios de aceite:**

- `dotnet ef dbcontext info` reconhece `AppDbContext`.
- A solucao compila.
- Nenhuma migration foi criada.
- Nenhum banco foi alterado.

**Prompt sugerido:**

```text
Garanta somente que o EF tooling reconheca o AppDbContext. Ajuste design-time factory ou configuracao de startup se necessario e documente o comando dotnet ef dbcontext info correto. Nao crie migrations, nao rode database update, nao crie seed, endpoints ou regras de negocio.
```

## Etapa 12 - Validacao final da Tarefa 02

**Objetivo:** Verificar se o estado atual atende aos criterios originais da Tarefa 02.

**Escopo:**

- Rodar build da solucao backend.
- Verificar existencia das entidades e enums.
- Verificar `DbSet`s no `AppDbContext`.
- Verificar Fluent API: FKs, indices unicos e tipos relevantes.
- Verificar provider Npgsql por connection string.
- Rodar `dotnet ef dbcontext info`.
- Registrar pendencias encontradas sem implementar escopo futuro.

**Fora do escopo:**

- Criar migrations.
- Rodar `database update`.
- Criar seed.
- Criar endpoints.
- Implementar regra de pontuacao ou autenticacao.

**Criterios de aceite:**

- Resultado dos comandos de validacao registrado.
- Pendencias listadas objetivamente, se houver.
- Nenhuma migration, seed ou feature fora da Tarefa 02 foi criada durante a validacao.

**Prompt sugerido:**

```text
Valide somente a Tarefa 02 no estado atual do repositorio. Rode o build, confira entidades User/Team/Match/Bet, enums Stage/MatchStatus, DbSets, Fluent API de FKs e indices unicos, Npgsql por connection string e dotnet ef dbcontext info. Se algo falhar, registre a pendencia objetivamente. Nao crie migrations, nao rode database update, nao crie seed, endpoints, autenticacao ou regra de pontuacao.
```

## Mapeamento do escopo original para as etapas

| Item da Tarefa 02 | Etapa responsavel |
|---|---|
| Enum `Stage` | Etapa 01 |
| Enum `MatchStatus` | Etapa 01 |
| Entidade `User` | Etapa 02 |
| Entidade `Team` | Etapa 03 |
| Entidade `Match` | Etapa 04 |
| Entidade `Bet` | Etapa 05 |
| `AppDbContext` e `DbSet`s | Etapa 06 |
| Indice unico em `User.Email` | Etapa 07 |
| FKs de `Bet` para `User` e `Match` | Etapa 07 |
| Indice unico `(UserId, MatchId)` em `Bet` | Etapa 07 |
| FKs de `Match` para `Team` | Etapa 08 |
| Persistencia de `Stage` e `Status` | Etapa 08 |
| Provider Npgsql e connection string | Etapa 09 |
| Datas em UTC | Etapa 10 |
| `dotnet ef dbcontext info` | Etapa 11 |
| Validacao dos criterios de aceite originais | Etapa 12 |

