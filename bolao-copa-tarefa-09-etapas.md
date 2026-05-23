# Tarefa 09 - Divisao em etapas independentes

Fonte: `C:\Repo\bolao-copa-backlog.md`, secao **Tarefa 09 - Lancamento de resultado e recalculo de pontos**.

Este arquivo quebra a Tarefa 09 em etapas pequenas, fechadas e implementaveis separadamente. Nenhuma etapa abaixo deve implementar ranking, estatisticas, integracao com API esportiva, frontend, seed, migrations ou regras novas de pontuacao.

## Escopo original da Tarefa 09

Registrar resultado de partida e recalcular pontos dos palpites afetados:

- Endpoint para definir ou atualizar `HomeGoals` e `AwayGoals`.
- Ao lancar resultado, definir `Match.Status = Finished`.
- Endpoint protegido conforme decisao de role/ator da duvida 3.3.
- Ao encerrar uma partida, recalcular `PointsEarned` de todos os `Bet` da partida usando `ScoreCalculator`.
- Ao corrigir resultado ja lancado, recalcular novamente os pontos dos palpites afetados.
- Executar alteracao de resultado e recalculo dentro de uma transacao.
- Garantir idempotencia: relancar o mesmo resultado nao duplica nem corrompe pontos.

## Bloqueio explicito

A Tarefa 09 esta bloqueada pela duvida 3.3 do backlog: quem lanca o resultado no MVP?

Antes de implementar o endpoint final, precisa existir uma decisao sobre uma das opcoes:

- resultado lancado por admin via endpoint protegido;
- resultado inserido diretamente no banco;
- resultado vindo de integracao externa;
- outro fluxo operacional definido pelo produto.

Enquanto essa resposta nao existir, etapas tecnicas internas podem ser planejadas ou preparadas, mas a exposicao final do endpoint protegido deve registrar o bloqueio ou usar uma decisao provisoria explicitamente aprovada.

## Dependencia base

A Tarefa 09 depende das Tarefas 07 e 06.

Na pratica, para endpoint protegido, tambem depende de autenticacao/autorizacao da Tarefa 04 ou mecanismo equivalente.

Cada etapa abaixo deve ser independente das outras etapas deste arquivo, mas pode exigir que exista:

- `ScoreCalculator` puro no dominio;
- entidade `Match` com `HomeGoals`, `AwayGoals`, `Status` e `Stage`;
- entidade `Bet` com `PointsEarned`;
- `AppDbContext` com `DbSet<Match>` e `DbSet<Bet>`;
- leitura de partida funcionando;
- autenticacao/autorizacao se o endpoint for exposto;
- decisao da duvida 3.3 para protecao do endpoint.

Se esse contrato nao existir, a etapa deve registrar bloqueio objetivo e nao deve implementar T06, T07 ou T04 dentro da Tarefa 09.

## Decisoes assumidas para a Tarefa 09

- `HomeGoals` e `AwayGoals` devem ser inteiros nao negativos.
- Lancar resultado define `Match.Status = Finished`.
- Corrigir resultado deve sobrescrever `HomeGoals` e `AwayGoals` e recalcular todos os `Bet` daquela partida.
- Recalculo usa o `ScoreCalculator` da Tarefa 07.
- `PointsEarned` deve ser substituido pelo novo valor calculado, nao incrementado.
- Relancar exatamente o mesmo resultado deve ser idempotente.
- Toda operacao de salvar resultado e recalcular apostas deve ocorrer em transacao.
- Endpoint deve ser protegido conforme decisao de role/ator; se usar decisao provisoria, documentar claramente.
- Ranking, estatisticas e cache ficam fora desta tarefa.

## Regra de independencia

Cada etapa deve poder ser implementada sozinha, sem depender da execucao previa de outra etapa deste arquivo.

Para preservar essa independencia:

- A etapa deve inspecionar o estado atual antes de alterar arquivos.
- A etapa deve preservar contratos, endpoints, servicos e testes existentes quando forem compativeis.
- A etapa deve criar apenas o minimo necessario para seu proprio objetivo compilar.
- Se faltar decisao de autorizacao da duvida 3.3, registrar bloqueio e nao expor endpoint final.
- A etapa nao deve alterar regra de pontuacao do `ScoreCalculator`.
- A etapa nao deve implementar ranking, estatisticas ou integracao esportiva.
- Validacoes globais da Tarefa 09 so devem ser exigidas quando todas as etapas forem executadas.

## Etapa 01 - Pre-validacao das dependencias de resultado

**Objetivo:** Confirmar que existe base minima para lancar resultado e recalcular pontos.

**Escopo:**

- Verificar se a solucao backend compila.
- Verificar se `ScoreCalculator` existe e possui testes.
- Verificar se `Match` possui `HomeGoals`, `AwayGoals`, `Status` e `Stage`.
- Verificar se `Bet` possui `PointsEarned`, `MatchId` e gols previstos.
- Verificar se `AppDbContext` possui `DbSet<Match>` e `DbSet<Bet>`.
- Verificar se existe mecanismo de autenticacao/autorizacao, se endpoint for esperado.

**Fora do escopo:**

- Criar `ScoreCalculator`.
- Criar entidades ou migrations.
- Criar endpoint de resultado.
- Implementar recalculo.

**Criterios de aceite:**

- Dependencias minimas foram confirmadas.
- Bloqueios foram registrados objetivamente, se houver.
- Nenhuma regra de resultado ou recalculo foi implementada.

**Prompt sugerido:**

```text
Valide somente as dependencias da Tarefa 09. Confira build, ScoreCalculator, Match com HomeGoals/AwayGoals/Status/Stage, Bet com PointsEarned/MatchId/gols previstos, DbSet<Match>, DbSet<Bet> e mecanismo de autenticacao/autorizacao se o endpoint for esperado. Se faltar algo, registre bloqueio objetivamente. Nao crie ScoreCalculator, entidades, migrations, endpoint de resultado ou recalculo.
```

## Etapa 02 - Decisao de ator e autorizacao do lancamento

**Objetivo:** Registrar como o resultado sera lancado no MVP.

**Escopo:**

- Documentar a resposta da duvida 3.3, se ja existir.
- Definir se o endpoint sera protegido por role, policy, usuario admin ou outro mecanismo.
- Definir comportamento quando usuario nao autorizado tentar lancar resultado.
- Registrar bloqueio se a decisao ainda nao existir.

**Fora do escopo:**

- Implementar roles/admin.
- Criar endpoint.
- Criar regras de resultado.
- Alterar autenticacao.

**Criterios de aceite:**

- Decisao de autorizacao esta registrada, ou bloqueio esta claro.
- O endpoint final nao e exposto sem decisao.
- Nenhuma regra de negocio foi implementada.

**Prompt sugerido:**

```text
Trate somente a decisao de ator/autorizacao da Tarefa 09. Documente a resposta da duvida 3.3 se existir; defina se o lancamento sera por role, policy, admin ou outro mecanismo. Se nao houver decisao, registre bloqueio e nao exponha endpoint. Nao implemente roles/admin, endpoint, regras de resultado ou alteracoes de autenticacao.
```

## Etapa 03 - DTOs e contrato de lancamento de resultado

**Objetivo:** Definir o contrato publico para informar resultado de partida.

**Escopo:**

- Criar DTO de request com `HomeGoals` e `AwayGoals`.
- Criar DTO de response com dados da partida atualizada.
- Incluir `matchId` via rota, nao no body, se o endpoint seguir `/matches/{id}/result`.
- Incluir pontos recalculados em resumo apenas se a decisao de contrato exigir; caso contrario manter resposta simples.

**Fora do escopo:**

- Criar endpoint.
- Validar autorizacao.
- Recalcular pontos.
- Criar ranking ou estatisticas.

**Criterios de aceite:**

- DTOs existem em camada apropriada.
- Request nao contem campos desnecessarios.
- Response nao expoe dados sensiveis.
- A solucao compila.

**Prompt sugerido:**

```text
Crie somente os DTOs/contratos para lancamento de resultado. O request deve conter HomeGoals e AwayGoals; o matchId deve vir da rota se o endpoint for /matches/{id}/result. Crie response simples com dados da partida atualizada. Nao crie endpoint, autorizacao, recalculo, ranking ou estatisticas.
```

## Etapa 04 - Validadores do resultado

**Objetivo:** Validar formato basico do resultado informado.

**Escopo:**

- Validar `HomeGoals` obrigatorio.
- Validar `AwayGoals` obrigatorio.
- Validar gols como inteiros nao negativos.
- Integrar com FluentValidation se a Tarefa 05 existir.

**Fora do escopo:**

- Validar permissao de usuario.
- Validar estado completo da partida.
- Recalcular pontos.
- Criar endpoint.

**Criterios de aceite:**

- Gols negativos sao rejeitados.
- Campos ausentes sao rejeitados.
- Erros sao claros.
- A solucao compila.

**Prompt sugerido:**

```text
Crie somente validadores para o request de resultado. HomeGoals e AwayGoals devem ser obrigatorios e inteiros nao negativos. Use FluentValidation se ja estiver configurado. Nao valide permissao, nao valide todo o estado da partida, nao recalcule pontos e nao crie endpoint.
```

## Etapa 05 - Acesso a dados para resultado e apostas afetadas

**Objetivo:** Isolar consultas e persistencia necessarias para resultado e recalculo.

**Escopo:**

- Buscar `Match` por id.
- Buscar todos os `Bet` de uma partida.
- Atualizar resultado da partida.
- Atualizar `PointsEarned` dos palpites.
- Usar `AppDbContext`.

**Fora do escopo:**

- Criar endpoint HTTP.
- Calcular pontos.
- Criar transacao final.
- Criar ranking ou estatisticas.

**Criterios de aceite:**

- E possivel buscar partida por id.
- E possivel listar palpites afetados por `MatchId`.
- E possivel atualizar partida e palpites.
- A solucao compila.

**Prompt sugerido:**

```text
Crie somente o acesso a dados necessario para resultado e recalculo. Deve buscar Match por id, listar Bets por MatchId, atualizar HomeGoals/AwayGoals/Status da partida e atualizar PointsEarned dos Bets usando AppDbContext. Nao crie endpoint, nao calcule pontos, nao implemente transacao final, ranking ou estatisticas.
```

## Etapa 06 - Servico de recalculo de pontos por partida

**Objetivo:** Recalcular `PointsEarned` dos palpites de uma partida usando `ScoreCalculator`.

**Escopo:**

- Receber partida com resultado completo.
- Receber lista de palpites da partida.
- Calcular pontos de cada palpite com `ScoreCalculator`.
- Substituir `PointsEarned` pelo valor calculado.
- Retornar resumo opcional de quantidade recalculada.

**Fora do escopo:**

- Salvar resultado da partida.
- Criar endpoint.
- Criar transacao.
- Atualizar ranking ou estatisticas.

**Criterios de aceite:**

- Todos os palpites da partida sao recalculados.
- `PointsEarned` e substituido, nao somado.
- Recalculo nao depende de HTTP.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente um servico de recalculo de pontos por partida. Ele deve receber Match com resultado completo e Bets da partida, usar ScoreCalculator para calcular cada PointsEarned e substituir o valor existente, nao somar. Pode retornar resumo de quantidade recalculada. Nao salve resultado, nao crie endpoint, transacao, ranking ou estatisticas.
```

## Etapa 07 - Caso de uso de lancar resultado

**Objetivo:** Implementar a logica de definir resultado de uma partida.

**Escopo:**

- Receber `matchId` e request de resultado.
- Buscar partida.
- Validar que a partida existe.
- Definir `HomeGoals`, `AwayGoals` e `Status = Finished`.
- Chamar recalculo dos palpites da partida.
- Retornar DTO de resposta.

**Fora do escopo:**

- Endpoint HTTP.
- Autorizacao.
- Transacao final, se for tratada em etapa separada.
- Ranking e estatisticas.

**Criterios de aceite:**

- Resultado e aplicado na partida.
- Status fica `Finished`.
- Palpites afetados sao recalculados.
- Partida inexistente retorna erro claro.

**Prompt sugerido:**

```text
Implemente somente o caso de uso de lancar resultado. Receba matchId e resultado, busque a partida, valide existencia, defina HomeGoals/AwayGoals e Status = Finished, chame o recalculo dos Bets da partida e retorne DTO de resposta. Nao crie endpoint HTTP, autorizacao, ranking ou estatisticas.
```

## Etapa 08 - Correcao de resultado ja lancado

**Objetivo:** Garantir que atualizar resultado existente recalcula os pontos corretamente.

**Escopo:**

- Permitir sobrescrever `HomeGoals` e `AwayGoals` de partida ja finalizada.
- Recalcular todos os `Bet` afetados apos a correcao.
- Garantir que pontos antigos sejam substituidos.
- Registrar comportamento quando o resultado novo for igual ao anterior.

**Fora do escopo:**

- Historico/auditoria de alteracoes.
- Ranking.
- Estatisticas.
- Integracao externa.

**Criterios de aceite:**

- Corrigir resultado recalcula pontos.
- Pontos nao sao duplicados nem incrementados.
- Mesmo resultado relancado permanece idempotente.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente o comportamento de correcao de resultado ja lancado. Sobrescreva HomeGoals/AwayGoals, recalcule todos os Bets afetados e substitua PointsEarned, sem incrementar. Relancar o mesmo resultado deve ser idempotente. Nao implemente auditoria, ranking, estatisticas ou integracao externa.
```

## Etapa 09 - Transacao da operacao de resultado e recalculo

**Objetivo:** Garantir atomicidade entre salvar resultado e recalcular apostas.

**Escopo:**

- Executar atualizacao da partida e dos palpites em uma transacao.
- Garantir rollback se o recalculo falhar.
- Evitar salvar resultado sem pontos coerentes.
- Manter transacao no nivel de aplicacao/infra adequado ao projeto.

**Fora do escopo:**

- Criar endpoint.
- Criar retries complexos.
- Criar outbox/eventos.
- Atualizar ranking ou estatisticas.

**Criterios de aceite:**

- Resultado e pontos sao salvos atomicamente.
- Falha no recalculo nao deixa resultado parcial.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente a transacao da operacao de resultado e recalculo. A atualizacao de Match e Bets deve ser atomica; se o recalculo falhar, nada deve ficar parcialmente salvo. Nao crie endpoint, retries complexos, outbox/eventos, ranking ou estatisticas.
```

## Etapa 10 - Politica de autorizacao do endpoint de resultado

**Objetivo:** Aplicar a decisao de protecao do lancamento de resultado.

**Escopo:**

- Implementar policy/role/checagem definida pela Etapa 02.
- Garantir `401` para usuario nao autenticado.
- Garantir `403` para usuario autenticado sem permissao.
- Manter a autorizacao restrita ao endpoint de resultado.

**Fora do escopo:**

- Criar sistema completo de administracao.
- Criar painel admin.
- Criar recuperacao de senha.
- Alterar fluxos de usuario comum.

**Criterios de aceite:**

- Somente ator autorizado consegue lancar resultado.
- Sem token retorna `401`.
- Sem permissao retorna `403`.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente a politica de autorizacao definida para lancamento de resultado. Garanta 401 sem autenticacao e 403 para usuario autenticado sem permissao. Restrinja a mudanca ao endpoint de resultado. Nao crie sistema completo de administracao, painel admin, recuperacao de senha ou alteracoes nos fluxos de usuario comum.
```

## Etapa 11 - Endpoint para lancar ou atualizar resultado

**Objetivo:** Expor via HTTP a operacao de resultado.

**Escopo:**

- Criar endpoint, por exemplo `PUT /matches/{id}/result`.
- Exigir autorizacao definida.
- Receber `HomeGoals` e `AwayGoals`.
- Chamar caso de uso de lancamento/correcao.
- Retornar DTO de resposta.
- Retornar `404` para partida inexistente.

**Fora do escopo:**

- Criar endpoints de ranking.
- Criar endpoints de estatisticas.
- Criar integracao esportiva.
- Criar endpoints publicos de palpites.

**Criterios de aceite:**

- Endpoint atualiza resultado da partida.
- Endpoint define `Status = Finished`.
- Endpoint recalcula pontos dos palpites afetados.
- Endpoint e protegido conforme decisao de role/ator.

**Prompt sugerido:**

```text
Crie somente o endpoint protegido para lancar/atualizar resultado, preferencialmente PUT /matches/{id}/result. Ele deve receber HomeGoals/AwayGoals, chamar o caso de uso, retornar DTO de resposta, retornar 404 para partida inexistente e exigir autorizacao definida. Nao crie ranking, estatisticas, integracao esportiva ou endpoints publicos de palpites.
```

## Etapa 12 - Tratamento de erros de resultado

**Objetivo:** Padronizar respostas de erro do fluxo de lancamento.

**Escopo:**

- Mapear partida inexistente.
- Mapear request invalido.
- Mapear usuario nao autenticado.
- Mapear usuario sem permissao.
- Mapear erro inesperado sem vazar detalhes sensiveis.
- Usar `ProblemDetails` se a Tarefa 05 existir.

**Fora do escopo:**

- Criar middleware global se ainda nao existir.
- Criar regras novas de negocio.
- Alterar ScoreCalculator.
- Criar ranking ou estatisticas.

**Criterios de aceite:**

- Erros retornam status HTTP consistentes.
- Mensagens sao claras.
- Stack trace nao e exposto em producao.
- A solucao compila.

**Prompt sugerido:**

```text
Padronize somente os erros do fluxo de resultado. Mapeie partida inexistente, request invalido, usuario nao autenticado, usuario sem permissao e erro inesperado, usando ProblemDetails se ja existir. Nao crie middleware global novo, nao altere ScoreCalculator, nao crie ranking, estatisticas ou regras novas.
```

## Etapa 13 - Testes do recalculo de pontos

**Objetivo:** Cobrir o recalculo de `PointsEarned` sem HTTP.

**Escopo:**

- Testar recalculo de todos os palpites de uma partida.
- Testar que palpites de outra partida nao sao alterados.
- Testar que `PointsEarned` e substituido.
- Testar placar exato, vencedor e erro total via `ScoreCalculator`.
- Testar correcao de resultado.

**Fora do escopo:**

- Testes de endpoint HTTP.
- Testes de ranking.
- Testes de estatisticas.
- Testes de frontend.

**Criterios de aceite:**

- Recalculo atualiza todos os palpites afetados.
- Palpites nao afetados permanecem iguais.
- Correcao recalcula corretamente.
- Testes passam.

**Prompt sugerido:**

```text
Crie somente testes do recalculo de pontos sem HTTP. Cubra todos os Bets de uma partida, Bets de outra partida inalterados, PointsEarned substituido, cenarios de placar exato/vencedor/erro total via ScoreCalculator e correcao de resultado. Nao teste endpoint HTTP, ranking, estatisticas ou frontend.
```

## Etapa 14 - Testes de transacao e idempotencia

**Objetivo:** Validar atomicidade e relancamento seguro do resultado.

**Escopo:**

- Testar que falha no recalculo nao salva resultado parcial.
- Testar que relancar mesmo resultado mantem os mesmos pontos.
- Testar que corrigir resultado altera pontos para os novos valores.
- Testar que nao ha duplicacao de registros.

**Fora do escopo:**

- Criar fila/outbox.
- Criar auditoria.
- Testar ranking.
- Testar integracao externa.

**Criterios de aceite:**

- Operacao e atomica.
- Mesmo resultado e idempotente.
- Correcao recalcula sem duplicar.
- Testes passam.

**Prompt sugerido:**

```text
Crie somente testes de transacao e idempotencia da Tarefa 09. Cubra falha no recalculo sem salvar resultado parcial, relancar mesmo resultado mantendo pontos, corrigir resultado alterando pontos e ausencia de duplicacao. Nao crie fila/outbox, auditoria, ranking ou integracao externa.
```

## Etapa 15 - Testes de API do endpoint de resultado

**Objetivo:** Validar o comportamento HTTP e a protecao do endpoint.

**Escopo:**

- Testar endpoint sem token retornando `401`.
- Testar usuario sem permissao retornando `403`.
- Testar resultado valido retornando sucesso.
- Testar partida inexistente retornando `404`.
- Testar gols invalidos retornando `400`.
- Testar que `PointsEarned` foi recalculado apos a chamada.

**Fora do escopo:**

- Testar ranking.
- Testar estatisticas.
- Testar frontend.
- Testar integracao esportiva.

**Criterios de aceite:**

- Endpoint esta protegido.
- Resultado valido atualiza partida e apostas.
- Erros HTTP principais estao cobertos.
- Testes passam.

**Prompt sugerido:**

```text
Crie somente testes de API para o endpoint de resultado. Cubra 401 sem token, 403 sem permissao, sucesso com resultado valido, 404 para partida inexistente, 400 para gols invalidos e PointsEarned recalculado apos a chamada. Nao teste ranking, estatisticas, frontend ou integracao esportiva.
```

## Etapa 16 - Documentacao local do lancamento de resultado

**Objetivo:** Documentar como executar e validar o fluxo da Tarefa 09.

**Escopo:**

- Atualizar README ou documentacao tecnica.
- Documentar endpoint de resultado.
- Documentar autorizacao exigida.
- Documentar que resultado define `Status = Finished`.
- Documentar que pontos dos palpites sao recalculados.
- Documentar idempotencia e correcao de resultado.
- Registrar que ranking e estatisticas ficam fora desta tarefa.

**Fora do escopo:**

- Documentar endpoints futuros como prontos.
- Documentar painel admin inexistente.
- Documentar integracao esportiva.
- Criar frontend.

**Criterios de aceite:**

- Documentacao permite testar o fluxo localmente.
- Autorizacao e efeitos colaterais estao claros.
- Escopo fora da tarefa esta claro.

**Prompt sugerido:**

```text
Atualize somente a documentacao local da Tarefa 09. Documente endpoint de resultado, autorizacao exigida, Status = Finished, recalculo de PointsEarned, idempotencia, correcao de resultado e exemplos simples. Deixe claro que ranking, estatisticas, painel admin, integracao esportiva e frontend ficam fora desta tarefa.
```

## Etapa 17 - Validacao final da Tarefa 09

**Objetivo:** Verificar se lancamento de resultado e recalculo atendem aos criterios originais.

**Escopo:**

- Rodar build e testes.
- Subir a API.
- Obter credencial autorizada conforme decisao da duvida 3.3.
- Chamar endpoint de resultado.
- Conferir `HomeGoals`, `AwayGoals` e `Status = Finished`.
- Conferir `PointsEarned` recalculado para todos os palpites da partida.
- Corrigir resultado e conferir novo recalculo.
- Relancar mesmo resultado e conferir idempotencia.
- Conferir protecao do endpoint.

**Fora do escopo:**

- Validar ranking.
- Validar estatisticas.
- Criar integracao com API esportiva.
- Criar frontend.

**Criterios de aceite:**

- Definir resultado atualiza `PointsEarned` de todos os palpites da partida.
- Corrigir resultado recalcula os pontos.
- Relancar mesmo resultado nao duplica nem corrompe pontos.
- Endpoint esta protegido conforme decisao de role/ator.
- Pendencias foram registradas objetivamente, se houver.

**Prompt sugerido:**

```text
Valide somente a Tarefa 09. Rode build e testes, suba a API, use credencial autorizada conforme decisao da duvida 3.3, chame o endpoint de resultado, confira HomeGoals/AwayGoals/Status = Finished, PointsEarned recalculado para todos os Bets da partida, correcao de resultado recalculando novamente, relancar mesmo resultado idempotente e endpoint protegido. Se algo falhar, registre pendencia objetivamente. Nao implemente ranking, estatisticas, integracao esportiva ou frontend.
```

## Mapeamento do escopo original para as etapas

| Item da Tarefa 09 | Etapa responsavel |
|---|---|
| Validar dependencias T07/T06 | Etapa 01 |
| Resolver bloqueio da duvida 3.3 | Etapa 02 |
| DTOs de resultado | Etapa 03 |
| Validacao de gols | Etapa 04 |
| Acesso a `Match` e `Bet` | Etapa 05 |
| Recalcular `PointsEarned` via `ScoreCalculator` | Etapas 06, 07, 13 e 17 |
| Definir `HomeGoals` e `AwayGoals` | Etapas 07, 11 e 17 |
| Definir `Status = Finished` | Etapas 07, 11 e 17 |
| Corrigir resultado ja lancado | Etapas 08, 14 e 17 |
| Transacao | Etapas 09, 14 e 17 |
| Endpoint protegido conforme role/ator | Etapas 10, 11, 15 e 17 |
| Idempotencia | Etapas 08, 14 e 17 |
| Erros claros | Etapa 12 |
| Testes de recalculo | Etapa 13 |
| Testes de API | Etapa 15 |
| Documentacao | Etapa 16 |
| Validacao dos criterios de aceite originais | Etapa 17 |

