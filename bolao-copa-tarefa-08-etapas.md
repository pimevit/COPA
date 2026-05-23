# Tarefa 08 - Divisao em etapas independentes

Fonte: `C:\Repo\bolao-copa-backlog.md`, secao **Tarefa 08 - Endpoints de Bet (criar/editar com janela + historico)**.

Este arquivo quebra a Tarefa 08 em etapas pequenas, fechadas e implementaveis separadamente. Nenhuma etapa abaixo deve implementar calculo de pontos, lancamento de resultado, visibilidade publica de palpites de outros usuarios, ranking, estatisticas, frontend, migrations ou seed.

## Escopo original da Tarefa 08

Implementar endpoints autenticados de palpites:

- `POST /bets`.
- `PUT /bets/{id}`.
- Validar janela de edicao com `now < Match.AllowBetUntil` em UTC.
- Fora da janela, retornar erro claro (`403` ou `422`).
- Garantir unicidade `(UserId, MatchId)`.
- Criar palpite quando nao existir.
- Editar palpite existente quando existir.
- `GET /bets/me` retornando historico do usuario logado.
- Validar gols previstos nao negativos.
- Usar `userId` do JWT, nunca do corpo da requisicao.

## Dependencia base

A Tarefa 08 depende das Tarefas 04 e 06.

Cada etapa abaixo deve ser independente das outras etapas deste arquivo, mas pode exigir que exista:

- autenticacao JWT funcionando;
- forma confiavel de obter `userId` do usuario logado;
- entidade `Bet`;
- entidade `Match` com `AllowBetUntil`;
- indice unico `(UserId, MatchId)`;
- `AppDbContext` com `DbSet<Bet>` e `DbSet<Match>`;
- endpoints/queries de `Matches` ou pelo menos leitura de partida por id;
- relogio UTC ou forma testavel de obter o instante atual.

Se esse contrato nao existir, a etapa deve registrar bloqueio objetivo e nao deve implementar T04 ou T06 dentro da Tarefa 08.

## Decisoes assumidas para a Tarefa 08

- Todos os endpoints de `bets` exigem usuario autenticado.
- O `userId` sempre vem do JWT, nunca do body ou query string.
- `POST /bets` cria um palpite quando ainda nao existe palpite do usuario para a partida.
- Se ja existir palpite para `(UserId, MatchId)`, `POST /bets` nao deve criar duplicado; pode retornar `409 Conflict` com mensagem clara, salvo decisao futura de upsert.
- `PUT /bets/{id}` edita apenas palpite do proprio usuario.
- `PUT /bets/{id}` deve retornar `404` se o palpite nao existir para o usuario logado.
- Alteracoes so sao aceitas quando `now UTC < Match.AllowBetUntil`.
- Fora da janela, usar `403 Forbidden` ou `422 Unprocessable Entity`; escolher um padrao e documentar. A preferencia pratica e `422` para regra de negocio violada.
- `PointsEarned` nao e calculado nesta tarefa; novo palpite deve manter `PointsEarned` como zero ou valor padrao definido no modelo.
- Historico de `GET /bets/me` retorna somente palpites do usuario autenticado.
- Historico deve incluir dados basicos da partida e selecoes para consumo do frontend, sem expor palpites de outros usuarios.

## Regra de independencia

Cada etapa deve poder ser implementada sozinha, sem depender da execucao previa de outra etapa deste arquivo.

Para preservar essa independencia:

- A etapa deve inspecionar o estado atual antes de alterar arquivos.
- A etapa deve preservar contratos, validadores, endpoints e servicos existentes quando forem compativeis.
- A etapa deve criar apenas o minimo necessario para seu proprio objetivo compilar.
- Se faltar contrato das Tarefas 04/06, registrar bloqueio e parar.
- A etapa nao deve calcular pontos, registrar resultados, expor palpites publicos ou criar ranking.
- Validacoes globais da Tarefa 08 so devem ser exigidas quando todas as etapas forem executadas.

## Etapa 01 - Pre-validacao das dependencias de Bets

**Objetivo:** Confirmar que existe base minima para criar endpoints autenticados de palpite.

**Escopo:**

- Verificar se a solucao backend compila.
- Verificar se autenticacao JWT existe.
- Verificar se ha forma de obter `userId` do JWT.
- Verificar se `Bet`, `Match` e `AllowBetUntil` existem.
- Verificar se existe indice unico `(UserId, MatchId)`.
- Verificar se `AppDbContext` possui `DbSet<Bet>` e `DbSet<Match>`.

**Fora do escopo:**

- Criar autenticacao.
- Criar entidades ou migrations.
- Criar endpoints de bets.
- Criar regra de janela.

**Criterios de aceite:**

- Dependencias minimas foram confirmadas.
- Bloqueios foram registrados objetivamente, se houver.
- Nenhum endpoint de `bets` foi criado.

**Prompt sugerido:**

```text
Valide somente as dependencias da Tarefa 08. Confira build, autenticacao JWT, forma de obter userId do JWT, entidades Bet/Match, Match.AllowBetUntil, indice unico (UserId, MatchId), DbSet<Bet> e DbSet<Match>. Se faltar algo, registre bloqueio objetivamente. Nao crie autenticacao, entidades, migrations, endpoints de bets ou regra de janela.
```

## Etapa 02 - DTOs e contratos de Bets

**Objetivo:** Definir contratos publicos dos endpoints de palpite.

**Escopo:**

- Criar DTO de request para criar palpite.
- Criar DTO de request para editar palpite.
- Criar DTO de resposta de palpite.
- Criar DTO de historico do usuario.
- Incluir `matchId`, gols previstos, dados basicos da partida e times quando necessario.
- Nao incluir `userId` no request.

**Fora do escopo:**

- Criar validadores.
- Criar endpoints.
- Criar queries EF.
- Calcular pontos.

**Criterios de aceite:**

- DTOs existem em camada apropriada.
- Requests nao recebem `userId`.
- Responses nao expõem dados de outros usuarios.
- A solucao compila.

**Prompt sugerido:**

```text
Crie somente os DTOs/contratos de Bets. Inclua request de criacao, request de edicao, response de palpite e response de historico do usuario. Requests devem conter matchId e gols previstos conforme aplicavel, mas nunca userId. Responses podem incluir dados basicos da partida e times. Nao crie validadores, endpoints, queries EF ou calculo de pontos.
```

## Etapa 03 - Servico de usuario autenticado

**Objetivo:** Isolar a leitura do `userId` a partir do JWT.

**Escopo:**

- Criar ou reutilizar abstracao para obter usuario logado.
- Extrair `userId` das claims do JWT.
- Retornar erro claro quando usuario nao estiver autenticado ou claim estiver ausente.
- Registrar no DI se necessario.

**Fora do escopo:**

- Implementar login ou emissao de token.
- Criar roles/admin.
- Criar endpoints de bets.
- Aceitar `userId` no body.

**Criterios de aceite:**

- Codigo de Bets consegue obter `userId` do JWT.
- `userId` nao vem do request body.
- Ausencia de usuario autenticado tem tratamento claro.
- A solucao compila.

**Prompt sugerido:**

```text
Crie ou reutilize somente um servico para obter o userId do usuario autenticado a partir das claims do JWT. Retorne erro claro quando nao houver usuario ou claim valida. Registre no DI se necessario. Nao implemente login, emissao de token, roles, endpoints de bets nem aceite userId no body.
```

## Etapa 04 - Abstracao de relogio UTC para Bets

**Objetivo:** Garantir que a janela de palpite use horario UTC testavel.

**Escopo:**

- Criar ou reutilizar abstracao de relogio UTC.
- Garantir que comandos de Bet usem `now` em UTC.
- Evitar horario local.
- Permitir testes deterministiscos da janela.

**Fora do escopo:**

- Calcular `AllowBetUntil`.
- Alterar seed de partidas.
- Criar endpoints.
- Calcular pontos.

**Criterios de aceite:**

- Existe forma testavel de obter `now UTC`.
- Regras de Bets nao dependem de horario local.
- A solucao compila.

**Prompt sugerido:**

```text
Crie ou reutilize somente uma abstracao de relogio UTC para os comandos de Bets. O codigo deve obter now em UTC de forma testavel e nao usar horario local. Nao calcule AllowBetUntil, nao altere seed, nao crie endpoints e nao calcule pontos.
```

## Etapa 05 - Helper de janela de palpite

**Objetivo:** Isolar a regra `now UTC < Match.AllowBetUntil`.

**Escopo:**

- Criar helper/servico simples para validar se a janela esta aberta.
- Usar regra `now UTC < AllowBetUntil`.
- Cobrir antes, exatamente no limite e depois do limite.
- Retornar resultado simples para comandos decidirem o erro.

**Fora do escopo:**

- Criar/editar palpite.
- Criar endpoints.
- Calcular pontos.
- Alterar `AllowBetUntil`.

**Criterios de aceite:**

- Antes do limite, janela esta aberta.
- Exatamente no limite, janela esta fechada.
- Depois do limite, janela esta fechada.
- Regra e testavel sem banco e sem HTTP.

**Prompt sugerido:**

```text
Crie somente um helper/servico simples para validar janela de palpite com a regra now UTC < Match.AllowBetUntil. Cubra antes do limite, exatamente no limite e depois do limite. Nao crie/edite palpite, nao crie endpoints, nao calcule pontos e nao altere AllowBetUntil.
```

## Etapa 06 - Validadores de request de Bets

**Objetivo:** Validar formato basico dos requests de palpite.

**Escopo:**

- Validar `matchId` obrigatorio no create.
- Validar gols previstos como inteiros nao negativos.
- Validar campos obrigatorios.
- Integrar com FluentValidation se a Tarefa 05 existir.

**Fora do escopo:**

- Validar janela de palpite.
- Validar propriedade do usuario.
- Calcular pontos.
- Criar endpoints.

**Criterios de aceite:**

- Gols negativos sao rejeitados.
- `matchId` ausente/invalido e rejeitado no create.
- Validacoes retornam erro claro.
- A solucao compila.

**Prompt sugerido:**

```text
Crie somente validadores para os requests de Bets. Valide matchId obrigatorio no create, gols previstos como inteiros nao negativos e campos obrigatorios. Use FluentValidation se ja estiver configurado. Nao valide janela, propriedade do usuario, pontuacao ou crie endpoints.
```

## Etapa 07 - Acesso a dados de Bets

**Objetivo:** Isolar operacoes de leitura e escrita de palpites.

**Escopo:**

- Buscar palpite por `(UserId, MatchId)`.
- Buscar palpite por `id` e `UserId`.
- Criar palpite.
- Atualizar gols previstos.
- Listar historico por `UserId`.
- Consultar `Match` com dados necessarios para janela.

**Fora do escopo:**

- Criar endpoint HTTP.
- Calcular pontuacao.
- Registrar resultado.
- Expor palpites de outros usuarios.

**Criterios de aceite:**

- Operacoes usam `AppDbContext`.
- Consultas filtram por usuario quando necessario.
- Historico retorna apenas dados do usuario.
- A solucao compila.

**Prompt sugerido:**

```text
Crie somente o acesso a dados necessario para Bets. Deve buscar Bet por (UserId, MatchId), buscar Bet por id e UserId, criar Bet, atualizar gols previstos, listar historico por UserId e consultar Match com AllowBetUntil. Use AppDbContext e sempre filtre por usuario quando aplicavel. Nao crie endpoints, nao calcule pontuacao, nao registre resultado e nao exponha palpites de outros usuarios.
```

## Etapa 08 - Caso de uso de criar palpite

**Objetivo:** Implementar a logica de criacao de palpite autenticado.

**Escopo:**

- Receber `userId` autenticado e request de criacao.
- Verificar se a partida existe.
- Verificar se a janela esta aberta.
- Verificar se ja existe palpite para `(UserId, MatchId)`.
- Criar palpite quando nao existir.
- Manter `PointsEarned` sem calculo nesta tarefa.
- Retornar DTO publico.

**Fora do escopo:**

- Endpoint HTTP.
- Editar palpite existente.
- Calcular pontuacao.
- Criar resultado da partida.

**Criterios de aceite:**

- Palpite dentro da janela e salvo.
- Fora da janela retorna erro de aplicacao claro.
- Duplicidade nao cria segundo registro.
- `PointsEarned` nao e calculado.

**Prompt sugerido:**

```text
Implemente somente o caso de uso de criar palpite. Use userId autenticado, verifique se Match existe, valide janela aberta com now UTC < AllowBetUntil, garanta que nao exista Bet para (UserId, MatchId), crie o palpite e retorne DTO publico. Nao crie endpoint HTTP, nao edite palpite existente, nao calcule pontuacao e nao registre resultado.
```

## Etapa 09 - Caso de uso de editar palpite

**Objetivo:** Implementar a logica de edicao de palpite do proprio usuario.

**Escopo:**

- Receber `userId` autenticado, `betId` e request de edicao.
- Buscar palpite por `id` e `UserId`.
- Verificar se a partida do palpite existe.
- Verificar se a janela esta aberta.
- Atualizar gols previstos.
- Retornar DTO publico.

**Fora do escopo:**

- Endpoint HTTP.
- Editar palpite de outro usuario.
- Calcular pontuacao.
- Alterar `Match`.

**Criterios de aceite:**

- Usuario so edita o proprio palpite.
- Palpite inexistente para o usuario retorna erro claro.
- Fora da janela retorna erro claro.
- `PointsEarned` nao e recalculado.

**Prompt sugerido:**

```text
Implemente somente o caso de uso de editar palpite. Use userId autenticado e betId, busque Bet por id e UserId, valide janela aberta da partida, atualize gols previstos e retorne DTO publico. Nao crie endpoint HTTP, nao permita editar palpite de outro usuario, nao calcule pontuacao e nao altere Match.
```

## Etapa 10 - Caso de uso de historico do usuario

**Objetivo:** Implementar a leitura do historico de palpites do usuario logado.

**Escopo:**

- Receber `userId` autenticado.
- Listar apenas palpites desse usuario.
- Incluir dados basicos da partida.
- Incluir dados dos times.
- Ordenar por data da partida, preferencialmente crescente ou decrescente documentado.
- Retornar DTO de historico.

**Fora do escopo:**

- Expor palpites de outros usuarios.
- Aplicar regra de visibilidade publica.
- Calcular pontuacao.
- Criar endpoint HTTP.

**Criterios de aceite:**

- Historico retorna apenas palpites do usuario autenticado.
- Dados de partida e times estao resolvidos.
- Query evita N+1.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente o caso de uso de historico de palpites do usuario. Use userId autenticado, liste apenas Bets desse usuario, inclua dados basicos da partida e times, ordene por MatchDate de forma documentada e projete para DTO de historico evitando N+1. Nao exponha palpites de outros usuarios, nao aplique visibilidade publica, nao calcule pontuacao e nao crie endpoint HTTP.
```

## Etapa 11 - Endpoint POST /bets

**Objetivo:** Expor a criacao de palpite via HTTP autenticado.

**Escopo:**

- Criar endpoint `POST /bets`.
- Exigir autenticacao.
- Obter `userId` do JWT.
- Chamar caso de uso de criacao.
- Retornar sucesso com DTO publico.
- Mapear janela fechada para `403` ou `422`, conforme padrao escolhido.
- Mapear duplicidade para `409`, salvo decisao futura de upsert.

**Fora do escopo:**

- Criar endpoint de edicao.
- Criar endpoint de historico.
- Calcular pontos.
- Aceitar `userId` no body.

**Criterios de aceite:**

- Endpoint exige token.
- Palpite dentro da janela e salvo.
- Fora da janela retorna erro claro.
- Duplicidade nao cria segundo palpite.

**Prompt sugerido:**

```text
Crie somente o endpoint autenticado POST /bets. Ele deve obter userId do JWT, chamar o caso de uso de criacao, retornar DTO publico, mapear janela fechada para 403 ou 422 conforme padrao escolhido e duplicidade para 409 salvo decisao de upsert. Nao crie PUT, historico, pontuacao nem aceite userId no body.
```

## Etapa 12 - Endpoint PUT /bets/{id}

**Objetivo:** Expor a edicao de palpite via HTTP autenticado.

**Escopo:**

- Criar endpoint `PUT /bets/{id}`.
- Exigir autenticacao.
- Obter `userId` do JWT.
- Chamar caso de uso de edicao.
- Retornar sucesso com DTO publico.
- Retornar `404` quando o palpite nao existir para o usuario.
- Mapear janela fechada para `403` ou `422`, conforme padrao escolhido.

**Fora do escopo:**

- Criar endpoint de criacao.
- Criar endpoint de historico.
- Editar palpite de outro usuario.
- Calcular pontos.

**Criterios de aceite:**

- Endpoint exige token.
- Usuario so edita o proprio palpite.
- Fora da janela retorna erro claro.
- `PointsEarned` nao e recalculado.

**Prompt sugerido:**

```text
Crie somente o endpoint autenticado PUT /bets/{id}. Ele deve obter userId do JWT, chamar o caso de uso de edicao, retornar DTO publico, retornar 404 quando o palpite nao existir para o usuario e mapear janela fechada para 403 ou 422 conforme padrao escolhido. Nao crie POST, historico, pontuacao ou edicao de palpite de outro usuario.
```

## Etapa 13 - Endpoint GET /bets/me

**Objetivo:** Expor o historico de palpites do usuario autenticado.

**Escopo:**

- Criar endpoint `GET /bets/me`.
- Exigir autenticacao.
- Obter `userId` do JWT.
- Chamar caso de uso de historico.
- Retornar somente palpites do usuario.

**Fora do escopo:**

- Expor palpites de outros usuarios.
- Criar endpoint publico por partida.
- Aplicar visibilidade publica.
- Calcular pontos.

**Criterios de aceite:**

- Endpoint exige token.
- Retorna apenas palpites do usuario autenticado.
- Resposta inclui dados suficientes para historico.
- Nao vaza dados de outros usuarios.

**Prompt sugerido:**

```text
Crie somente o endpoint autenticado GET /bets/me. Ele deve obter userId do JWT, chamar o caso de uso de historico e retornar apenas palpites do usuario autenticado com dados suficientes para historico. Nao exponha palpites de outros usuarios, nao crie endpoint publico por partida, nao aplique visibilidade publica e nao calcule pontos.
```

## Etapa 14 - Tratamento de erros de Bets

**Objetivo:** Padronizar respostas de erro dos fluxos de palpite.

**Escopo:**

- Mapear partida inexistente.
- Mapear palpite inexistente para o usuario.
- Mapear janela fechada.
- Mapear duplicidade.
- Mapear request invalido.
- Usar `ProblemDetails` se a Tarefa 05 existir.

**Fora do escopo:**

- Criar middleware global se ainda nao existir.
- Criar regra de negocio nova.
- Calcular pontuacao.
- Alterar autenticacao.

**Criterios de aceite:**

- Erros retornam status HTTP consistentes.
- Mensagens sao claras.
- Nao ha vazamento de dados de outros usuarios.
- A solucao compila.

**Prompt sugerido:**

```text
Padronize somente os erros dos fluxos de Bets. Mapeie partida inexistente, palpite inexistente para o usuario, janela fechada, duplicidade e request invalido, usando ProblemDetails se ja existir. Nao crie middleware global novo, nao crie regra de negocio nova, nao calcule pontuacao e nao altere autenticacao.
```

## Etapa 15 - Testes da camada Application de Bets

**Objetivo:** Cobrir comandos e queries de Bets sem depender de HTTP.

**Escopo:**

- Testar criacao dentro da janela.
- Testar criacao fora da janela.
- Testar duplicidade `(UserId, MatchId)`.
- Testar edicao dentro da janela.
- Testar edicao fora da janela.
- Testar que usuario nao edita palpite de outro usuario.
- Testar historico filtrado por usuario.
- Testar gols negativos via validator ou caso de uso.

**Fora do escopo:**

- Testar pontuacao.
- Testar endpoints HTTP.
- Testar visibilidade publica.
- Testar ranking.

**Criterios de aceite:**

- Casos principais e negativos estao cobertos.
- Testes usam relogio UTC controlado.
- Testes nao dependem de API externa.
- Testes passam.

**Prompt sugerido:**

```text
Crie somente testes da camada Application para Bets. Cubra criacao dentro/fora da janela, duplicidade (UserId, MatchId), edicao dentro/fora da janela, usuario nao editar palpite de outro usuario, historico filtrado por usuario e gols negativos via validator/caso de uso. Use relogio UTC controlado. Nao teste pontuacao, endpoints HTTP, visibilidade publica ou ranking.
```

## Etapa 16 - Testes de API de Bets

**Objetivo:** Validar o comportamento HTTP dos endpoints de palpite.

**Escopo:**

- Testar endpoints sem token retornando `401`.
- Testar `POST /bets` dentro da janela.
- Testar `POST /bets` fora da janela.
- Testar duplicidade.
- Testar `PUT /bets/{id}` do proprio usuario.
- Testar `PUT /bets/{id}` de outro usuario como nao acessivel.
- Testar `GET /bets/me` retornando apenas dados do usuario.
- Testar gols negativos retornando `400`.

**Fora do escopo:**

- Testar pontuacao.
- Testar ranking.
- Testar frontend.
- Testar visibilidade publica.

**Criterios de aceite:**

- Endpoints autenticados exigem token.
- Fluxos HTTP principais estao cobertos.
- Respostas de erro seguem o padrao definido.
- Testes passam.

**Prompt sugerido:**

```text
Crie somente testes de API para Bets. Cubra 401 sem token, POST dentro da janela, POST fora da janela, duplicidade, PUT do proprio usuario, PUT de outro usuario como nao acessivel, GET /bets/me apenas do usuario e gols negativos retornando 400. Nao teste pontuacao, ranking, frontend ou visibilidade publica.
```

## Etapa 17 - Documentacao local dos endpoints de Bets

**Objetivo:** Documentar como consumir e validar os endpoints de palpite.

**Escopo:**

- Atualizar README ou documentacao tecnica.
- Documentar autenticacao obrigatoria.
- Documentar `POST /bets`.
- Documentar `PUT /bets/{id}`.
- Documentar `GET /bets/me`.
- Documentar regra de janela `now UTC < AllowBetUntil`.
- Documentar regra de unicidade `(UserId, MatchId)`.
- Documentar que pontuacao e visibilidade publica ficam fora desta tarefa.

**Fora do escopo:**

- Documentar endpoints futuros como prontos.
- Documentar frontend.
- Documentar ranking.
- Documentar recálculo de pontos.

**Criterios de aceite:**

- Documentacao permite testar os endpoints de Bets.
- Regras de autenticacao, janela e unicidade estao claras.
- Escopo ainda pendente esta claro.

**Prompt sugerido:**

```text
Atualize somente a documentacao local dos endpoints de Bets. Documente autenticacao obrigatoria, POST /bets, PUT /bets/{id}, GET /bets/me, regra now UTC < AllowBetUntil, unicidade (UserId, MatchId) e exemplos simples. Deixe claro que pontuacao, visibilidade publica, ranking e recálculo ficam fora desta tarefa.
```

## Etapa 18 - Validacao final da Tarefa 08

**Objetivo:** Verificar se os endpoints de Bets atendem aos criterios originais.

**Escopo:**

- Rodar build e testes.
- Subir a API.
- Obter token de usuario.
- Testar `POST /bets` dentro da janela.
- Testar `POST /bets` fora da janela.
- Testar duplicidade `(UserId, MatchId)`.
- Testar `PUT /bets/{id}` dentro e fora da janela.
- Testar que usuario nao altera palpite de outro usuario.
- Testar `GET /bets/me`.
- Confirmar que `GET /bets/me` retorna apenas palpites do usuario autenticado.
- Confirmar que `PointsEarned` nao e calculado nesta tarefa.

**Fora do escopo:**

- Calcular pontos.
- Registrar resultados.
- Expor palpites publicos.
- Criar ranking.
- Criar frontend.

**Criterios de aceite:**

- Palpite dentro da janela e salvo.
- Palpite fora da janela e recusado com erro claro.
- Nao e possivel criar dois palpites para a mesma partida pelo mesmo usuario.
- `GET /bets/me` retorna apenas palpites do usuario autenticado.
- `userId` vem do JWT, nunca do body.
- Gols negativos sao recusados.
- Pendencias foram registradas objetivamente, se houver.

**Prompt sugerido:**

```text
Valide somente a Tarefa 08. Rode build e testes, suba a API, obtenha token, teste POST /bets dentro e fora da janela, duplicidade (UserId, MatchId), PUT /bets/{id} dentro e fora da janela, usuario nao alterar palpite de outro usuario, GET /bets/me apenas com palpites do usuario, userId vindo do JWT e gols negativos recusados. Confirme que PointsEarned nao e calculado. Se algo falhar, registre pendencia objetivamente. Nao implemente pontuacao, resultados, visibilidade publica, ranking ou frontend.
```

## Mapeamento do escopo original para as etapas

| Item da Tarefa 08 | Etapa responsavel |
|---|---|
| Validar dependencias T04/T06 | Etapa 01 |
| DTOs de Bets | Etapa 02 |
| Usar `userId` do JWT | Etapas 03, 08, 09, 10, 11, 12, 13 e 18 |
| Relogio UTC | Etapa 04 |
| Janela `now < Match.AllowBetUntil` | Etapas 05, 08, 09, 11, 12, 15, 16 e 18 |
| Validacao de gols nao negativos | Etapas 06, 15, 16 e 18 |
| Acesso a dados de Bets | Etapa 07 |
| Criar palpite | Etapas 08 e 11 |
| Editar palpite | Etapas 09 e 12 |
| Historico `GET /bets/me` | Etapas 10 e 13 |
| Unicidade `(UserId, MatchId)` | Etapas 07, 08, 11, 15, 16 e 18 |
| Erros claros | Etapa 14 |
| Testes de Application | Etapa 15 |
| Testes de API | Etapa 16 |
| Documentacao | Etapa 17 |
| Validacao dos criterios de aceite originais | Etapa 18 |

