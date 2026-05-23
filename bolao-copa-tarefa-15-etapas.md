# Tarefa 15 - Divisao em etapas independentes

Fonte: `C:\Repo\bolao-copa-backlog.md`, secao **Tarefa 15 - Fluxo de palpites (form, historico, janela e visibilidade)**.

Este arquivo quebra a Tarefa 15 em etapas pequenas, fechadas e implementaveis separadamente. Nenhuma etapa abaixo deve implementar calculo de pontos, ranking, estatisticas, backend, endpoints novos, lancamento de resultado ou regras de negocio fora do fluxo frontend de palpites.

## Escopo original da Tarefa 15

Implementar fluxo frontend de palpites:

- Formulario de palpite por partida.
- Formulario habilitado apenas quando `isBettingOpen` for verdadeiro.
- Criar palpite via `POST /bets`.
- Editar palpite via `PUT /bets/{id}`.
- Listar historico do usuario via `GET /bets/me`.
- Tratar erro de janela fechada retornado pela API.
- Aplicar regra de visibilidade no frontend:
  - palpites de outros usuarios ocultos antes do inicio da partida;
  - palpites de outros usuarios visiveis depois do inicio da partida.
- Usar TanStack Query com invalidacao de cache apos salvar.

## Dependencia base

A Tarefa 15 depende das Tarefas 13, 14 e 08.

Cada etapa abaixo deve ser independente das outras etapas deste arquivo, mas pode exigir que exista:

- usuario autenticado e sessao persistida;
- tela/lista de partidas consumindo `GET /matches`;
- `isBettingOpen` disponivel em cada partida;
- cliente HTTP central com Bearer token;
- TanStack Query configurado;
- endpoints `POST /bets`, `PUT /bets/{id}` e `GET /bets/me`;
- contrato de erro para janela fechada;
- contrato para visualizar palpites de terceiros, se essa parte for realmente implementavel nesta tarefa.

Se esse contrato nao existir, a etapa deve registrar bloqueio objetivo e nao deve implementar T08, T13 ou T14 dentro da Tarefa 15.

## Bloqueios e pontos de decisao

- A visibilidade de palpites de terceiros depende da duvida 3.6: partida iniciada e determinada por `Status` ou por `MatchDate <= now`.
- O backlog da Tarefa 08 cobre `GET /bets/me`, mas nao define endpoint para listar palpites de outros usuarios. Se esse endpoint nao existir, a etapa de visibilidade deve documentar o bloqueio e preparar apenas helper/componente condicionado a contrato futuro.
- A UI deve bloquear envio fora da janela, mas a API continua sendo a fonte final de seguranca.
- O frontend nao deve calcular pontos.

## Decisoes assumidas para a Tarefa 15

- Apenas usuarios autenticados podem criar, editar e ver historico proprio.
- O formulario usa `isBettingOpen` recebido da API como regra principal de habilitacao.
- Gols previstos devem ser inteiros nao negativos.
- Se o usuario ja tiver palpite para a partida, a UI deve editar esse palpite com `PUT /bets/{id}`.
- Se o usuario ainda nao tiver palpite para a partida, a UI deve criar com `POST /bets`.
- Apos salvar, invalidar caches de `GET /bets/me` e, se necessario, `GET /matches`.
- Erro de janela fechada vindo da API deve ser exibido de forma clara e deve bloquear nova tentativa sem refresh manual desnecessario.
- Historico do usuario deve vir de `GET /bets/me`, nao de dados locais inferidos.
- Enquanto a regra da duvida 3.6 nao for decidida, usar uma decisao provisoria documentada para "partida iniciada"; preferencia pratica: `status` quando disponivel, fallback para `matchDate <= now`.
- Palpites de terceiros so podem ser exibidos se houver fonte de dados aprovada.

## Regra de independencia

Cada etapa deve poder ser implementada sozinha, sem depender da execucao previa de outra etapa deste arquivo.

Para preservar essa independencia:

- A etapa deve inspecionar o estado atual antes de alterar arquivos.
- A etapa deve preservar componentes, hooks, rotas e stores existentes quando forem compativeis.
- A etapa deve criar apenas o minimo necessario para seu proprio objetivo compilar.
- Se faltar endpoint de Bets ou contrato de visibilidade, registrar bloqueio e parar a parte afetada.
- A etapa nao deve criar ou alterar backend.
- A etapa nao deve implementar ranking, estatisticas ou calculo de pontos.
- Validacoes globais da Tarefa 15 so devem ser exigidas quando todas as etapas forem executadas.

## Etapa 01 - Pre-validacao das dependencias do fluxo de palpites

**Objetivo:** Confirmar que frontend e API possuem base para criar, editar e listar palpites.

**Escopo:**

- Verificar se o frontend compila.
- Verificar se login/sessao funcionam.
- Verificar se a tela de partidas existe.
- Verificar se `isBettingOpen` esta disponivel em partidas.
- Verificar se `POST /bets`, `PUT /bets/{id}` e `GET /bets/me` estao documentados ou implementados.
- Verificar se ha contrato para palpites de terceiros.

**Fora do escopo:**

- Criar formulario.
- Criar hooks.
- Criar endpoints backend.
- Criar regra de visibilidade.

**Criterios de aceite:**

- Dependencias minimas foram confirmadas.
- Bloqueios foram registrados objetivamente, se houver.
- Nenhum fluxo de palpite foi implementado.

**Prompt sugerido:**

```text
Valide somente as dependencias da Tarefa 15. Confira build do frontend, login/sessao, tela de partidas, isBettingOpen em matches, contratos de POST /bets, PUT /bets/{id}, GET /bets/me e existencia de contrato para palpites de terceiros. Se faltar algo, registre bloqueio objetivamente. Nao crie formulario, hooks, endpoints backend ou regra de visibilidade.
```

## Etapa 02 - Decisao de visibilidade de palpites de terceiros

**Objetivo:** Registrar a regra operacional para mostrar ou ocultar palpites de outros usuarios.

**Escopo:**

- Documentar decisao da duvida 3.6, se existir.
- Definir se inicio da partida usa `Status`, `MatchDate <= now` ou fallback combinado.
- Verificar fonte de dados para palpites de terceiros.
- Registrar bloqueio se nao existir endpoint/contrato para terceiros.

**Fora do escopo:**

- Criar endpoint backend.
- Criar tela.
- Criar chamada de API.
- Alterar regra de partida.

**Criterios de aceite:**

- Regra de visibilidade esta documentada ou bloqueio esta claro.
- Nenhuma chamada ou tela de terceiros foi implementada sem contrato.

**Prompt sugerido:**

```text
Trate somente a decisao de visibilidade de palpites de terceiros. Documente a resposta da duvida 3.6 se existir, defina se inicio da partida usa Status, MatchDate <= now ou fallback combinado, e verifique se ha contrato para listar palpites de terceiros. Se nao houver, registre bloqueio. Nao crie endpoint backend, tela, chamada de API ou regra de partida.
```

## Etapa 03 - Tipos TypeScript de Bets

**Objetivo:** Definir contratos tipados para criar, editar e listar palpites.

**Escopo:**

- Criar tipo de request para `POST /bets`.
- Criar tipo de request para `PUT /bets/{id}`.
- Criar tipo de item de historico de `GET /bets/me`.
- Criar tipo de resposta de palpite salvo.
- Incluir `matchId`, gols previstos, `pointsEarned` quando vier do backend e dados basicos da partida quando o contrato retornar.

**Fora do escopo:**

- Criar chamadas HTTP.
- Criar formulario.
- Criar tipos de ranking.
- Criar tipos de estatisticas.

**Criterios de aceite:**

- Tipos de Bets existem.
- Tipos refletem o contrato da API.
- Requests nao incluem `userId`.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente os tipos TypeScript de Bets no frontend. Inclua request de criacao, request de edicao, item de historico de GET /bets/me e resposta de palpite salvo, refletindo o contrato real da API. Requests nao devem conter userId. Nao crie chamadas HTTP, formulario, ranking ou estatisticas.
```

## Etapa 04 - Funcoes de API de Bets

**Objetivo:** Criar funcoes isoladas para consumir os endpoints de palpites.

**Escopo:**

- Criar `fetchMyBets` para `GET /bets/me`.
- Criar `createBet` para `POST /bets`.
- Criar `updateBet` para `PUT /bets/{id}`.
- Usar cliente HTTP central.
- Tipar requests e responses.

**Fora do escopo:**

- Criar hooks TanStack Query.
- Criar formulario.
- Criar visibilidade de terceiros.
- Calcular pontos.

**Criterios de aceite:**

- Funcoes de API existem.
- Funcoes usam cliente HTTP central.
- Bearer token e tratado pelo cliente existente.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente as funcoes de API de Bets no frontend: fetchMyBets para GET /bets/me, createBet para POST /bets e updateBet para PUT /bets/{id}, usando o cliente HTTP central e tipos TypeScript. Nao crie hooks, formulario, visibilidade de terceiros ou calculo de pontos.
```

## Etapa 05 - Hooks TanStack Query de Bets

**Objetivo:** Expor historico e mutations de palpites para componentes.

**Escopo:**

- Criar hook `useMyBets`.
- Criar mutation de criar palpite.
- Criar mutation de editar palpite.
- Definir query keys estaveis.
- Expor loading, erro e dados.

**Fora do escopo:**

- Criar formulario visual.
- Criar invalidacao pos-save.
- Criar chamadas de terceiros.
- Criar ranking.

**Criterios de aceite:**

- Hooks de Bets existem.
- Estados de query/mutation estao disponiveis.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente hooks TanStack Query para Bets: useMyBets, mutation de createBet e mutation de updateBet, com query keys estaveis e estados de loading/erro/dados. Nao crie formulario visual, invalidacao pos-save, chamadas de terceiros ou ranking.
```

## Etapa 06 - Indexacao do historico por partida

**Objetivo:** Facilitar a decisao entre criar e editar palpite por partida.

**Escopo:**

- Criar helper para mapear `GET /bets/me` por `matchId`.
- Retornar palpite existente de uma partida.
- Manter helper puro e testavel.

**Fora do escopo:**

- Criar formulario.
- Chamar API.
- Alterar historico no backend.
- Calcular pontos.

**Criterios de aceite:**

- E possivel localizar palpite por `matchId`.
- Helper nao depende de HTTP.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente helper para indexar o historico de GET /bets/me por matchId e localizar o palpite existente de uma partida. O helper deve ser puro e testavel. Nao crie formulario, chamadas de API, alteracao de backend ou calculo de pontos.
```

## Etapa 07 - Validacao local do formulario de palpite

**Objetivo:** Validar gols previstos antes do envio.

**Escopo:**

- Validar gols do mandante obrigatorios.
- Validar gols do visitante obrigatorios.
- Validar inteiros nao negativos.
- Retornar mensagens claras.
- Manter validacao coerente com a API.

**Fora do escopo:**

- Validar janela.
- Enviar palpite.
- Criar UI completa.
- Calcular pontos.

**Criterios de aceite:**

- Gols invalidos impedem submit.
- Mensagens sao claras.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente a validacao local do formulario de palpite. Gols do mandante e visitante devem ser obrigatorios, inteiros e nao negativos, com mensagens claras e coerentes com a API. Nao valide janela, nao envie palpite, nao crie UI completa e nao calcule pontos.
```

## Etapa 08 - Componente BetForm base

**Objetivo:** Criar o formulario visual de palpite sem integracao de submit final.

**Escopo:**

- Criar campos para gols do mandante e visitante.
- Receber nomes/codigos dos times para contexto.
- Receber valores iniciais quando houver palpite existente.
- Mostrar erros de validacao local.
- Expor callback de submit tipado.
- Respeitar disabled/loading recebido por props.

**Fora do escopo:**

- Chamar API diretamente.
- Decidir create vs update.
- Invalidar cache.
- Exibir historico.

**Criterios de aceite:**

- Formulario renderiza e valida.
- Valores iniciais funcionam.
- Componente e tipado.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente o componente BetForm base. Ele deve renderizar campos de gols mandante/visitante, receber contexto dos times, valores iniciais, erros de validacao local, callback de submit tipado e props disabled/loading. Nao chame API diretamente, nao decida create/update, nao invalide cache e nao exiba historico.
```

## Etapa 09 - Bloqueio visual pela janela de palpite

**Objetivo:** Garantir que a UI desabilite o formulario quando `isBettingOpen` for falso.

**Escopo:**

- Usar `match.isBettingOpen`.
- Desabilitar campos e submit quando fechado.
- Exibir mensagem curta de janela fechada.
- Manter claro que API ainda valida a regra.

**Fora do escopo:**

- Recalcular janela no frontend.
- Alterar backend.
- Enviar palpite.
- Criar historico.

**Criterios de aceite:**

- Formulario fica habilitado quando `isBettingOpen` e verdadeiro.
- Formulario fica bloqueado quando `isBettingOpen` e falso.
- Mensagem de janela fechada aparece.
- Projeto compila.

**Prompt sugerido:**

```text
Implemente somente o bloqueio visual do BetForm pela janela de palpite. Use match.isBettingOpen, desabilite campos/submit quando falso e mostre mensagem curta de janela fechada, deixando claro que a API tambem valida. Nao recalcule janela, nao altere backend, nao envie palpite e nao crie historico.
```

## Etapa 10 - Decisao create vs update

**Objetivo:** Escolher endpoint correto com base no historico do usuario.

**Escopo:**

- Usar historico indexado por `matchId`.
- Se existir palpite da partida, preparar `PUT /bets/{id}`.
- Se nao existir, preparar `POST /bets`.
- Manter decisao em helper ou hook testavel.

**Fora do escopo:**

- Criar UI completa.
- Chamar API diretamente dentro de componente visual.
- Calcular pontos.
- Criar endpoints backend.

**Criterios de aceite:**

- Partida com palpite existente usa update.
- Partida sem palpite usa create.
- Decisao nao usa `userId` vindo da UI.
- Projeto compila.

**Prompt sugerido:**

```text
Implemente somente a decisao create vs update para Bets. Use historico indexado por matchId; se houver Bet existente, prepare updateBet com o id; se nao houver, prepare createBet. Mantenha em helper ou hook testavel. Nao crie UI completa, nao calcule pontos e nao crie backend.
```

## Etapa 11 - Integracao do BetForm com mutations

**Objetivo:** Enviar criacao ou edicao de palpite a partir do formulario.

**Escopo:**

- Conectar submit do BetForm a create/update.
- Usar mutations de Bets.
- Usar decisao create vs update.
- Exibir loading durante envio.
- Tratar sucesso e erro em nivel local.

**Fora do escopo:**

- Invalidar cache pos-save.
- Exibir ranking.
- Calcular pontos.
- Criar visibilidade de terceiros.

**Criterios de aceite:**

- Submit cria palpite quando nao existe.
- Submit edita palpite quando existe.
- Loading impede submit duplicado.
- Projeto compila.

**Prompt sugerido:**

```text
Integre somente o BetForm com as mutations de Bets. No submit, use a decisao create vs update, exiba loading durante envio e trate sucesso/erro localmente. Nao implemente invalidacao pos-save ainda, ranking, calculo de pontos ou visibilidade de terceiros.
```

## Etapa 12 - Invalidacao de cache apos salvar

**Objetivo:** Atualizar dados apos criar ou editar palpite.

**Escopo:**

- Invalidar `GET /bets/me` apos sucesso.
- Invalidar ou atualizar dados locais relacionados a partida, se necessario.
- Evitar refetch desnecessario amplo.
- Manter comportamento simples.

**Fora do escopo:**

- Criar cache manual complexo.
- Invalidar ranking/estatisticas.
- Criar optimistic update obrigatorio.
- Calcular pontos.

**Criterios de aceite:**

- Historico reflete palpite salvo.
- Dados da UI ficam consistentes apos salvar.
- Projeto compila.

**Prompt sugerido:**

```text
Implemente somente invalidacao de cache apos salvar palpite. Invalide GET /bets/me e dados relacionados a partida apenas se necessario, evitando refetch amplo. Nao crie cache manual complexo, nao invalide ranking/estatisticas, nao implemente optimistic update obrigatorio e nao calcule pontos.
```

## Etapa 13 - Tratamento de erro de janela fechada da API

**Objetivo:** Exibir erro claro quando a API recusar palpite por janela fechada.

**Escopo:**

- Detectar erro HTTP definido para janela fechada.
- Exibir mensagem clara no formulario.
- Desabilitar ou atualizar estado local quando fizer sentido.
- Preservar validacoes locais.

**Fora do escopo:**

- Alterar API.
- Recalcular `isBettingOpen`.
- Criar notificacao global complexa.
- Criar ranking.

**Criterios de aceite:**

- Erro de janela fechada e exibido claramente.
- Usuario entende que nao pode enviar/editar.
- Projeto compila.

**Prompt sugerido:**

```text
Implemente somente o tratamento frontend de erro de janela fechada vindo da API. Detecte o status/erro definido, mostre mensagem clara no formulario e desabilite/atualize estado local se fizer sentido. Nao altere API, nao recalcule isBettingOpen, nao crie notificacao global complexa ou ranking.
```

## Etapa 14 - Componente de historico do usuario

**Objetivo:** Exibir `GET /bets/me` de forma clara.

**Escopo:**

- Renderizar lista de palpites do usuario.
- Exibir partida, times, placar previsto e pontos se retornados.
- Exibir data da partida.
- Cobrir estados loading, erro e vazio.
- Manter dados apenas do usuario autenticado.

**Fora do escopo:**

- Exibir palpites de outros usuarios.
- Calcular pontos.
- Criar ranking.
- Criar estatisticas.

**Criterios de aceite:**

- Historico do usuario aparece corretamente.
- Estado vazio e claro.
- Dados de outros usuarios nao aparecem.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente o componente de historico do usuario usando GET /bets/me. Exiba partida, times, placar previsto, pontos se retornados e data da partida, com loading, erro e vazio. Nao exiba palpites de outros usuarios, nao calcule pontos, nao crie ranking ou estatisticas.
```

## Etapa 15 - Integracao do formulario na tela de partidas

**Objetivo:** Permitir palpitar diretamente a partir da lista de partidas.

**Escopo:**

- Inserir BetForm em cada partida ou em area expansivel por partida.
- Carregar valores iniciais do historico quando houver.
- Respeitar `isBettingOpen`.
- Manter layout responsivo.

**Fora do escopo:**

- Criar tela de detalhe da partida.
- Criar modal complexo.
- Exibir palpites de terceiros.
- Criar ranking.

**Criterios de aceite:**

- Usuario consegue criar/editar palpite a partir da partida.
- Formulario mostra palpite existente quando houver.
- UI bloqueia fora da janela.
- Projeto compila.

**Prompt sugerido:**

```text
Integre somente o BetForm na tela de partidas. Pode ser por card ou area expansivel por partida, usando historico para valores iniciais e respeitando isBettingOpen. Mantenha layout responsivo. Nao crie tela de detalhe, modal complexo, palpites de terceiros ou ranking.
```

## Etapa 16 - Tela/secao de historico de palpites

**Objetivo:** Disponibilizar o historico do usuario no fluxo de palpites.

**Escopo:**

- Adicionar secao ou rota para historico do usuario.
- Usar componente de historico.
- Garantir acesso apenas autenticado.
- Manter navegacao simples a partir da tela de partidas ou menu existente.

**Fora do escopo:**

- Criar estatisticas.
- Criar ranking.
- Criar exportacao.
- Exibir palpites de terceiros.

**Criterios de aceite:**

- Historico fica acessivel ao usuario.
- Historico mostra apenas palpites do usuario.
- Projeto compila.

**Prompt sugerido:**

```text
Implemente somente a secao ou rota de historico de palpites do usuario usando o componente existente. Deve exigir usuario autenticado e ter navegacao simples a partir da tela de partidas ou menu existente. Nao crie estatisticas, ranking, exportacao ou palpites de terceiros.
```

## Etapa 17 - Helper de partida iniciada para visibilidade

**Objetivo:** Isolar a regra frontend que decide se palpites de terceiros podem aparecer.

**Escopo:**

- Criar helper `hasMatchStarted` ou equivalente.
- Usar decisao da Etapa 02.
- Preferir `Status` quando essa for a decisao do produto.
- Usar `MatchDate <= now` apenas como fallback documentado.
- Manter helper testavel.

**Fora do escopo:**

- Buscar palpites de terceiros.
- Criar UI de terceiros.
- Alterar backend.
- Criar regra de pontuacao.

**Criterios de aceite:**

- Regra de partida iniciada esta isolada.
- Pendencia da duvida 3.6 esta documentada, se ainda aberta.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente um helper hasMatchStarted para a visibilidade de palpites de terceiros. Use a decisao da duvida 3.6 se existir; caso contrario, documente fallback usando Status preferencialmente ou MatchDate <= now. Nao busque palpites de terceiros, nao crie UI, nao altere backend ou pontuacao.
```

## Etapa 18 - Contrato e API para palpites de terceiros

**Objetivo:** Preparar ou bloquear a leitura de palpites de outros usuarios conforme contrato disponivel.

**Escopo:**

- Verificar se existe endpoint para palpites de terceiros.
- Se existir, criar tipos e funcao de API correspondentes.
- Se nao existir, registrar bloqueio objetivo.
- Nao inventar endpoint.

**Fora do escopo:**

- Criar backend.
- Criar UI completa.
- Expor dados antes da partida.
- Criar ranking.

**Criterios de aceite:**

- Fonte de dados de terceiros esta clara.
- Se houver contrato, tipos/funcoes existem.
- Se nao houver contrato, bloqueio esta documentado.

**Prompt sugerido:**

```text
Trate somente o contrato para palpites de terceiros. Verifique se existe endpoint aprovado; se existir, crie tipos e funcao de API correspondentes. Se nao existir, registre bloqueio objetivo e nao invente endpoint. Nao crie backend, UI completa, exposicao antes da partida ou ranking.
```

## Etapa 19 - UI de visibilidade de palpites de terceiros

**Objetivo:** Exibir ou ocultar palpites de outros usuarios conforme a regra.

**Escopo:**

- Usar helper de partida iniciada.
- Antes do inicio, mostrar estado oculto.
- Depois do inicio, renderizar palpites de terceiros se houver fonte de dados.
- Tratar ausencia de contrato como mensagem tecnica/pendencia, nao como feature pronta.

**Fora do escopo:**

- Criar endpoint backend.
- Criar moderacao.
- Criar ranking.
- Calcular pontos.

**Criterios de aceite:**

- Antes do inicio, palpites de terceiros ficam ocultos.
- Depois do inicio, dados sao exibidos somente se houver contrato.
- Projeto compila.

**Prompt sugerido:**

```text
Implemente somente a UI de visibilidade de palpites de terceiros quando houver contrato de dados. Use hasMatchStarted: antes do inicio, mostre estado oculto; depois do inicio, renderize os palpites disponiveis. Se nao houver endpoint, registre pendencia e nao finja feature pronta. Nao crie backend, moderacao, ranking ou calculo de pontos.
```

## Etapa 20 - Feedback visual do fluxo de salvar palpite

**Objetivo:** Tornar o envio de palpite compreensivel para o usuario.

**Escopo:**

- Exibir loading durante salvar.
- Exibir sucesso apos salvar.
- Exibir erro geral.
- Evitar submit duplicado.
- Manter mensagens curtas e responsivas.

**Fora do escopo:**

- Criar sistema global de notificacoes completo.
- Criar animacoes complexas.
- Criar ranking.
- Criar estatisticas.

**Criterios de aceite:**

- Usuario entende quando o palpite esta salvando.
- Sucesso e erro sao claros.
- Submit duplicado e evitado.
- Projeto compila.

**Prompt sugerido:**

```text
Implemente somente feedback visual do fluxo de salvar palpite. Mostre loading, sucesso e erro geral, evite submit duplicado e mantenha mensagens curtas/responsivas. Nao crie sistema global de notificacoes completo, animacoes complexas, ranking ou estatisticas.
```

## Etapa 21 - Responsividade e acessibilidade do fluxo de palpites

**Objetivo:** Ajustar formulario e historico para mobile e uso acessivel.

**Escopo:**

- Garantir labels nos campos.
- Garantir mensagens de erro associadas.
- Garantir foco visivel.
- Ajustar layout mobile-first.
- Evitar sobreposicao nos cards de partida.
- Garantir botoes com area de toque adequada.

**Fora do escopo:**

- Auditoria WCAG completa.
- Tema visual final amplo.
- Criar telas de outras features.
- Criar componentes complexos nao necessarios.

**Criterios de aceite:**

- Formulario funciona bem em mobile.
- Historico e legivel em mobile.
- Campos sao acessiveis.
- Projeto compila.

**Prompt sugerido:**

```text
Ajuste somente responsividade e acessibilidade do fluxo de palpites. Garanta labels, erros associados, foco visivel, layout mobile-first, ausencia de sobreposicao nos cards e botoes com area de toque adequada. Nao faca auditoria WCAG completa, tema amplo ou telas de outras features.
```

## Etapa 22 - Testes dos helpers de Bets e visibilidade

**Objetivo:** Cobrir decisoes puras sem depender da API real.

**Escopo:**

- Testar indexacao do historico por `matchId`.
- Testar decisao create vs update.
- Testar validacao de gols.
- Testar `hasMatchStarted`.
- Testar regra de ocultar/exibir terceiros.

**Fora do escopo:**

- Testar backend.
- Testar ranking.
- Testar estatisticas.
- Testar E2E real.

**Criterios de aceite:**

- Helpers principais estao cobertos.
- Testes nao dependem de API real.
- Testes passam.

**Prompt sugerido:**

```text
Crie somente testes dos helpers de Bets e visibilidade. Cubra indexacao por matchId, decisao create vs update, validacao de gols, hasMatchStarted e regra de ocultar/exibir terceiros. Nao teste backend, ranking, estatisticas ou E2E real.
```

## Etapa 23 - Testes do formulario e historico

**Objetivo:** Cobrir comportamento de UI com mocks.

**Escopo:**

- Testar formulario habilitado quando `isBettingOpen` e verdadeiro.
- Testar formulario bloqueado quando falso.
- Testar submit de create.
- Testar submit de update.
- Testar erro de janela fechada.
- Testar historico carregado.
- Testar estados loading, erro e vazio.

**Fora do escopo:**

- Teste E2E contra API real.
- Testar ranking.
- Testar estatisticas.
- Testar backend.

**Criterios de aceite:**

- Fluxos principais de UI estao cobertos.
- Testes usam mocks.
- Testes passam.

**Prompt sugerido:**

```text
Crie somente testes de UI do formulario e historico de palpites com mocks. Cubra formulario habilitado/ bloqueado por isBettingOpen, submit create, submit update, erro de janela fechada, historico carregado e estados loading/erro/vazio. Nao crie E2E contra API real, ranking, estatisticas ou backend.
```

## Etapa 24 - Validacao manual contra API

**Objetivo:** Confirmar o fluxo de palpites funcionando com API local.

**Escopo:**

- Subir API local.
- Subir frontend.
- Fazer login.
- Abrir tela de partidas.
- Criar palpite em partida com janela aberta.
- Editar palpite existente.
- Tentar enviar fora da janela.
- Conferir historico em `GET /bets/me`.
- Conferir invalidacao/atualizacao apos salvar.

**Fora do escopo:**

- Validar ranking.
- Validar estatisticas.
- Validar calculo de pontos.
- Testar deploy.

**Criterios de aceite:**

- Criacao funciona contra API.
- Edicao funciona contra API.
- Janela fechada bloqueia na UI e API.
- Historico atualiza corretamente.

**Prompt sugerido:**

```text
Valide somente o fluxo de palpites contra API local. Suba API e frontend, faca login, abra partidas, crie palpite em janela aberta, edite palpite existente, tente enviar fora da janela, confira historico em GET /bets/me e invalidacao apos salvar. Nao valide ranking, estatisticas, calculo de pontos ou deploy.
```

## Etapa 25 - Documentacao local do fluxo de palpites

**Objetivo:** Documentar como testar e manter o fluxo.

**Escopo:**

- Atualizar README ou documentacao frontend.
- Documentar endpoints usados.
- Documentar regra de janela via `isBettingOpen`.
- Documentar comportamento de create vs update.
- Documentar historico via `GET /bets/me`.
- Documentar regra de visibilidade e pendencias da duvida 3.6/contrato de terceiros.
- Reforcar que ranking e calculo de pontos ficam fora da tarefa.

**Fora do escopo:**

- Documentar endpoints inexistentes como prontos.
- Documentar ranking.
- Documentar estatisticas.
- Documentar deploy.

**Criterios de aceite:**

- Documentacao permite validar o fluxo localmente.
- Bloqueios de visibilidade estao claros, se existirem.
- Escopo fora da tarefa esta claro.

**Prompt sugerido:**

```text
Atualize somente a documentacao local do fluxo de palpites. Documente endpoints POST /bets, PUT /bets/{id}, GET /bets/me, regra de janela por isBettingOpen, create vs update, historico e regra de visibilidade com pendencias da duvida 3.6/contrato de terceiros. Nao documente endpoints inexistentes como prontos, ranking, estatisticas ou deploy.
```

## Etapa 26 - Validacao final da Tarefa 15

**Objetivo:** Verificar se o fluxo de palpites atende aos criterios originais.

**Escopo:**

- Rodar build/typecheck/lint/testes disponiveis.
- Subir frontend.
- Subir API.
- Fazer login.
- Criar palpite dentro da janela.
- Editar palpite dentro da janela.
- Confirmar UI bloqueada fora da janela.
- Confirmar API recusando fora da janela.
- Confirmar historico do usuario.
- Confirmar invalidacao de cache apos salvar.
- Confirmar regra de visibilidade de terceiros ou bloqueio documentado.
- Confirmar responsividade.

**Fora do escopo:**

- Calcular pontos.
- Ranking.
- Estatisticas.
- Backend novo.
- Deploy.

**Criterios de aceite:**

- Palpite so pode ser enviado/editado dentro da janela; UI bloqueia e API confirma.
- Historico do usuario e exibido corretamente.
- Palpites de terceiros respeitam a regra de visibilidade quando houver contrato de dados.
- Se nao houver contrato de terceiros, o bloqueio esta documentado.
- Layout e responsivo.
- Pendencias foram registradas objetivamente, se houver.

**Prompt sugerido:**

```text
Valide somente a Tarefa 15. Rode build/typecheck/lint/testes disponiveis, suba frontend e API, faca login, crie palpite dentro da janela, edite dentro da janela, confirme UI bloqueada fora da janela, confirme API recusando fora da janela, confira historico do usuario, invalidacao de cache apos salvar, regra de visibilidade de terceiros ou bloqueio documentado e responsividade. Se algo falhar, registre pendencia objetivamente. Nao implemente calculo de pontos, ranking, estatisticas, backend novo ou deploy.
```

## Mapeamento do escopo original para as etapas

| Item da Tarefa 15 | Etapa responsavel |
|---|---|
| Validar dependencias T13/T14/T08 | Etapa 01 |
| Decisao de visibilidade e duvida 3.6 | Etapas 02, 17, 18, 19 e 26 |
| Tipos de Bets | Etapa 03 |
| Integracao com `GET /bets/me` | Etapas 04, 05, 06, 14, 16, 24 e 26 |
| Integracao com `POST /bets` | Etapas 04, 05, 10, 11, 24 e 26 |
| Integracao com `PUT /bets/{id}` | Etapas 04, 05, 10, 11, 24 e 26 |
| Formulario de palpite | Etapas 07, 08, 15, 21, 23 e 26 |
| Janela `isBettingOpen` | Etapas 09, 13, 15, 23, 24 e 26 |
| Create vs update | Etapas 06, 10, 11, 22 e 26 |
| Invalidacao de cache | Etapas 12, 24 e 26 |
| Erro de janela fechada | Etapas 13, 23, 24 e 26 |
| Historico do usuario | Etapas 14, 16, 23, 24 e 26 |
| Palpites de terceiros | Etapas 17, 18, 19, 22 e 26 |
| Feedback visual | Etapa 20 |
| Responsividade e acessibilidade | Etapa 21 |
| Testes de helpers | Etapa 22 |
| Testes de UI | Etapa 23 |
| Validacao manual contra API | Etapa 24 |
| Documentacao | Etapa 25 |
| Validacao dos criterios de aceite originais | Etapa 26 |

