# Tarefa 16 - Divisao em etapas independentes

Fonte: `C:\Repo\bolao-copa-backlog.md`, secao **Tarefa 16 - Tela de ranking**.

Este arquivo quebra a Tarefa 16 em etapas pequenas, fechadas e implementaveis separadamente. Nenhuma etapa abaixo deve implementar push, WebSocket, backend, endpoint novo, ranking persistido, recalculo de pontos, estatisticas, palpites ou deploy.

## Escopo original da Tarefa 16

Implementar tela de ranking:

- Consumir `GET /ranking`.
- Renderizar ranking ordenado.
- Destacar Top 3.
- Destacar usuario logado.
- Usar refetch periodico por polling para simular "tempo real" em free tier.
- Usar TanStack Query com `refetchInterval`.
- Tratar estados de loading e erro.
- Layout mobile-first.
- Nao usar WebSocket.

## Dependencia base

A Tarefa 16 depende das Tarefas 13 e 10.

Cada etapa abaixo deve ser independente das outras etapas deste arquivo, mas pode exigir que exista:

- frontend React + Vite + TypeScript;
- Tailwind configurado;
- React Router configurado;
- cliente HTTP central;
- TanStack Query configurado;
- login/sessao funcionando;
- auth store com usuario/token;
- endpoint `GET /ranking`;
- contrato de resposta com `position`, `userId`, `name`, `points`, `isTop3` e `isCurrentUser`.

Se esse contrato nao existir, a etapa deve registrar bloqueio objetivo e nao deve implementar T10 ou T13 dentro da Tarefa 16.

## Decisoes assumidas para a Tarefa 16

- A tela de ranking sera uma rota do app, por exemplo `/ranking`.
- A rota pode ser protegida se `isCurrentUser` depender de token; se a API suportar ranking publico, a tela pode funcionar sem sessao e destacar usuario apenas quando autenticado.
- O frontend deve confiar na ordenacao retornada pela API.
- O frontend nao deve recalcular desempates.
- O frontend nao deve recalcular pontos.
- `isTop3` e `isCurrentUser` devem vir do backend e ser usados para destaque visual.
- Polling deve usar intervalo simples, por exemplo 30 segundos.
- Polling deve ser desativado quando a aba estiver em background se o TanStack Query/padrao do projeto permitir sem complexidade.
- Estados de loading, erro e lista vazia devem existir.
- Push/WebSocket ficam fora do escopo.

## Regra de independencia

Cada etapa deve poder ser implementada sozinha, sem depender da execucao previa de outra etapa deste arquivo.

Para preservar essa independencia:

- A etapa deve inspecionar o estado atual antes de alterar arquivos.
- A etapa deve preservar rotas, hooks, componentes e estilos existentes quando forem compativeis.
- A etapa deve criar apenas o minimo necessario para seu proprio objetivo compilar.
- Se faltar contrato de `GET /ranking` ou scaffold frontend, registrar bloqueio e parar.
- A etapa nao deve criar WebSocket, push, backend, snapshots ou recalculo de ranking.
- Validacoes globais da Tarefa 16 so devem ser exigidas quando todas as etapas forem executadas.

## Etapa 01 - Pre-validacao das dependencias da tela de ranking

**Objetivo:** Confirmar que o frontend e a API possuem base para exibir ranking.

**Escopo:**

- Verificar se o frontend compila.
- Verificar se login/sessao existem.
- Verificar se React Router existe.
- Verificar se TanStack Query existe.
- Verificar se cliente HTTP central existe.
- Verificar se contrato de `GET /ranking` esta documentado ou implementado.
- Verificar se resposta contem `isTop3` e `isCurrentUser`.

**Fora do escopo:**

- Criar tela de ranking.
- Criar hook de ranking.
- Criar endpoint backend.
- Criar polling.

**Criterios de aceite:**

- Dependencias minimas foram confirmadas.
- Bloqueios foram registrados objetivamente, se houver.
- Nenhum componente de ranking foi criado.

**Prompt sugerido:**

```text
Valide somente as dependencias da Tarefa 16. Confira build do frontend, login/sessao, React Router, TanStack Query, cliente HTTP central e contrato de GET /ranking com position, userId, name, points, isTop3 e isCurrentUser. Se faltar algo, registre bloqueio objetivamente. Nao crie tela de ranking, hook, endpoint backend ou polling.
```

## Etapa 02 - Tipos TypeScript de Ranking

**Objetivo:** Definir contratos tipados para o ranking no frontend.

**Escopo:**

- Criar tipo de item de ranking.
- Incluir `position`.
- Incluir `userId`.
- Incluir `name`.
- Incluir `points`.
- Incluir `isTop3`.
- Incluir `isCurrentUser`.
- Refletir nomes reais do contrato da API.

**Fora do escopo:**

- Criar chamada HTTP.
- Criar hook.
- Criar tela.
- Criar calculos de desempate.

**Criterios de aceite:**

- Tipos de ranking existem.
- Tipos refletem o contrato da API.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente os tipos TypeScript de Ranking no frontend. O item deve conter position, userId, name, points, isTop3 e isCurrentUser, respeitando o contrato real da API. Nao crie chamada HTTP, hook, tela ou calculos de desempate.
```

## Etapa 03 - Funcao de API para GET /ranking

**Objetivo:** Criar funcao isolada para buscar ranking.

**Escopo:**

- Criar funcao `fetchRanking`.
- Usar cliente HTTP central.
- Tipar retorno.
- Usar token automaticamente via cliente HTTP quando houver sessao.

**Fora do escopo:**

- Criar hook TanStack Query.
- Criar polling.
- Criar tela.
- Criar calculos locais.

**Criterios de aceite:**

- Funcao chama `GET /ranking`.
- Funcao usa cliente HTTP central.
- Retorno e tipado.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente a funcao de API fetchRanking no frontend, usando o cliente HTTP central e retorno tipado. O token deve ser enviado automaticamente pelo cliente quando houver sessao. Nao crie hook TanStack Query, polling, tela ou calculos locais.
```

## Etapa 04 - Hook useRanking com TanStack Query

**Objetivo:** Expor a busca do ranking para a tela.

**Escopo:**

- Criar hook `useRanking`.
- Usar `fetchRanking`.
- Definir query key estavel.
- Expor loading, erro, dados e refetch.
- Manter sem polling nesta etapa, se polling ficar separado.

**Fora do escopo:**

- Criar tela.
- Criar polling periodico.
- Criar cache manual complexo.
- Criar WebSocket.

**Criterios de aceite:**

- Hook de ranking existe.
- Estados da query sao acessiveis.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente o hook useRanking com TanStack Query. Ele deve usar fetchRanking, ter query key estavel e expor loading, erro, dados e refetch. Nao crie tela, polling periodico, cache manual complexo ou WebSocket.
```

## Etapa 05 - Polling do ranking

**Objetivo:** Configurar atualizacao periodica compativel com free tier.

**Escopo:**

- Configurar `refetchInterval`, por exemplo 30 segundos.
- Evitar WebSocket/push.
- Manter polling simples.
- Respeitar comportamento padrao de foco/aba do TanStack Query quando possivel.

**Fora do escopo:**

- Criar WebSocket.
- Criar SignalR.
- Criar push.
- Criar job externo.
- Criar snapshot persistido.

**Criterios de aceite:**

- Ranking atualiza periodicamente sem recarregar a pagina.
- Implementacao nao usa WebSocket/push.
- Projeto compila.

**Prompt sugerido:**

```text
Configure somente polling para o ranking usando TanStack Query refetchInterval, por exemplo 30s. Mantenha simples e compativel com free tier, respeitando comportamento de foco/aba quando possivel. Nao use WebSocket, SignalR, push, jobs externos ou snapshot persistido.
```

## Etapa 06 - Helper de formatacao de pontos e posicao

**Objetivo:** Padronizar exibicao de pontos e posicoes.

**Escopo:**

- Criar helper para formatar pontos.
- Criar helper para formatar posicao.
- Tratar zero pontos.
- Manter textos curtos.

**Fora do escopo:**

- Recalcular pontos.
- Reordenar ranking.
- Criar UI completa.
- Criar estatisticas.

**Criterios de aceite:**

- Pontos e posicoes sao exibidos de forma consistente.
- Helper nao altera dados.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente helpers de formatacao para pontos e posicao do ranking. Trate zero pontos e mantenha textos curtos. Nao recalcule pontos, nao reordene ranking, nao crie UI completa ou estatisticas.
```

## Etapa 07 - Componente RankingRow

**Objetivo:** Exibir uma linha/item do ranking.

**Escopo:**

- Mostrar posicao.
- Mostrar nome do usuario.
- Mostrar pontos.
- Aplicar destaque quando `isCurrentUser` for verdadeiro.
- Aplicar estilo compacto e responsivo.

**Fora do escopo:**

- Criar lista completa.
- Criar podium Top 3.
- Reordenar dados.
- Criar polling.

**Criterios de aceite:**

- Item renderiza dados basicos.
- Usuario atual fica visualmente destacado.
- Componente e tipado.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente o componente RankingRow. Ele deve mostrar position, name e points, destacar isCurrentUser e ter layout compacto/responsivo. Nao crie lista completa, podium Top 3, reordenacao de dados ou polling.
```

## Etapa 08 - Componente TopThreeHighlight

**Objetivo:** Exibir destaque visual para Top 3.

**Escopo:**

- Receber itens com `isTop3`.
- Destacar posicoes 1, 2 e 3.
- Manter layout mobile-first.
- Tratar lista com menos de 3 usuarios.

**Fora do escopo:**

- Calcular Top 3 localmente quando API ja envia flag.
- Criar ranking completo.
- Criar animacoes complexas.
- Criar WebSocket.

**Criterios de aceite:**

- Top 3 fica destacado.
- Lista com menos de 3 usuarios nao quebra.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente o componente TopThreeHighlight para destacar itens com isTop3. Ele deve funcionar com 1, 2 ou 3 usuarios e ter layout mobile-first. Nao recalcule Top 3 localmente se a API ja envia flag, nao crie ranking completo, animacoes complexas ou WebSocket.
```

## Etapa 09 - Componente RankingList

**Objetivo:** Renderizar lista ordenada de ranking.

**Escopo:**

- Receber lista da API.
- Preservar ordem recebida.
- Renderizar `RankingRow`.
- Destacar usuario atual.
- Lidar com lista vazia.

**Fora do escopo:**

- Recalcular ordenacao/desempate.
- Criar hook de API.
- Criar polling.
- Criar ranking persistido.

**Criterios de aceite:**

- Lista renderiza todos os itens.
- Ordem da API e preservada.
- Lista vazia tem tratamento.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente o componente RankingList. Ele deve receber a lista da API, preservar a ordem recebida, renderizar RankingRow, destacar usuario atual e tratar lista vazia. Nao recalcule ordenacao/desempate, nao crie hook, polling ou ranking persistido.
```

## Etapa 10 - Estados de loading, erro e vazio

**Objetivo:** Definir estados de UI da tela de ranking.

**Escopo:**

- Criar estado de carregamento.
- Criar estado de erro com retry.
- Criar estado de lista vazia.
- Manter mensagens curtas e responsivas.

**Fora do escopo:**

- Criar sistema global de notificacoes.
- Criar skeleton complexo obrigatorio.
- Criar tratamento backend novo.
- Criar WebSocket.

**Criterios de aceite:**

- Loading, erro e vazio estao cobertos.
- Erro permite tentar novamente quando possivel.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente estados de loading, erro e lista vazia para a tela de ranking. Erro deve permitir tentar novamente quando possivel. Mensagens devem ser curtas e responsivas. Nao crie sistema global de notificacoes, skeleton complexo obrigatorio, backend novo ou WebSocket.
```

## Etapa 11 - Tela de ranking

**Objetivo:** Implementar a tela que consome e renderiza `GET /ranking`.

**Escopo:**

- Criar rota/tela de ranking.
- Usar `useRanking`.
- Renderizar destaque de Top 3.
- Renderizar lista completa.
- Renderizar estados loading, erro e vazio.
- Manter layout mobile-first.

**Fora do escopo:**

- Criar WebSocket/push.
- Criar ranking persistido.
- Criar estatisticas.
- Criar recalculo local.

**Criterios de aceite:**

- Tela consome `GET /ranking`.
- Ranking ordenado e renderizado.
- Top 3 e usuario atual ficam destacados.
- Projeto compila.

**Prompt sugerido:**

```text
Implemente somente a tela/rota de ranking. Use useRanking, renderize TopThreeHighlight, RankingList e estados loading/erro/vazio com layout mobile-first. Nao crie WebSocket/push, ranking persistido, estatisticas ou recalculo local.
```

## Etapa 12 - Destaque do usuario logado

**Objetivo:** Garantir que a linha do usuario atual seja facil de localizar.

**Escopo:**

- Usar `isCurrentUser` vindo da API.
- Destacar visualmente a linha.
- Se usuario atual nao estiver no ranking, nao quebrar a tela.
- Opcionalmente mostrar um marcador curto "voce".

**Fora do escopo:**

- Calcular usuario atual manualmente se API ja retorna flag.
- Criar endpoint separado de posicao.
- Criar ranking parcial.
- Criar estatisticas.

**Criterios de aceite:**

- Usuario logado fica destacado quando `isCurrentUser` e verdadeiro.
- Ausencia do usuario no ranking nao quebra a tela.
- Projeto compila.

**Prompt sugerido:**

```text
Implemente somente o destaque do usuario logado no ranking usando isCurrentUser vindo da API. Destaque a linha e opcionalmente mostre marcador curto como voce. Se o usuario nao estiver no ranking, nao quebre a tela. Nao calcule usuario manualmente se a API ja retorna flag, nao crie endpoint separado, ranking parcial ou estatisticas.
```

## Etapa 13 - Indicador de atualizacao periodica

**Objetivo:** Comunicar que o ranking atualiza automaticamente.

**Escopo:**

- Exibir ultimo horario de atualizacao, se simples.
- Exibir estado discreto de refetch, se TanStack Query fornecer.
- Evitar poluir a UI.

**Fora do escopo:**

- Criar contador regressivo complexo.
- Criar WebSocket.
- Criar notificacoes.
- Criar historico de atualizacoes.

**Criterios de aceite:**

- Usuario entende que ha atualizacao automatica.
- Indicador nao atrapalha leitura.
- Projeto compila.

**Prompt sugerido:**

```text
Implemente somente um indicador discreto de atualizacao do ranking. Pode mostrar ultimo horario de atualizacao ou estado de refetch do TanStack Query. Evite poluir a UI. Nao crie contador complexo, WebSocket, notificacoes ou historico de atualizacoes.
```

## Etapa 14 - Navegacao para a tela de ranking

**Objetivo:** Tornar a tela acessivel pelo app.

**Escopo:**

- Adicionar rota `/ranking` ou padrao equivalente.
- Adicionar link no menu/navegacao existente, se houver.
- Respeitar protecao de rota decidida no app.

**Fora do escopo:**

- Criar menu completo do zero se nao existir.
- Criar layout global novo.
- Criar telas de outras features.
- Alterar autenticacao.

**Criterios de aceite:**

- Usuario consegue acessar a tela.
- Navegacao nao quebra rotas existentes.
- Projeto compila.

**Prompt sugerido:**

```text
Adicione somente a navegacao necessaria para a tela de ranking. Crie rota /ranking ou padrao equivalente e link no menu existente se houver, respeitando protecao de rota ja decidida. Nao crie menu completo do zero, layout global novo, outras telas ou alteracoes de autenticacao.
```

## Etapa 15 - Responsividade e refinamento visual

**Objetivo:** Ajustar a tela para mobile e desktop.

**Escopo:**

- Garantir layout mobile-first.
- Ajustar Top 3 em telas pequenas.
- Garantir que nomes longos nao quebrem o layout.
- Garantir que pontos e posicao fiquem legiveis.
- Evitar sobreposicao visual.

**Fora do escopo:**

- Criar design system completo.
- Criar animacoes complexas.
- Criar tema final amplo.
- Criar telas de outras features.

**Criterios de aceite:**

- Ranking funciona em mobile.
- Ranking funciona em desktop.
- Textos e numeros nao se sobrepoem.
- Projeto compila.

**Prompt sugerido:**

```text
Ajuste somente responsividade e refinamento visual da tela de ranking. Garanta layout mobile-first, Top 3 adequado em telas pequenas, nomes longos sem quebrar layout, pontos/posicao legiveis e ausencia de sobreposicao. Nao crie design system completo, animacoes complexas, tema amplo ou outras telas.
```

## Etapa 16 - Acessibilidade basica do ranking

**Objetivo:** Melhorar leitura e navegacao do ranking.

**Escopo:**

- Usar estrutura semantica para lista/tabela.
- Garantir contraste nos destaques.
- Garantir texto claro para Top 3 e usuario atual.
- Garantir foco visivel em elementos interativos, se houver.

**Fora do escopo:**

- Auditoria WCAG completa.
- Internacionalizacao.
- Criar interacoes complexas.
- Criar animacoes.

**Criterios de aceite:**

- Ranking tem estrutura semantica.
- Destaques sao legiveis.
- Projeto compila.

**Prompt sugerido:**

```text
Ajuste somente acessibilidade basica da tela de ranking. Use estrutura semantica para lista/tabela, contraste adequado nos destaques, texto claro para Top 3 e usuario atual, e foco visivel se houver elementos interativos. Nao faca auditoria WCAG completa, internacionalizacao, interacoes complexas ou animacoes.
```

## Etapa 17 - Testes dos helpers e componentes de ranking

**Objetivo:** Cobrir comportamento local sem API real.

**Escopo:**

- Testar formatacao de pontos.
- Testar formatacao de posicao.
- Testar `RankingRow`.
- Testar `TopThreeHighlight` com 1, 2 e 3 usuarios.
- Testar `RankingList` preservando ordem recebida.
- Testar destaque de usuario atual.

**Fora do escopo:**

- Testar backend.
- Testar polling real com tempo longo.
- Testar WebSocket.
- Testar estatisticas.

**Criterios de aceite:**

- Helpers e componentes principais estao cobertos.
- Testes usam mocks/dados locais.
- Testes passam.

**Prompt sugerido:**

```text
Crie somente testes dos helpers e componentes de ranking. Cubra formatacao de pontos/posicao, RankingRow, TopThreeHighlight com 1/2/3 usuarios, RankingList preservando ordem recebida e destaque de usuario atual. Nao teste backend, polling real longo, WebSocket ou estatisticas.
```

## Etapa 18 - Testes da tela de ranking

**Objetivo:** Cobrir a tela com dados mockados.

**Escopo:**

- Testar loading.
- Testar erro.
- Testar lista vazia.
- Testar renderizacao de ranking.
- Testar Top 3 destacado.
- Testar usuario atual destacado.
- Testar refetch/polling com timers controlados, se viavel.

**Fora do escopo:**

- Teste E2E contra API real.
- Testar backend.
- Testar WebSocket.
- Testar ranking persistido.

**Criterios de aceite:**

- Estados principais da tela estao cobertos.
- Testes usam mocks.
- Testes passam.

**Prompt sugerido:**

```text
Crie somente testes da tela de ranking com dados mockados. Cubra loading, erro, lista vazia, renderizacao, Top 3 destacado, usuario atual destacado e refetch/polling com timers controlados se viavel. Nao crie E2E contra API real, testes de backend, WebSocket ou ranking persistido.
```

## Etapa 19 - Validacao manual contra API

**Objetivo:** Confirmar a tela funcionando com `GET /ranking` real em ambiente local.

**Escopo:**

- Subir API local.
- Subir frontend.
- Fazer login, se necessario.
- Acessar tela de ranking.
- Confirmar chamada a `GET /ranking`.
- Conferir ordenacao.
- Conferir Top 3.
- Conferir usuario logado.
- Conferir polling sem recarregar pagina.

**Fora do escopo:**

- Validar calculo do ranking no backend.
- Criar dados complexos de desempate.
- Testar WebSocket.
- Testar deploy.

**Criterios de aceite:**

- Tela consome API real local.
- Destaques aparecem corretamente.
- Polling funciona sem reload.
- Pendencias foram registradas objetivamente, se houver.

**Prompt sugerido:**

```text
Valide somente a tela de ranking contra API local. Suba API e frontend, faca login se necessario, acesse a tela, confirme chamada GET /ranking, ordenacao, Top 3, usuario logado e polling sem recarregar a pagina. Nao valide calculo backend em profundidade, dados complexos de desempate, WebSocket ou deploy.
```

## Etapa 20 - Documentacao local da tela de ranking

**Objetivo:** Documentar como testar e manter a tela.

**Escopo:**

- Atualizar README ou documentacao frontend.
- Documentar rota da tela.
- Documentar dependencia de `GET /ranking`.
- Documentar campos usados.
- Documentar polling e intervalo escolhido.
- Documentar que WebSocket/push fica fora do escopo.

**Fora do escopo:**

- Documentar backend de ranking em detalhe.
- Documentar push/WebSocket como pronto.
- Documentar estatisticas.
- Documentar deploy.

**Criterios de aceite:**

- Documentacao permite validar a tela localmente.
- Polling e limites estao claros.
- Escopo futuro esta claro.

**Prompt sugerido:**

```text
Atualize somente a documentacao local da tela de ranking. Documente rota, dependencia de GET /ranking, campos usados, polling e intervalo escolhido, e deixe claro que WebSocket/push fica fora do escopo. Nao documente backend de ranking em detalhe, push/WebSocket como pronto, estatisticas ou deploy.
```

## Etapa 21 - Validacao final da Tarefa 16

**Objetivo:** Verificar se a tela de ranking atende aos criterios originais.

**Escopo:**

- Rodar build/typecheck/lint/testes disponiveis.
- Subir frontend.
- Subir API.
- Acessar tela de ranking.
- Confirmar ranking ordenado.
- Confirmar Top 3 destacado.
- Confirmar usuario logado destacado.
- Confirmar polling funcionando sem recarregar.
- Confirmar loading e erro.
- Confirmar responsividade.
- Confirmar que nao ha WebSocket/push.

**Fora do escopo:**

- Criar WebSocket/push.
- Criar backend novo.
- Criar estatisticas.
- Criar ranking persistido.
- Recalcular pontos.
- Deploy.

**Criterios de aceite:**

- Ranking ordenado renderizado com Top 3 e usuario logado destacados.
- Atualizacao periodica funciona sem recarregar a pagina.
- Estados de loading/erro estao tratados.
- Layout e responsivo.
- Nao ha uso de WebSocket/push.
- Pendencias foram registradas objetivamente, se houver.

**Prompt sugerido:**

```text
Valide somente a Tarefa 16. Rode build/typecheck/lint/testes disponiveis, suba frontend e API, acesse ranking, confira ranking ordenado, Top 3 destacado, usuario logado destacado, polling funcionando sem recarregar, loading/erro, responsividade e ausencia de WebSocket/push. Se algo falhar, registre pendencia objetivamente. Nao implemente WebSocket/push, backend novo, estatisticas, ranking persistido, recalculo de pontos ou deploy.
```

## Mapeamento do escopo original para as etapas

| Item da Tarefa 16 | Etapa responsavel |
|---|---|
| Validar dependencias T13/T10 | Etapa 01 |
| Tipos de Ranking | Etapa 02 |
| Consumir `GET /ranking` | Etapas 03, 04, 11, 19 e 21 |
| Polling/refetch periodico | Etapas 05, 13, 18, 19 e 21 |
| Formatacao de pontos/posicao | Etapa 06 |
| Linha do ranking | Etapa 07 |
| Destaque Top 3 | Etapas 08, 11, 17, 18, 19 e 21 |
| Lista ordenada | Etapas 09, 11, 18, 19 e 21 |
| Loading, erro e vazio | Etapa 10 |
| Tela de ranking | Etapa 11 |
| Usuario logado destacado | Etapas 07, 12, 17, 18, 19 e 21 |
| Navegacao | Etapa 14 |
| Responsividade | Etapa 15 |
| Acessibilidade basica | Etapa 16 |
| Testes de componentes/helpers | Etapa 17 |
| Testes da tela | Etapa 18 |
| Validacao manual contra API | Etapa 19 |
| Documentacao | Etapa 20 |
| Validacao dos criterios de aceite originais | Etapa 21 |

