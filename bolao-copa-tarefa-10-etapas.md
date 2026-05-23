# Tarefa 10 - Divisao em etapas independentes

Fonte: `C:\Repo\bolao-copa-backlog.md`, secao **Tarefa 10 - Endpoint de ranking com desempate**.

Este arquivo quebra a Tarefa 10 em etapas pequenas, fechadas e implementaveis separadamente. Nenhuma etapa abaixo deve implementar WebSocket/push, snapshots persistidos, ranking persistido, frontend, estatisticas individuais, recalculo de pontos ou regras novas de pontuacao.

## Escopo original da Tarefa 10

Expor ranking global ordenado:

- `GET /ranking`.
- Somar `PointsEarned` por usuario.
- Ordenar por total de pontos.
- Aplicar desempate em cascata:
  - mais placares exatos;
  - mais acertos de vencedor;
  - melhor sequencia de acertos consecutivos;
  - primeiro a cadastrar palpites.
- Retornar posicao, usuario, nome, pontos e flags:
  - `isTop3`;
  - `isCurrentUser`.
- Atualizacao compativel com free tier:
  - recalculo on-demand;
  - cache curto opcional.

## Dependencia base

A Tarefa 10 depende das Tarefas 07 e 09.

Na pratica, para `isCurrentUser`, tambem pode depender da autenticacao da Tarefa 04.

Cada etapa abaixo deve ser independente das outras etapas deste arquivo, mas pode exigir que exista:

- entidade `User`;
- entidade `Bet` com `PointsEarned`, `CreatedAt`, gols previstos e `UserId`;
- entidade `Match` com resultado, `MatchDate`, `Stage` e `Status`;
- pontos ja recalculados pela Tarefa 09;
- regra de pontuacao/classificacao da Tarefa 07;
- forma de obter usuario atual do JWT, se `isCurrentUser` for suportado para usuario logado.

Se esse contrato nao existir, a etapa deve registrar bloqueio objetivo e nao deve implementar T04, T07 ou T09 dentro da Tarefa 10.

## Decisoes assumidas para a Tarefa 10

- Ranking sera recalculado on-demand.
- Cache em memoria curto e opcional, apenas se nao complicar a implementacao.
- Ranking considera apenas palpites de partidas avaliadas, ou seja, partidas com resultado completo e `PointsEarned` definido.
- `totalPoints` e a soma de `Bet.PointsEarned`.
- "Placar exato" significa gols previstos iguais a `HomeGoals` e `AwayGoals` do resultado.
- "Acerto de vencedor" significa desfecho correto da partida: mandante venceu, visitante venceu ou empate. A definicao deve ser documentada porque se relaciona com a duvida 3.1.
- "Acerto" para sequencia sera assumido como `PointsEarned > 0`, conforme prompt original e duvida 3.2.
- Sequencia e calculada por data da partida (`MatchDate`) em ordem crescente.
- Primeiro a cadastrar significa menor `Bet.CreatedAt` entre os palpites avaliados do usuario.
- Se a requisicao for anonima ou nao houver usuario autenticado, `isCurrentUser` deve ser `false` para todos.
- Nao criar `RankingSnapshot` nesta tarefa.

## Regra de independencia

Cada etapa deve poder ser implementada sozinha, sem depender da execucao previa de outra etapa deste arquivo.

Para preservar essa independencia:

- A etapa deve inspecionar o estado atual antes de alterar arquivos.
- A etapa deve preservar contratos, queries e testes existentes quando forem compativeis.
- A etapa deve criar apenas o minimo necessario para seu proprio objetivo compilar.
- Se faltar pontos recalculados, registrar bloqueio e nao implementar a Tarefa 09.
- A etapa nao deve alterar `ScoreCalculator` sem necessidade explicita.
- A etapa nao deve persistir ranking, criar push/WebSocket ou criar frontend.
- Validacoes globais da Tarefa 10 so devem ser exigidas quando todas as etapas forem executadas.

## Etapa 01 - Pre-validacao das dependencias de ranking

**Objetivo:** Confirmar que existe base minima para calcular ranking global.

**Escopo:**

- Verificar se a solucao backend compila.
- Verificar se `User`, `Bet` e `Match` existem.
- Verificar se `Bet.PointsEarned` existe.
- Verificar se `Bet.CreatedAt` existe.
- Verificar se `Match` possui resultado e `MatchDate`.
- Verificar se a Tarefa 09 ja recalcula pontos.
- Verificar se ha forma de obter usuario atual, se `isCurrentUser` for necessario.

**Fora do escopo:**

- Criar endpoint de ranking.
- Criar recalculo de pontos.
- Criar snapshots.
- Criar cache.

**Criterios de aceite:**

- Dependencias minimas foram confirmadas.
- Bloqueios foram registrados objetivamente, se houver.
- Nenhuma query de ranking foi criada.

**Prompt sugerido:**

```text
Valide somente as dependencias da Tarefa 10. Confira build, entidades User/Bet/Match, Bet.PointsEarned, Bet.CreatedAt, Match com resultado e MatchDate, existencia do recalculo de pontos da Tarefa 09 e forma de obter usuario atual se isCurrentUser for necessario. Se faltar algo, registre bloqueio objetivamente. Nao crie endpoint de ranking, recalculo, snapshots ou cache.
```

## Etapa 02 - Decisoes de classificacao para desempates

**Objetivo:** Registrar as definicoes usadas para placar exato, acerto de vencedor e sequencia.

**Escopo:**

- Documentar como identificar placar exato.
- Documentar como identificar acerto de vencedor/empate.
- Documentar que "acerto" para sequencia sera `PointsEarned > 0`, salvo decisao futura.
- Registrar relacao com as duvidas 3.1 e 3.2.
- Definir se placar exato tambem conta como acerto de vencedor.

**Fora do escopo:**

- Implementar query de ranking.
- Alterar `ScoreCalculator`.
- Criar endpoint.
- Criar estatisticas individuais.

**Criterios de aceite:**

- Definicoes de desempate estao explicitas.
- Ambiguidades foram registradas.
- Nenhuma regra foi implementada ainda.

**Prompt sugerido:**

```text
Trate somente as decisoes de classificacao para desempates do ranking. Documente placar exato, acerto de vencedor/empate, acerto para sequencia como PointsEarned > 0 salvo decisao futura, relacao com as duvidas 3.1/3.2 e se placar exato tambem conta como acerto de vencedor. Nao implemente query, endpoint, ScoreCalculator ou estatisticas.
```

## Etapa 03 - DTOs e contrato de resposta do ranking

**Objetivo:** Definir o contrato publico de `GET /ranking`.

**Escopo:**

- Criar DTO de item do ranking.
- Incluir `position`.
- Incluir `userId`.
- Incluir `name`.
- Incluir `points`.
- Incluir `isTop3`.
- Incluir `isCurrentUser`.
- Incluir campos de desempate apenas se forem uteis para debug/contrato e aprovados.

**Fora do escopo:**

- Criar endpoint.
- Criar query.
- Criar cache.
- Criar frontend.

**Criterios de aceite:**

- DTO de ranking existe em camada apropriada.
- Campos obrigatorios do backlog estao presentes.
- DTO nao expoe dados sensiveis do usuario.
- A solucao compila.

**Prompt sugerido:**

```text
Crie somente os DTOs/contratos de resposta do ranking. O item deve conter position, userId, name, points, isTop3 e isCurrentUser. Campos de desempate so devem entrar se forem realmente parte do contrato aprovado. Nao crie endpoint, query, cache ou frontend.
```

## Etapa 04 - Leitura do usuario atual para isCurrentUser

**Objetivo:** Permitir marcar o usuario logado no ranking.

**Escopo:**

- Reutilizar ou criar leitura opcional do `userId` autenticado.
- Permitir requisicao anonima, se o endpoint for publico.
- Garantir que ausencia de usuario nao gere erro no ranking.
- Retornar `isCurrentUser = false` quando nao houver usuario.

**Fora do escopo:**

- Implementar login.
- Tornar ranking obrigatoriamente autenticado sem decisao.
- Criar roles/admin.
- Criar endpoint.

**Criterios de aceite:**

- `userId` atual pode ser obtido quando houver token.
- Ranking pode funcionar sem usuario autenticado, se esse for o contrato.
- A solucao compila.

**Prompt sugerido:**

```text
Crie ou reutilize somente a leitura opcional do userId autenticado para marcar isCurrentUser no ranking. Se nao houver token, o ranking deve poder retornar com isCurrentUser=false para todos, salvo decisao de endpoint autenticado. Nao implemente login, roles, admin ou endpoint de ranking.
```

## Etapa 05 - Query base de dados avaliados para ranking

**Objetivo:** Projetar os dados necessarios para calcular ranking sem N+1.

**Escopo:**

- Ler `Users`, `Bets` e `Matches` necessarios.
- Considerar apenas partidas com resultado completo.
- Trazer `PointsEarned`, gols previstos, resultado da partida, `MatchDate` e `Bet.CreatedAt`.
- Usar projecao eficiente e read-only.
- Evitar N+1.

**Fora do escopo:**

- Ordenar ranking final.
- Calcular desempates.
- Criar endpoint HTTP.
- Persistir snapshot.

**Criterios de aceite:**

- Query base fornece todos os dados necessarios.
- Partidas sem resultado completo nao entram no calculo.
- Query evita N+1.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente a query base de dados avaliados para ranking. Leia Users, Bets e Matches, considerando apenas partidas com resultado completo, trazendo PointsEarned, gols previstos, resultado, MatchDate e Bet.CreatedAt. Use projecao read-only eficiente e evite N+1. Nao ordene ranking final, nao calcule desempates, nao crie endpoint ou snapshot.
```

## Etapa 06 - Agregacao de pontos por usuario

**Objetivo:** Somar `PointsEarned` por usuario.

**Escopo:**

- Agrupar palpites avaliados por usuario.
- Somar `PointsEarned`.
- Retornar estrutura intermediaria com usuario e total de pontos.
- Tratar usuario sem palpite avaliado conforme decisao do contrato.

**Fora do escopo:**

- Calcular desempates.
- Atribuir posicao.
- Criar endpoint.
- Criar cache.

**Criterios de aceite:**

- Total de pontos por usuario esta correto.
- Soma usa `PointsEarned`, sem recalcular pontuacao.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente a agregacao de pontos do ranking. Agrupe palpites avaliados por usuario e some Bet.PointsEarned, retornando estrutura intermediaria com usuario e total. Nao recalcule pontuacao, nao calcule desempates, nao atribua posicao, nao crie endpoint ou cache.
```

## Etapa 07 - Calculo de placares exatos por usuario

**Objetivo:** Calcular o primeiro criterio de desempate.

**Escopo:**

- Identificar palpites com placar exato.
- Agrupar contagem por usuario.
- Usar apenas partidas com resultado completo.
- Reutilizar helper de classificacao, se existir.

**Fora do escopo:**

- Calcular pontos.
- Calcular acertos de vencedor.
- Calcular sequencia.
- Criar endpoint.

**Criterios de aceite:**

- Contagem de placares exatos por usuario esta correta.
- Resultado incompleto nao entra.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente o calculo de placares exatos por usuario para desempate do ranking. Um placar exato ocorre quando gols previstos iguais aos gols finais da partida. Use apenas partidas com resultado completo e reutilize helper de classificacao se existir. Nao calcule pontos, acertos de vencedor, sequencia ou endpoint.
```

## Etapa 08 - Calculo de acertos de vencedor por usuario

**Objetivo:** Calcular o segundo criterio de desempate.

**Escopo:**

- Identificar acerto do desfecho da partida:
  - vitoria mandante;
  - vitoria visitante;
  - empate.
- Agrupar contagem por usuario.
- Usar apenas partidas com resultado completo.
- Documentar se placar exato tambem conta como acerto de vencedor.

**Fora do escopo:**

- Alterar regra de pontuacao.
- Calcular sequencia.
- Criar endpoint.
- Criar estatisticas individuais.

**Criterios de aceite:**

- Contagem de acertos de vencedor/empate por usuario esta correta.
- Definicao usada esta documentada.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente o calculo de acertos de vencedor/empate por usuario para desempate do ranking. Compare o desfecho previsto com o resultado final (mandante, visitante ou empate), use apenas partidas com resultado completo e documente se placar exato tambem conta como acerto de vencedor. Nao altere pontuacao, nao calcule sequencia, nao crie endpoint ou estatisticas.
```

## Etapa 09 - Calculo de melhor sequencia de acertos

**Objetivo:** Calcular o terceiro criterio de desempate.

**Escopo:**

- Ordenar palpites avaliados do usuario por `MatchDate`.
- Considerar "acerto" como `PointsEarned > 0`, salvo decisao futura.
- Calcular maior sequencia consecutiva de acertos.
- Quebrar sequencia quando `PointsEarned = 0`.
- Documentar a relacao com a duvida 3.2.

**Fora do escopo:**

- Calcular pontos.
- Alterar `ScoreCalculator`.
- Criar endpoint.
- Criar estatisticas individuais.

**Criterios de aceite:**

- Melhor sequencia por usuario esta correta.
- Ordem usa `MatchDate`.
- Definicao de acerto esta documentada.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente o calculo de melhor sequencia de acertos para ranking. Para cada usuario, ordene palpites avaliados por MatchDate, considere acerto como PointsEarned > 0 salvo decisao futura, incremente sequencia em acertos e quebre quando PointsEarned = 0. Documente a duvida 3.2. Nao recalcule pontos, nao altere ScoreCalculator, nao crie endpoint ou estatisticas.
```

## Etapa 10 - Calculo do primeiro palpite cadastrado

**Objetivo:** Calcular o quarto criterio de desempate.

**Escopo:**

- Obter o menor `Bet.CreatedAt` por usuario.
- Usar apenas palpites avaliados ou todos os palpites, conforme decisao documentada.
- Definir comportamento para usuario sem palpite avaliado.

**Fora do escopo:**

- Alterar `CreatedAt`.
- Criar migration.
- Criar endpoint.
- Criar ranking persistido.

**Criterios de aceite:**

- Primeiro palpite por usuario esta correto.
- Criterio esta documentado.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente o calculo do primeiro palpite cadastrado para desempate. Obtenha o menor Bet.CreatedAt por usuario e documente se a base sao palpites avaliados ou todos os palpites. Defina comportamento para usuario sem palpite avaliado. Nao altere CreatedAt, nao crie migration, endpoint ou ranking persistido.
```

## Etapa 11 - Ordenacao e atribuicao de posicao

**Objetivo:** Aplicar a ordenacao completa do ranking.

**Escopo:**

- Ordenar por total de pontos desc.
- Desempatar por placares exatos desc.
- Desempatar por acertos de vencedor desc.
- Desempatar por melhor sequencia desc.
- Desempatar por primeiro palpite asc.
- Atribuir `position` sequencial.

**Fora do escopo:**

- Criar endpoint HTTP.
- Criar cache.
- Criar frontend.
- Persistir snapshot.

**Criterios de aceite:**

- Ordenacao por pontos esta correta.
- Desempates seguem a ordem definida.
- Posicoes sao atribuidas corretamente.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente a ordenacao e atribuicao de posicao do ranking. Ordene por total de pontos desc, placares exatos desc, acertos de vencedor desc, melhor sequencia desc e primeiro palpite asc, depois atribua position sequencial. Nao crie endpoint, cache, frontend ou snapshot.
```

## Etapa 12 - Flags isTop3 e isCurrentUser

**Objetivo:** Marcar destaques esperados na resposta.

**Escopo:**

- Definir `isTop3` para posicoes 1 a 3.
- Definir `isCurrentUser` comparando `userId` do item com usuario autenticado.
- Tratar requisicao anonima sem erro.
- Manter flags no DTO final.

**Fora do escopo:**

- Alterar autenticacao.
- Criar endpoint.
- Criar frontend.
- Criar regras visuais.

**Criterios de aceite:**

- Top 3 fica sinalizado corretamente.
- Usuario autenticado fica sinalizado corretamente.
- Sem usuario autenticado, nenhuma linha fica como atual.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente as flags do ranking. isTop3 deve ser verdadeiro para posicoes 1 a 3. isCurrentUser deve comparar o userId do item com o usuario autenticado, sem falhar em requisicao anonima. Nao altere autenticacao, nao crie endpoint, frontend ou regras visuais.
```

## Etapa 13 - Query/caso de uso de ranking no Application

**Objetivo:** Consolidar o calculo do ranking na camada `Application`.

**Escopo:**

- Criar query/caso de uso `GetRanking`.
- Usar a query base e calculos de desempate.
- Retornar DTO final.
- Manter logica fora do controller.
- Manter recalculo on-demand.

**Fora do escopo:**

- Criar endpoint HTTP.
- Criar WebSocket/push.
- Criar snapshot persistido.
- Criar estatisticas individuais.

**Criterios de aceite:**

- Caso de uso retorna ranking completo.
- Logica principal fica em `Application`.
- Nao ha persistencia de ranking.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente o caso de uso GetRanking na camada Application. Ele deve consolidar dados, pontos, desempates, posicoes e flags, retornando DTO final e recalculando on-demand. Mantenha a logica fora do controller. Nao crie endpoint HTTP, WebSocket/push, snapshot persistido ou estatisticas individuais.
```

## Etapa 14 - Endpoint GET /ranking

**Objetivo:** Expor o ranking global via HTTP.

**Escopo:**

- Criar endpoint `GET /ranking`.
- Permitir usuario autenticado opcional para `isCurrentUser`, ou exigir autenticacao se esse for o padrao decidido.
- Chamar caso de uso de ranking.
- Retornar `200` com lista ordenada.
- Garantir contrato de resposta.

**Fora do escopo:**

- Criar WebSocket/push.
- Criar snapshots.
- Criar endpoint de estatisticas.
- Criar frontend.

**Criterios de aceite:**

- `GET /ranking` retorna ranking ordenado.
- Resposta inclui posicao, nome, pontos, `isTop3` e `isCurrentUser`.
- Endpoint nao recalcula `PointsEarned`.
- A solucao compila.

**Prompt sugerido:**

```text
Crie somente o endpoint GET /ranking. Ele deve chamar o caso de uso de ranking, retornar 200 com lista ordenada e incluir position, userId, name, points, isTop3 e isCurrentUser. Use usuario autenticado opcional para isCurrentUser, salvo padrao contrario ja decidido. Nao crie WebSocket/push, snapshots, estatisticas, frontend ou recalculo de PointsEarned.
```

## Etapa 15 - Cache curto opcional de ranking

**Objetivo:** Avaliar e, se fizer sentido, adicionar cache compativel com free tier.

**Escopo:**

- Avaliar custo da query on-demand.
- Adicionar cache em memoria curto apenas se necessario.
- Definir TTL pequeno.
- Invalidacao pode ser por expiracao de tempo.
- Garantir que cache nao persiste snapshot.

**Fora do escopo:**

- Redis.
- Banco de snapshot.
- WebSocket/push.
- Jobs externos.
- Cache complexo por usuario, salvo `isCurrentUser` tratado corretamente.

**Criterios de aceite:**

- Decisao sobre cache foi registrada.
- Se implementado, cache tem TTL curto.
- `isCurrentUser` continua correto.
- Nenhum snapshot persistido foi criado.

**Prompt sugerido:**

```text
Avalie somente cache curto para o ranking. Se a query on-demand for suficiente, registre a decisao sem implementar cache. Se implementar, use cache em memoria com TTL pequeno e garanta que isCurrentUser continue correto. Nao use Redis, snapshots persistidos, WebSocket/push, jobs externos ou cache complexo.
```

## Etapa 16 - Tratamento de erros do ranking

**Objetivo:** Padronizar respostas de erro do endpoint de ranking.

**Escopo:**

- Tratar falhas inesperadas da query.
- Tratar ausencia de dependencias de dados de forma segura.
- Usar `ProblemDetails` se a Tarefa 05 existir.
- Garantir que dados sensiveis nao aparecem em erros.

**Fora do escopo:**

- Criar middleware global se ainda nao existir.
- Criar regra de negocio nova.
- Criar fallback de snapshot.
- Criar push.

**Criterios de aceite:**

- Erros retornam formato consistente.
- Stack trace nao e exposto em producao.
- A solucao compila.

**Prompt sugerido:**

```text
Padronize somente os erros do ranking. Trate falhas inesperadas da query e ausencia de dados de forma segura, usando ProblemDetails se ja existir, sem expor dados sensiveis ou stack trace em producao. Nao crie middleware global novo, regra de negocio nova, fallback de snapshot ou push.
```

## Etapa 17 - Testes dos calculos de desempate

**Objetivo:** Cobrir os criterios de ranking sem depender de HTTP.

**Escopo:**

- Testar soma de pontos.
- Testar desempate por placares exatos.
- Testar desempate por acertos de vencedor.
- Testar desempate por melhor sequencia.
- Testar desempate por primeiro palpite.
- Testar combinacao em cascata.

**Fora do escopo:**

- Testar endpoint HTTP.
- Testar frontend.
- Testar WebSocket.
- Testar snapshots.

**Criterios de aceite:**

- Todos os desempates principais estao cobertos.
- Testes documentam as decisoes das duvidas 3.1/3.2.
- Testes passam.

**Prompt sugerido:**

```text
Crie somente testes dos calculos de ranking sem HTTP. Cubra soma de pontos, desempate por placares exatos, acertos de vencedor, melhor sequencia, primeiro palpite e combinacao em cascata. Documente nos testes as decisoes ligadas as duvidas 3.1/3.2. Nao teste endpoint, frontend, WebSocket ou snapshots.
```

## Etapa 18 - Testes de API do ranking

**Objetivo:** Validar o comportamento HTTP de `GET /ranking`.

**Escopo:**

- Testar `GET /ranking` retornando `200`.
- Testar ordenacao por pontos.
- Testar desempate em cascata com dados controlados.
- Testar `isTop3`.
- Testar `isCurrentUser` com usuario autenticado.
- Testar comportamento sem usuario autenticado, se endpoint for publico.

**Fora do escopo:**

- Testar frontend.
- Testar push/WebSocket.
- Testar snapshots persistidos.
- Testar estatisticas individuais.

**Criterios de aceite:**

- Endpoint retorna contrato correto.
- Ordenacao e flags estao corretas.
- Testes passam.

**Prompt sugerido:**

```text
Crie somente testes de API para GET /ranking. Cubra 200, ordenacao por pontos, desempate em cascata com dados controlados, isTop3, isCurrentUser com usuario autenticado e comportamento sem usuario autenticado se o endpoint for publico. Nao teste frontend, push/WebSocket, snapshots ou estatisticas individuais.
```

## Etapa 19 - Documentacao local do ranking

**Objetivo:** Documentar contrato, ordenacao e limites do endpoint.

**Escopo:**

- Atualizar README ou documentacao tecnica.
- Documentar `GET /ranking`.
- Documentar campos de resposta.
- Documentar ordem de desempate.
- Documentar definicoes assumidas para duvidas 3.1/3.2.
- Documentar estrategia on-demand e cache curto opcional, se existir.
- Declarar que WebSocket/push e snapshot persistido estao fora desta tarefa.

**Fora do escopo:**

- Documentar ranking persistido como pronto.
- Documentar frontend.
- Documentar estatisticas individuais.
- Documentar push.

**Criterios de aceite:**

- Documentacao permite testar ranking localmente.
- Desempates estao claros.
- Limites da tarefa estao claros.

**Prompt sugerido:**

```text
Atualize somente a documentacao local do ranking. Documente GET /ranking, campos de resposta, ordem de desempate, definicoes assumidas das duvidas 3.1/3.2, estrategia on-demand e cache curto se existir. Deixe claro que WebSocket/push, RankingSnapshot, frontend e estatisticas individuais ficam fora desta tarefa.
```

## Etapa 20 - Validacao final da Tarefa 10

**Objetivo:** Verificar se o endpoint de ranking atende aos criterios originais.

**Escopo:**

- Rodar build e testes.
- Subir a API.
- Garantir dados com pontos recalculados.
- Chamar `GET /ranking`.
- Conferir ordenacao por pontos.
- Conferir desempates em cascata.
- Conferir posicoes.
- Conferir `isTop3`.
- Conferir `isCurrentUser`.
- Conferir que ranking e recalculado on-demand ou usa cache curto documentado.
- Conferir que nao ha WebSocket/push nem snapshot persistido.

**Fora do escopo:**

- Criar ranking persistido.
- Criar WebSocket/push.
- Criar frontend.
- Criar estatisticas individuais.
- Recalcular `PointsEarned`.

**Criterios de aceite:**

- Ordenacao por pontos esta correta.
- Empates sao resolvidos na ordem definida.
- Posicao do usuario logado e Top 3 estao sinalizados na resposta.
- Endpoint nao cria snapshots persistidos.
- Pendencias foram registradas objetivamente, se houver.

**Prompt sugerido:**

```text
Valide somente a Tarefa 10. Rode build e testes, suba a API, garanta dados com PointsEarned ja recalculado, chame GET /ranking, confira ordenacao por pontos, desempates em cascata, posicoes, isTop3, isCurrentUser, estrategia on-demand/cache curto documentada e ausencia de WebSocket/push ou snapshot persistido. Se algo falhar, registre pendencia objetivamente. Nao implemente ranking persistido, push, frontend, estatisticas individuais ou recalculo de PointsEarned.
```

## Mapeamento do escopo original para as etapas

| Item da Tarefa 10 | Etapa responsavel |
|---|---|
| Validar dependencias T07/T09 | Etapa 01 |
| Decisoes das duvidas 3.1/3.2 | Etapa 02 |
| DTO de ranking | Etapa 03 |
| `isCurrentUser` | Etapas 04, 12, 14, 18 e 20 |
| Query base eficiente | Etapa 05 |
| Somar `PointsEarned` por usuario | Etapa 06 |
| Desempate por placares exatos | Etapa 07 |
| Desempate por acertos de vencedor | Etapa 08 |
| Desempate por melhor sequencia | Etapa 09 |
| Desempate por primeiro palpite | Etapa 10 |
| Ordenacao completa e posicao | Etapa 11 |
| `isTop3` | Etapas 12, 18 e 20 |
| Caso de uso em `Application` | Etapa 13 |
| `GET /ranking` | Etapa 14 |
| Estrategia free tier on-demand/cache curto | Etapa 15 |
| Erros claros | Etapa 16 |
| Testes de desempate | Etapa 17 |
| Testes de API | Etapa 18 |
| Documentacao | Etapa 19 |
| Validacao dos criterios de aceite originais | Etapa 20 |

