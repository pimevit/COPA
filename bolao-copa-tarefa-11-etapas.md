# Tarefa 11 - Divisao em etapas independentes

Fonte: `C:\Repo\bolao-copa-backlog.md`, secao **Tarefa 11 - Endpoint de estatisticas do usuario**.

Este arquivo quebra a Tarefa 11 em etapas pequenas, fechadas e implementaveis separadamente. Nenhuma etapa abaixo deve implementar estatisticas comparativas/globais, ranking, frontend, recálculo de pontos, lancamento de resultado, endpoints de bets ou regras novas de pontuacao.

## Escopo original da Tarefa 11

Fornecer estatisticas individuais do usuario autenticado:

- `GET /stats/me`.
- Total de pontos.
- Numero de placares exatos.
- Numero de acertos de vencedor.
- Percentual de acertos.
- Melhor sequencia de acertos.
- Referencia ao historico ja exposto por `GET /bets/me`.
- Definir "acerto" e sequencia conforme decisoes das duvidas 3.1 e 3.2.

## Dependencia base

A Tarefa 11 depende das Tarefas 07 e 08.

Na pratica, tambem depende da autenticacao da Tarefa 04, porque o endpoint e do usuario logado.

Cada etapa abaixo deve ser independente das outras etapas deste arquivo, mas pode exigir que exista:

- autenticacao JWT funcionando;
- forma confiavel de obter `userId` do usuario logado;
- entidade `Bet` com `UserId`, `PointsEarned`, gols previstos e `CreatedAt`;
- entidade `Match` com resultado, `MatchDate`, `Stage` e `Status`;
- `GET /bets/me` ou caso de uso equivalente de historico;
- pontos ja calculados/recalculados em `Bet.PointsEarned`;
- logica de classificacao de acerto da Tarefa 07, ou helper reutilizavel equivalente.

Se esse contrato nao existir, a etapa deve registrar bloqueio objetivo e nao deve implementar T04, T07 ou T08 dentro da Tarefa 11.

## Decisoes assumidas para a Tarefa 11

- `GET /stats/me` exige usuario autenticado.
- O `userId` vem do JWT, nunca de parametro de rota, body ou query string.
- Estatisticas consideram apenas palpites do usuario autenticado.
- Palpites avaliados sao aqueles ligados a partidas com resultado completo.
- `totalPoints` e a soma de `Bet.PointsEarned` dos palpites avaliados.
- "Placar exato" significa gols previstos iguais aos gols finais da partida.
- "Acerto de vencedor" significa desfecho correto da partida: mandante venceu, visitante venceu ou empate.
- Placar exato tambem conta como acerto de vencedor, salvo decisao futura contraria.
- "Acerto" para percentual e sequencia sera assumido como `PointsEarned > 0`, conforme prompt original e duvida 3.2.
- Percentual de acertos = acertos / total de palpites avaliados.
- Se nao houver palpites avaliados, percentual deve retornar `0` para evitar divisao por zero.
- Melhor sequencia usa partidas ordenadas por `MatchDate` crescente; em empate de data, usar criterio estavel como `Match.Id`.
- Historico detalhado continua em `GET /bets/me`; `GET /stats/me` deve retornar apenas referencia curta a esse recurso.

## Regra de independencia

Cada etapa deve poder ser implementada sozinha, sem depender da execucao previa de outra etapa deste arquivo.

Para preservar essa independencia:

- A etapa deve inspecionar o estado atual antes de alterar arquivos.
- A etapa deve preservar contratos, queries e testes existentes quando forem compativeis.
- A etapa deve criar apenas o minimo necessario para seu proprio objetivo compilar.
- Se faltar autenticacao, dados de bets ou pontos calculados, registrar bloqueio e nao implementar tarefas dependentes.
- A etapa nao deve duplicar regra de pontuacao quando houver helper reutilizavel.
- A etapa nao deve criar estatisticas globais, ranking, frontend ou novos endpoints de historico.
- Validacoes globais da Tarefa 11 so devem ser exigidas quando todas as etapas forem executadas.

## Etapa 01 - Pre-validacao das dependencias de estatisticas

**Objetivo:** Confirmar que existe base minima para calcular estatisticas do usuario.

**Escopo:**

- Verificar se a solucao backend compila.
- Verificar se autenticacao JWT existe.
- Verificar se ha forma de obter `userId` do JWT.
- Verificar se `Bet` e `Match` possuem os campos necessarios.
- Verificar se `PointsEarned` ja e preenchido por tarefa anterior.
- Verificar se existe `GET /bets/me` ou caso de uso equivalente.

**Fora do escopo:**

- Criar endpoint de estatisticas.
- Criar endpoint de bets.
- Criar autenticacao.
- Recalcular pontos.

**Criterios de aceite:**

- Dependencias minimas foram confirmadas.
- Bloqueios foram registrados objetivamente, se houver.
- Nenhuma metrica de estatistica foi implementada.

**Prompt sugerido:**

```text
Valide somente as dependencias da Tarefa 11. Confira build, autenticacao JWT, forma de obter userId do JWT, Bet e Match com campos necessarios, PointsEarned preenchido por tarefa anterior e existencia de GET /bets/me ou caso de uso equivalente. Se faltar algo, registre bloqueio objetivamente. Nao crie endpoint de estatisticas, endpoint de bets, autenticacao ou recalculo de pontos.
```

## Etapa 02 - Decisoes de metricas e acerto

**Objetivo:** Registrar definicoes usadas pelas estatisticas individuais.

**Escopo:**

- Definir palpite avaliado.
- Definir placar exato.
- Definir acerto de vencedor.
- Definir acerto para percentual e sequencia.
- Definir comportamento quando nao houver palpites avaliados.
- Registrar relacao com as duvidas 3.1 e 3.2.

**Fora do escopo:**

- Implementar query.
- Alterar `ScoreCalculator`.
- Criar endpoint.
- Criar estatisticas globais.

**Criterios de aceite:**

- Definicoes estao explicitas.
- Ambiguidades foram registradas.
- Nenhuma regra foi implementada ainda.

**Prompt sugerido:**

```text
Trate somente as decisoes de metricas da Tarefa 11. Documente o que e palpite avaliado, placar exato, acerto de vencedor, acerto para percentual/sequencia, comportamento sem palpites avaliados e relacao com as duvidas 3.1/3.2. Nao implemente query, endpoint, ScoreCalculator ou estatisticas globais.
```

## Etapa 03 - DTO de resposta de GET /stats/me

**Objetivo:** Definir o contrato publico das estatisticas do usuario.

**Escopo:**

- Criar DTO de resposta de estatisticas.
- Incluir `totalPoints`.
- Incluir `exactScoreCount`.
- Incluir `winnerHitCount`.
- Incluir `hitPercentage`.
- Incluir `bestHitStreak`.
- Incluir referencia curta ao historico, como `historyEndpoint` ou campo equivalente.

**Fora do escopo:**

- Criar endpoint.
- Implementar calculos.
- Criar DTO de historico completo.
- Criar frontend.

**Criterios de aceite:**

- DTO existe em camada apropriada.
- Campos do backlog estao presentes.
- DTO nao expoe dados de outros usuarios.
- A solucao compila.

**Prompt sugerido:**

```text
Crie somente o DTO de resposta de GET /stats/me. Inclua totalPoints, exactScoreCount, winnerHitCount, hitPercentage, bestHitStreak e uma referencia curta ao historico, como historyEndpoint = /bets/me ou equivalente. Nao crie endpoint, calculos, DTO de historico completo ou frontend.
```

## Etapa 04 - Leitura do usuario autenticado

**Objetivo:** Garantir que estatisticas sejam calculadas para o usuario logado.

**Escopo:**

- Reutilizar ou criar servico para obter `userId` do JWT.
- Garantir que endpoint/caso de uso nao aceite `userId` externo.
- Definir erro para usuario nao autenticado.

**Fora do escopo:**

- Implementar login.
- Criar roles/admin.
- Criar endpoint de estatisticas.
- Criar estatisticas de outro usuario.

**Criterios de aceite:**

- `userId` vem do JWT.
- Usuario nao autenticado tem tratamento claro.
- Nao existe parametro externo de usuario para `stats/me`.
- A solucao compila.

**Prompt sugerido:**

```text
Crie ou reutilize somente a leitura do userId autenticado para Stats. O userId deve vir do JWT, nunca de rota, body ou query string. Defina erro claro para usuario nao autenticado. Nao implemente login, roles/admin, endpoint de estatisticas ou estatisticas de outro usuario.
```

## Etapa 05 - Query base de palpites avaliados do usuario

**Objetivo:** Projetar os dados necessarios para calcular estatisticas sem N+1.

**Escopo:**

- Buscar apenas `Bet` do usuario autenticado.
- Juntar dados de `Match`.
- Considerar apenas partidas com resultado completo.
- Trazer `PointsEarned`, gols previstos, resultado, `MatchDate` e `Match.Id`.
- Usar leitura read-only.
- Evitar N+1.

**Fora do escopo:**

- Calcular todas as metricas.
- Criar endpoint.
- Expor palpites de outros usuarios.
- Recalcular pontos.

**Criterios de aceite:**

- Query base retorna apenas palpites avaliados do usuario.
- Dados necessarios para metricas estao presentes.
- Query evita N+1.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente a query base de palpites avaliados do usuario para Stats. Busque apenas Bets do userId autenticado, junte Match, considere somente partidas com resultado completo e traga PointsEarned, gols previstos, resultado, MatchDate e Match.Id. Use leitura read-only e evite N+1. Nao calcule todas as metricas, nao crie endpoint, nao exponha outros usuarios e nao recalcule pontos.
```

## Etapa 06 - Classificador reutilizavel de acertos

**Objetivo:** Evitar duplicacao da logica de classificacao de palpites.

**Escopo:**

- Criar ou reutilizar helper para classificar:
  - placar exato;
  - acerto de vencedor/empate;
  - acerto geral (`PointsEarned > 0`).
- Manter helper sem dependencia de HTTP.
- Reutilizar regra da Tarefa 07 quando existir.

**Fora do escopo:**

- Alterar pontuacao.
- Criar endpoint.
- Criar ranking.
- Criar estatisticas globais.

**Criterios de aceite:**

- Classificacao e reutilizavel por estatisticas.
- Regra nao duplica implementacao conflitante.
- A solucao compila.

**Prompt sugerido:**

```text
Crie ou reutilize somente um helper de classificacao de palpites para Stats. Ele deve identificar placar exato, acerto de vencedor/empate e acerto geral como PointsEarned > 0, reaproveitando a logica da Tarefa 07 quando existir. Nao altere pontuacao, nao crie endpoint, ranking ou estatisticas globais.
```

## Etapa 07 - Calculo de total de pontos

**Objetivo:** Somar pontos do usuario autenticado.

**Escopo:**

- Somar `PointsEarned` dos palpites avaliados do usuario.
- Retornar zero quando nao houver palpites avaliados.
- Nao recalcular pontuacao.

**Fora do escopo:**

- Calcular demais metricas.
- Criar endpoint.
- Alterar `PointsEarned`.
- Criar ranking.

**Criterios de aceite:**

- Total de pontos esta correto.
- Usuario sem palpites avaliados retorna zero.
- Soma usa `PointsEarned` existente.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente o calculo de totalPoints para Stats. Some Bet.PointsEarned dos palpites avaliados do usuario e retorne zero quando nao houver dados. Nao recalcule pontuacao, nao calcule demais metricas, nao crie endpoint e nao altere ranking.
```

## Etapa 08 - Calculo de placares exatos

**Objetivo:** Contar placares exatos do usuario autenticado.

**Escopo:**

- Comparar gols previstos com gols finais da partida.
- Contar apenas palpites avaliados do usuario.
- Reutilizar helper de classificacao, se existir.

**Fora do escopo:**

- Calcular pontos.
- Calcular percentual.
- Criar endpoint.
- Criar estatisticas globais.

**Criterios de aceite:**

- `exactScoreCount` esta correto.
- Partidas sem resultado nao entram.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente o calculo de exactScoreCount para Stats. Compare gols previstos com gols finais, considerando apenas palpites avaliados do usuario e reutilizando helper de classificacao se existir. Nao calcule pontos, percentual, endpoint ou estatisticas globais.
```

## Etapa 09 - Calculo de acertos de vencedor

**Objetivo:** Contar acertos de desfecho do usuario autenticado.

**Escopo:**

- Comparar desfecho previsto com desfecho real:
  - mandante venceu;
  - visitante venceu;
  - empate.
- Contar apenas palpites avaliados do usuario.
- Documentar se placar exato tambem conta como acerto de vencedor.

**Fora do escopo:**

- Alterar regra de pontuacao.
- Calcular ranking.
- Criar endpoint.
- Criar estatisticas globais.

**Criterios de aceite:**

- `winnerHitCount` esta correto.
- Definicao usada esta documentada.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente o calculo de winnerHitCount para Stats. Compare desfecho previsto com desfecho real (mandante, visitante ou empate), considerando apenas palpites avaliados do usuario, e documente se placar exato tambem conta como acerto de vencedor. Nao altere pontuacao, ranking, endpoint ou estatisticas globais.
```

## Etapa 10 - Calculo de percentual de acertos

**Objetivo:** Calcular percentual de acerto do usuario.

**Escopo:**

- Contar palpites avaliados do usuario.
- Contar acertos com a definicao `PointsEarned > 0`.
- Calcular `hitPercentage = hits / evaluatedBets`.
- Retornar `0` quando nao houver palpites avaliados.
- Definir formato do numero, como decimal entre 0 e 1 ou percentual 0 a 100.

**Fora do escopo:**

- Criar grafico.
- Criar endpoint global.
- Alterar regra de pontuacao.
- Criar frontend.

**Criterios de aceite:**

- Percentual e consistente com a definicao adotada.
- Divisao por zero e evitada.
- Formato do campo esta documentado.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente o calculo de hitPercentage para Stats. Conte palpites avaliados do usuario, considere acerto como PointsEarned > 0, calcule hits / evaluatedBets e retorne 0 quando nao houver avaliados. Documente se o formato sera decimal 0..1 ou percentual 0..100. Nao crie grafico, endpoint global, frontend ou altere pontuacao.
```

## Etapa 11 - Calculo de melhor sequencia de acertos

**Objetivo:** Calcular a melhor sequencia de acertos consecutivos do usuario.

**Escopo:**

- Ordenar palpites avaliados por `MatchDate` crescente.
- Usar `Match.Id` como desempate estavel quando necessario.
- Considerar acerto como `PointsEarned > 0`.
- Quebrar sequencia quando `PointsEarned = 0`.
- Retornar zero quando nao houver palpites avaliados.

**Fora do escopo:**

- Calcular sequencia global.
- Alterar pontuacao.
- Criar ranking.
- Criar frontend.

**Criterios de aceite:**

- `bestHitStreak` esta correto.
- Ordem usa data da partida.
- Definicao de acerto esta documentada.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente o calculo de bestHitStreak para Stats. Ordene palpites avaliados por MatchDate crescente e Match.Id como desempate, considere acerto como PointsEarned > 0, quebre a sequencia em PointsEarned = 0 e retorne zero sem dados. Nao calcule sequencia global, nao altere pontuacao, ranking ou frontend.
```

## Etapa 12 - Referencia ao historico de palpites

**Objetivo:** Incluir no retorno uma referencia curta ao historico existente.

**Escopo:**

- Adicionar referencia para `GET /bets/me` no DTO de stats.
- Evitar duplicar historico completo dentro de `GET /stats/me`.
- Documentar que detalhes continuam no endpoint de bets.

**Fora do escopo:**

- Criar novo endpoint de historico.
- Alterar `GET /bets/me`.
- Expor palpites de outros usuarios.
- Criar frontend.

**Criterios de aceite:**

- Resposta de stats aponta para o historico.
- Historico completo nao e duplicado.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente a referencia ao historico na resposta de Stats. Inclua campo simples apontando para GET /bets/me ou equivalente e nao duplique o historico completo dentro de GET /stats/me. Nao crie novo endpoint de historico, nao altere GET /bets/me, nao exponha outros usuarios e nao crie frontend.
```

## Etapa 13 - Caso de uso GetMyStats no Application

**Objetivo:** Consolidar as metricas do usuario na camada `Application`.

**Escopo:**

- Criar caso de uso/query `GetMyStats`.
- Receber `userId` autenticado.
- Usar query base de palpites avaliados.
- Calcular total de pontos, placares exatos, acertos de vencedor, percentual e melhor sequencia.
- Retornar DTO final.
- Manter logica fora do controller.

**Fora do escopo:**

- Criar endpoint HTTP.
- Criar estatisticas globais.
- Criar ranking.
- Recalcular pontos.

**Criterios de aceite:**

- Caso de uso retorna todas as metricas da Tarefa 11.
- Logica principal fica em `Application`.
- Dados sao filtrados pelo usuario autenticado.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente o caso de uso GetMyStats na camada Application. Ele deve receber userId autenticado, usar a query de palpites avaliados do usuario, calcular totalPoints, exactScoreCount, winnerHitCount, hitPercentage e bestHitStreak, e retornar DTO final com referencia ao historico. Nao crie endpoint HTTP, estatisticas globais, ranking ou recalculo de pontos.
```

## Etapa 14 - Endpoint GET /stats/me

**Objetivo:** Expor estatisticas do usuario logado via HTTP.

**Escopo:**

- Criar endpoint `GET /stats/me`.
- Exigir autenticacao.
- Obter `userId` do JWT.
- Chamar caso de uso `GetMyStats`.
- Retornar `200` com DTO de estatisticas.

**Fora do escopo:**

- Criar endpoint de stats por outro usuario.
- Criar estatisticas globais.
- Criar ranking.
- Criar frontend.

**Criterios de aceite:**

- Endpoint exige token.
- Endpoint retorna apenas estatisticas do usuario logado.
- Resposta possui todas as metricas esperadas.
- A solucao compila.

**Prompt sugerido:**

```text
Crie somente o endpoint autenticado GET /stats/me. Ele deve obter userId do JWT, chamar GetMyStats e retornar 200 com totalPoints, exactScoreCount, winnerHitCount, hitPercentage, bestHitStreak e referencia ao historico. Nao crie stats por outro usuario, estatisticas globais, ranking ou frontend.
```

## Etapa 15 - Tratamento de erros de Stats

**Objetivo:** Padronizar respostas de erro do endpoint de estatisticas.

**Escopo:**

- Retornar `401` quando nao houver autenticacao.
- Tratar falhas inesperadas da query.
- Usar `ProblemDetails` se a Tarefa 05 existir.
- Evitar vazamento de dados sensiveis.

**Fora do escopo:**

- Criar middleware global se ainda nao existir.
- Criar rules de negocio novas.
- Criar endpoint global.
- Alterar autenticacao.

**Criterios de aceite:**

- Erros retornam status HTTP consistentes.
- Stack trace nao e exposto em producao.
- A solucao compila.

**Prompt sugerido:**

```text
Padronize somente os erros de GET /stats/me. Retorne 401 sem autenticacao, trate falhas inesperadas da query usando ProblemDetails se ja existir e nao exponha dados sensiveis ou stack trace em producao. Nao crie middleware global novo, regra de negocio nova, endpoint global ou altere autenticacao.
```

## Etapa 16 - Testes dos calculos de estatisticas

**Objetivo:** Cobrir as metricas sem depender de HTTP.

**Escopo:**

- Testar total de pontos.
- Testar placares exatos.
- Testar acertos de vencedor.
- Testar percentual de acertos.
- Testar melhor sequencia.
- Testar usuario sem palpites avaliados.
- Testar que palpites de outro usuario nao entram.

**Fora do escopo:**

- Testar endpoint HTTP.
- Testar ranking.
- Testar frontend.
- Testar estatisticas globais.

**Criterios de aceite:**

- Metricas principais estao cobertas.
- Testes documentam decisoes das duvidas 3.1/3.2.
- Testes passam.

**Prompt sugerido:**

```text
Crie somente testes dos calculos de Stats sem HTTP. Cubra totalPoints, exactScoreCount, winnerHitCount, hitPercentage, bestHitStreak, usuario sem palpites avaliados e exclusao de palpites de outro usuario. Documente nos testes as decisoes ligadas as duvidas 3.1/3.2. Nao teste endpoint HTTP, ranking, frontend ou estatisticas globais.
```

## Etapa 17 - Testes de API de GET /stats/me

**Objetivo:** Validar o comportamento HTTP do endpoint de estatisticas.

**Escopo:**

- Testar endpoint sem token retornando `401`.
- Testar usuario autenticado retornando `200`.
- Testar contrato de resposta.
- Testar que dados de outro usuario nao aparecem.
- Testar usuario sem palpites avaliados.

**Fora do escopo:**

- Testar ranking.
- Testar frontend.
- Testar endpoints de bets.
- Testar estatisticas globais.

**Criterios de aceite:**

- Endpoint exige autenticacao.
- Contrato de resposta esta correto.
- Dados sao filtrados pelo usuario autenticado.
- Testes passam.

**Prompt sugerido:**

```text
Crie somente testes de API para GET /stats/me. Cubra 401 sem token, 200 autenticado, contrato de resposta, ausencia de dados de outro usuario e usuario sem palpites avaliados. Nao teste ranking, frontend, endpoints de bets ou estatisticas globais.
```

## Etapa 18 - Documentacao local de estatisticas

**Objetivo:** Documentar como consumir e validar `GET /stats/me`.

**Escopo:**

- Atualizar README ou documentacao tecnica.
- Documentar autenticacao obrigatoria.
- Documentar campos de resposta.
- Documentar definicoes de acerto, percentual e sequencia.
- Documentar referencia ao historico `GET /bets/me`.
- Registrar que estatisticas globais ficam fora desta tarefa.

**Fora do escopo:**

- Documentar dashboard/frontend.
- Documentar ranking como parte deste endpoint.
- Documentar comparativos globais.
- Criar exemplos de features futuras como prontas.

**Criterios de aceite:**

- Documentacao permite testar o endpoint localmente.
- Definicoes das metricas estao claras.
- Escopo fora da tarefa esta claro.

**Prompt sugerido:**

```text
Atualize somente a documentacao local de GET /stats/me. Documente autenticacao obrigatoria, campos de resposta, definicoes de acerto/percentual/sequencia, referencia ao historico GET /bets/me e exemplos simples. Deixe claro que estatisticas globais, ranking, dashboard/frontend e comparativos ficam fora desta tarefa.
```

## Etapa 19 - Validacao final da Tarefa 11

**Objetivo:** Verificar se o endpoint de estatisticas atende aos criterios originais.

**Escopo:**

- Rodar build e testes.
- Subir a API.
- Obter token de usuario.
- Chamar `GET /stats/me`.
- Conferir total de pontos.
- Conferir placares exatos.
- Conferir acertos de vencedor.
- Conferir percentual de acertos.
- Conferir melhor sequencia.
- Conferir referencia ao historico.
- Conferir que dados de outro usuario nao entram.

**Fora do escopo:**

- Criar estatisticas globais.
- Criar ranking.
- Criar frontend.
- Recalcular pontos.
- Alterar `GET /bets/me`.

**Criterios de aceite:**

- Metricas sao calculadas corretamente para o usuario autenticado.
- Percentual de acertos e consistente com a definicao adotada.
- Melhor sequencia usa a ordem por data da partida.
- Endpoint exige autenticacao.
- Pendencias foram registradas objetivamente, se houver.

**Prompt sugerido:**

```text
Valide somente a Tarefa 11. Rode build e testes, suba a API, obtenha token, chame GET /stats/me e confira totalPoints, exactScoreCount, winnerHitCount, hitPercentage, bestHitStreak, referencia ao historico e exclusao de dados de outro usuario. Se algo falhar, registre pendencia objetivamente. Nao implemente estatisticas globais, ranking, frontend, recalculo de pontos ou alteracoes em GET /bets/me.
```

## Mapeamento do escopo original para as etapas

| Item da Tarefa 11 | Etapa responsavel |
|---|---|
| Validar dependencias T07/T08 | Etapa 01 |
| Definir acerto e sequencia | Etapa 02 |
| DTO de resposta | Etapa 03 |
| Usuario autenticado via JWT | Etapas 04, 13, 14, 17 e 19 |
| Query base do usuario | Etapa 05 |
| Reutilizar classificacao de acertos | Etapa 06 |
| Total de pontos | Etapas 07, 13, 16 e 19 |
| Numero de placares exatos | Etapas 08, 13, 16 e 19 |
| Numero de acertos de vencedor | Etapas 09, 13, 16 e 19 |
| Percentual de acertos | Etapas 10, 13, 16 e 19 |
| Melhor sequencia | Etapas 11, 13, 16 e 19 |
| Referencia ao historico `GET /bets/me` | Etapas 12, 13, 18 e 19 |
| Caso de uso em `Application` | Etapa 13 |
| `GET /stats/me` | Etapa 14 |
| Erros claros | Etapa 15 |
| Testes de calculo | Etapa 16 |
| Testes de API | Etapa 17 |
| Documentacao | Etapa 18 |
| Validacao dos criterios de aceite originais | Etapa 19 |

