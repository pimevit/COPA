# Tarefa 06 - Divisao em etapas independentes

Fonte: `C:\Repo\bolao-copa-backlog.md`, secao **Tarefa 06 - Endpoints de Matches**.

Este arquivo quebra a Tarefa 06 em etapas pequenas, fechadas e implementaveis separadamente. Nenhuma etapa abaixo deve implementar criacao/edicao de palpites, cadastro de resultado, pontuacao, autenticacao, frontend, seed ou migrations.

## Escopo original da Tarefa 06

Expor endpoints de leitura de partidas:

- `GET /matches` com filtros opcionais por `Stage` e `Status`.
- Ordenacao por `MatchDate`.
- `GET /matches/{id}` com dados das selecoes resolvidos:
  - nome;
  - code;
  - flag;
  - data;
  - fase;
  - status;
  - resultado quando houver.
- Campo `isBettingOpen` no DTO, derivado de `AllowBetUntil` comparado com o instante atual em UTC.
- Query eficiente, sem N+1.
- Logica de leitura em `Application`.

## Dependencia base

A Tarefa 06 depende das Tarefas 02 e 03.

Cada etapa abaixo deve ser independente das outras etapas deste arquivo, mas pode exigir que exista:

- entidade `Team`;
- entidade `Match`;
- enums `Stage` e `MatchStatus`;
- `AppDbContext` com `DbSet<Team>` e `DbSet<Match>`;
- migration aplicada em ambiente local/desenvolvimento;
- seed de `Team` e `Match`;
- API executavel.

Se esse contrato nao existir, a etapa deve registrar bloqueio objetivo e nao deve implementar T02 ou T03 dentro da Tarefa 06.

## Decisoes assumidas para a Tarefa 06

- Endpoints de partidas sao publicos no MVP, salvo decisao contraria futura.
- `GET /matches` retorna partidas ordenadas por `MatchDate` crescente.
- Filtros `Stage` e `Status` sao opcionais.
- Filtros invalidos devem retornar `400`, preferencialmente seguindo o padrao transversal de erros se a Tarefa 05 existir.
- `GET /matches/{id}` retorna `404` quando a partida nao existe.
- `isBettingOpen` deve ser calculado com `DateTime.UtcNow` ou abstracao equivalente de relogio UTC.
- `isBettingOpen = true` quando o instante atual UTC for menor ou igual a `AllowBetUntil`, salvo regra mais restritiva futura.
- Resultado so deve aparecer quando `HomeGoals` e `AwayGoals` estiverem preenchidos.
- Queries devem ser read-only e usar projecao eficiente, evitando N+1.

## Regra de independencia

Cada etapa deve poder ser implementada sozinha, sem depender da execucao previa de outra etapa deste arquivo.

Para preservar essa independencia:

- A etapa deve inspecionar o estado atual antes de alterar arquivos.
- A etapa deve preservar contratos, endpoints e queries existentes quando forem compativeis.
- A etapa deve criar apenas o minimo necessario para seu proprio objetivo compilar.
- Se faltar contrato das Tarefas 02/03, registrar bloqueio e parar.
- A etapa nao deve criar dados, migrations, seeds ou endpoints fora de `matches`.
- Validacoes globais da Tarefa 06 so devem ser exigidas quando todas as etapas forem executadas.

## Etapa 01 - Pre-validacao das dependencias de Matches

**Objetivo:** Confirmar que existe base de dados e dominio para leitura de partidas.

**Escopo:**

- Verificar se a solucao backend compila.
- Verificar se `Team`, `Match`, `Stage` e `MatchStatus` existem.
- Verificar se `AppDbContext` possui `DbSet<Team>` e `DbSet<Match>`.
- Verificar se existem dados de seed de times e partidas, quando aplicavel.
- Verificar se a API executa.

**Fora do escopo:**

- Criar entidades.
- Criar migration.
- Criar seed.
- Criar endpoints.
- Implementar queries.

**Criterios de aceite:**

- Dependencias minimas foram confirmadas.
- Bloqueios foram registrados objetivamente, se houver.
- Nenhum endpoint de `matches` foi criado.

**Prompt sugerido:**

```text
Valide somente as dependencias da Tarefa 06. Confira build, entidades Team/Match, enums Stage/MatchStatus, DbSet<Team>, DbSet<Match>, dados de seed quando aplicavel e API executavel. Se faltar algo, registre bloqueio objetivamente. Nao crie entidades, migrations, seed, queries ou endpoints.
```

## Etapa 02 - DTOs de resposta de Matches

**Objetivo:** Definir contratos de resposta para listagem e detalhe de partidas.

**Escopo:**

- Criar DTO para dados resumidos de selecao/time.
- Criar DTO de item de lista de partida.
- Criar DTO de detalhe de partida.
- Incluir dados de `homeTeam` e `awayTeam`.
- Incluir `matchDate`, `stage`, `status`, `homeGoals`, `awayGoals` e `isBettingOpen`.
- Manter resultado como campos nulos quando ainda nao houver placar.

**Fora do escopo:**

- Criar queries EF.
- Criar endpoints.
- Criar DTOs de palpite.
- Criar regra de pontuacao.

**Criterios de aceite:**

- DTOs existem em camada apropriada.
- DTOs nao expõem entidades EF diretamente.
- DTOs contemplam lista e detalhe.
- A solucao compila.

**Prompt sugerido:**

```text
Crie somente os DTOs de resposta de Matches. Inclua DTO de Team resumido, item de lista e detalhe de partida, com homeTeam, awayTeam, matchDate, stage, status, homeGoals, awayGoals e isBettingOpen. Resultado deve aceitar nulo. Nao crie queries EF, endpoints, DTOs de palpite ou regra de pontuacao.
```

## Etapa 03 - Contrato de filtros de Matches

**Objetivo:** Definir como `GET /matches` recebe filtros opcionais.

**Escopo:**

- Criar contrato/objeto de query para filtros de listagem.
- Suportar filtros opcionais por `Stage` e `Status`.
- Definir comportamento para valores ausentes.
- Definir comportamento para valores invalidos.

**Fora do escopo:**

- Implementar query no banco.
- Criar endpoint HTTP.
- Criar filtros por data, time ou rodada.
- Criar validacoes de negocio.

**Criterios de aceite:**

- Contrato de filtro existe.
- `Stage` e `Status` sao opcionais.
- Valores invalidos tem tratamento definido.
- A solucao compila.

**Prompt sugerido:**

```text
Crie somente o contrato de filtros de GET /matches. Ele deve aceitar Stage e Status como opcionais e definir tratamento para valores ausentes e invalidos. Nao implemente query no banco, endpoint HTTP, filtros por data/time/rodada ou validacoes de negocio.
```

## Etapa 04 - Abstracao de relogio UTC

**Objetivo:** Isolar a obtencao do instante atual usado por `isBettingOpen`.

**Escopo:**

- Criar ou reutilizar abstracao para obter o instante atual em UTC.
- Registrar no DI, se necessario.
- Permitir teste deterministico do calculo de janela.
- Evitar uso de horario local.

**Fora do escopo:**

- Calcular pontuacao.
- Calcular `AllowBetUntil`.
- Implementar regras de timezone de usuario.
- Criar endpoints.

**Criterios de aceite:**

- Existe forma testavel de obter `now` em UTC.
- Codigo consumidor nao depende de horario local.
- A solucao compila.

**Prompt sugerido:**

```text
Crie ou reutilize somente uma abstracao de relogio UTC para obter o instante atual de forma testavel. Registre no DI se necessario. Nao calcule pontuacao, nao calcule AllowBetUntil, nao implemente timezone de usuario e nao crie endpoints.
```

## Etapa 05 - Helper de isBettingOpen

**Objetivo:** Isolar o calculo de abertura da janela de palpite para DTOs de partidas.

**Escopo:**

- Criar funcao/helper simples para calcular `isBettingOpen`.
- Comparar `AllowBetUntil` com o instante atual UTC.
- Cobrir casos antes, exatamente no limite e depois do limite.
- Manter regra sem dependencia de HTTP ou EF.

**Fora do escopo:**

- Bloquear criacao/edicao de palpites.
- Calcular `AllowBetUntil`.
- Criar endpoints.
- Criar regra de pontuacao.

**Criterios de aceite:**

- `isBettingOpen` e calculado em UTC.
- Antes do limite retorna aberto.
- Depois do limite retorna fechado.
- Regra e testavel sem banco.

**Prompt sugerido:**

```text
Crie somente um helper ou servico simples para calcular isBettingOpen comparando AllowBetUntil com o instante atual UTC. Cubra antes, exatamente no limite e depois do limite se houver testes. Nao implemente bloqueio de palpites, calculo de AllowBetUntil, endpoints, EF ou pontuacao.
```

## Etapa 06 - Query de listagem no Application

**Objetivo:** Implementar a leitura de lista de partidas na camada `Application`.

**Escopo:**

- Criar query/servico de listagem em `Application`.
- Aplicar filtros opcionais por `Stage` e `Status`.
- Ordenar por `MatchDate` crescente.
- Projetar dados para DTO de lista.
- Incluir `isBettingOpen`.
- Manter leitura sem rastreamento, quando EF for usado.

**Fora do escopo:**

- Criar endpoint HTTP.
- Criar detalhe por id.
- Criar ou editar palpites.
- Registrar resultado.
- Calcular pontuacao.

**Criterios de aceite:**

- Query retorna lista ordenada por `MatchDate`.
- Filtros por `Stage` e `Status` funcionam.
- DTO inclui times resolvidos e `isBettingOpen`.
- Query nao causa N+1.

**Prompt sugerido:**

```text
Implemente somente a query de listagem de matches na camada Application. Ela deve aplicar filtros opcionais por Stage e Status, ordenar por MatchDate crescente, projetar para DTO de lista com homeTeam, awayTeam e isBettingOpen, e evitar N+1. Nao crie endpoint HTTP, detalhe por id, palpites, cadastro de resultado ou pontuacao.
```

## Etapa 07 - Query de detalhe no Application

**Objetivo:** Implementar a leitura de uma partida por id na camada `Application`.

**Escopo:**

- Criar query/servico de detalhe em `Application`.
- Buscar partida por `id`.
- Projetar dados para DTO de detalhe.
- Resolver dados de `homeTeam` e `awayTeam`.
- Incluir resultado quando houver.
- Incluir `isBettingOpen`.
- Definir retorno nulo/erro de nao encontrado para endpoint mapear como `404`.

**Fora do escopo:**

- Criar endpoint HTTP.
- Criar listagem.
- Criar/editar palpites.
- Registrar resultado.
- Calcular pontuacao.

**Criterios de aceite:**

- Query retorna detalhe com times resolvidos.
- Partida inexistente tem resultado claro para mapear `404`.
- DTO inclui `isBettingOpen`.
- Query nao causa N+1.

**Prompt sugerido:**

```text
Implemente somente a query de detalhe de match na camada Application. Ela deve buscar por id, projetar DTO de detalhe com homeTeam, awayTeam, matchDate, stage, status, resultado quando houver e isBettingOpen, evitando N+1. Partida inexistente deve ter retorno claro para o endpoint mapear como 404. Nao crie endpoint HTTP, listagem, palpites, resultado ou pontuacao.
```

## Etapa 08 - Endpoint GET /matches

**Objetivo:** Expor a listagem de partidas via HTTP.

**Escopo:**

- Criar endpoint `GET /matches`.
- Receber filtros opcionais `Stage` e `Status`.
- Chamar a query de listagem.
- Retornar `200` com lista.
- Retornar `400` para filtros invalidos, se aplicavel.

**Fora do escopo:**

- Criar endpoint de detalhe.
- Criar endpoint de palpites.
- Criar endpoint de resultados.
- Criar autenticacao obrigatoria.
- Calcular pontuacao.

**Criterios de aceite:**

- `GET /matches` retorna lista ordenada por data.
- Filtros por `Stage` e `Status` funcionam.
- Filtro invalido retorna erro claro.
- Resposta inclui `isBettingOpen`.

**Prompt sugerido:**

```text
Crie somente o endpoint GET /matches. Ele deve receber filtros opcionais Stage e Status, chamar a query de listagem, retornar 200 com lista ordenada por MatchDate e retornar 400 para filtros invalidos quando aplicavel. A resposta deve incluir isBettingOpen. Nao crie detalhe, palpites, resultados, autenticacao obrigatoria ou pontuacao.
```

## Etapa 09 - Endpoint GET /matches/{id}

**Objetivo:** Expor o detalhe de uma partida via HTTP.

**Escopo:**

- Criar endpoint `GET /matches/{id}`.
- Chamar a query de detalhe.
- Retornar `200` com DTO de detalhe quando existir.
- Retornar `404` quando nao existir.

**Fora do escopo:**

- Criar endpoint de listagem.
- Criar endpoint de palpites.
- Criar endpoint de resultados.
- Calcular pontuacao.

**Criterios de aceite:**

- `GET /matches/{id}` retorna detalhe com times resolvidos.
- Partida inexistente retorna `404`.
- Resposta inclui resultado quando houver.
- Resposta inclui `isBettingOpen`.

**Prompt sugerido:**

```text
Crie somente o endpoint GET /matches/{id}. Ele deve chamar a query de detalhe, retornar 200 com DTO de detalhe quando existir e 404 quando nao existir. O DTO deve incluir times resolvidos, resultado quando houver e isBettingOpen. Nao crie listagem, palpites, resultados administrativos ou pontuacao.
```

## Etapa 10 - Tratamento de filtros e enum parsing

**Objetivo:** Garantir que filtros `Stage` e `Status` tenham comportamento previsivel.

**Escopo:**

- Padronizar parsing de `Stage` e `Status`.
- Aceitar apenas valores existentes nos enums.
- Retornar erro claro para valor invalido.
- Evitar fallback silencioso para filtro ausente.

**Fora do escopo:**

- Criar novos valores de enum.
- Criar filtros adicionais.
- Criar regras de negocio.
- Alterar seed.

**Criterios de aceite:**

- Valores validos filtram corretamente.
- Valores invalidos retornam `400`.
- Ausencia de filtro retorna todas as partidas.
- A solucao compila.

**Prompt sugerido:**

```text
Padronize somente o tratamento dos filtros Stage e Status em GET /matches. Aceite apenas valores existentes nos enums, retorne 400 claro para valores invalidos e nao faca fallback silencioso. Ausencia de filtro deve retornar todas as partidas. Nao crie novos enums, filtros adicionais, regras de negocio ou seed.
```

## Etapa 11 - Testes da camada Application

**Objetivo:** Cobrir as queries de listagem e detalhe sem depender de HTTP.

**Escopo:**

- Testar listagem ordenada por `MatchDate`.
- Testar filtro por `Stage`.
- Testar filtro por `Status`.
- Testar `isBettingOpen` antes/depois do limite.
- Testar detalhe existente.
- Testar detalhe inexistente.

**Fora do escopo:**

- Testar criacao de palpite.
- Testar pontuacao.
- Testar frontend.
- Criar suite E2E ampla.

**Criterios de aceite:**

- Queries principais estao cobertas.
- Testes usam relogio UTC controlado.
- Testes nao dependem de API externa.
- Testes passam.

**Prompt sugerido:**

```text
Crie somente testes da camada Application para Matches. Cubra listagem ordenada por MatchDate, filtro por Stage, filtro por Status, isBettingOpen antes/depois do limite, detalhe existente e detalhe inexistente. Use relogio UTC controlado. Nao teste palpites, pontuacao, frontend ou E2E amplo.
```

## Etapa 12 - Testes de API para Matches

**Objetivo:** Validar o comportamento HTTP dos endpoints de partidas.

**Escopo:**

- Testar `GET /matches` retornando `200`.
- Testar filtros validos.
- Testar filtro invalido retornando `400`.
- Testar `GET /matches/{id}` existente retornando `200`.
- Testar `GET /matches/{id}` inexistente retornando `404`.
- Conferir presenca de dados dos times e `isBettingOpen`.

**Fora do escopo:**

- Testar endpoints de palpites.
- Testar autenticacao.
- Testar pontuacao.
- Testar frontend.

**Criterios de aceite:**

- Endpoints de matches possuem cobertura HTTP basica.
- Contratos de resposta sao verificados.
- Testes nao exigem servico externo.
- Testes passam.

**Prompt sugerido:**

```text
Crie somente testes de API para endpoints de Matches. Cubra GET /matches 200, filtros validos, filtro invalido 400, GET /matches/{id} existente 200, inexistente 404, dados dos times e isBettingOpen. Nao teste palpites, autenticacao, pontuacao ou frontend.
```

## Etapa 13 - Documentacao local dos endpoints de Matches

**Objetivo:** Documentar como consumir e validar os endpoints de partidas.

**Escopo:**

- Atualizar README ou documentacao tecnica.
- Documentar `GET /matches`.
- Documentar filtros `Stage` e `Status`.
- Documentar `GET /matches/{id}`.
- Documentar `isBettingOpen`.
- Informar que criacao/edicao de palpites, resultado e pontuacao estao fora desta tarefa.

**Fora do escopo:**

- Documentar endpoints futuros como prontos.
- Criar Swagger customizado.
- Documentar frontend.
- Documentar deploy.

**Criterios de aceite:**

- Documentacao permite testar listagem e detalhe.
- Filtros e exemplos estao claros.
- O escopo ainda pendente esta claro.

**Prompt sugerido:**

```text
Atualize somente a documentacao local dos endpoints de Matches. Documente GET /matches, filtros Stage/Status, GET /matches/{id}, campo isBettingOpen e exemplos simples. Deixe claro que criacao/edicao de palpites, cadastro de resultado e pontuacao estao fora desta tarefa. Nao documente endpoints futuros como prontos, frontend ou deploy.
```

## Etapa 14 - Validacao final da Tarefa 06

**Objetivo:** Verificar se os endpoints de Matches atendem aos criterios originais.

**Escopo:**

- Rodar build e testes.
- Subir a API.
- Executar `GET /matches`.
- Validar ordenacao por `MatchDate`.
- Validar filtros por `Stage` e `Status`.
- Executar `GET /matches/{id}`.
- Conferir dados resolvidos de `homeTeam` e `awayTeam`.
- Conferir resultado quando houver.
- Conferir `isBettingOpen` com base em UTC.
- Conferir ausencia de N+1 em leitura principal, por inspecao de query/logs quando possivel.

**Fora do escopo:**

- Criar ou editar palpites.
- Cadastrar resultado.
- Calcular pontuacao.
- Criar frontend.
- Alterar seed/migration fora de ajuste estritamente necessario para teste local.

**Criterios de aceite:**

- `GET /matches` retorna lista ordenada por data.
- Filtros por `Stage` e `Status` funcionam.
- `GET /matches/{id}` retorna detalhe com selecoes resolvidas.
- Partida inexistente retorna `404`.
- `isBettingOpen` reflete `AllowBetUntil` vs. agora UTC.
- Pendencias foram registradas objetivamente, se houver.

**Prompt sugerido:**

```text
Valide somente a Tarefa 06. Rode build e testes, suba a API, execute GET /matches, confira ordenacao por MatchDate, filtros Stage/Status, GET /matches/{id}, dados resolvidos de homeTeam/awayTeam, resultado quando houver, isBettingOpen com base em UTC e ausencia de N+1 por inspecao quando possivel. Se algo falhar, registre pendencia objetivamente. Nao implemente palpites, cadastro de resultado, pontuacao, frontend ou features fora de Matches.
```

## Mapeamento do escopo original para as etapas

| Item da Tarefa 06 | Etapa responsavel |
|---|---|
| Validar dependencias T02/T03 | Etapa 01 |
| DTOs de listagem e detalhe | Etapa 02 |
| Filtros opcionais `Stage` e `Status` | Etapas 03, 06, 08 e 10 |
| Ordenacao por `MatchDate` | Etapas 06, 11 e 14 |
| Dados das selecoes resolvidos | Etapas 02, 06, 07, 12 e 14 |
| `isBettingOpen` por UTC | Etapas 04, 05, 06, 07, 11, 12 e 14 |
| Query eficiente sem N+1 | Etapas 06, 07 e 14 |
| `GET /matches` | Etapa 08 |
| `GET /matches/{id}` | Etapa 09 |
| Filtro invalido retorna erro claro | Etapa 10 |
| Testes de Application | Etapa 11 |
| Testes de API | Etapa 12 |
| Documentacao | Etapa 13 |
| Validacao dos criterios de aceite originais | Etapa 14 |

