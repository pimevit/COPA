# Tarefa 14 - Divisao em etapas independentes

Fonte: `C:\Repo\bolao-copa-backlog.md`, secao **Tarefa 14 - Tela de partidas / jogos do dia**.

Este arquivo quebra a Tarefa 14 em etapas pequenas, fechadas e implementaveis separadamente. Nenhuma etapa abaixo deve implementar formulario de palpite, criacao/edicao de palpites, ranking, estatisticas, backend, endpoints novos, autenticacao nova ou regras de negocio fora da exibicao de partidas.

## Escopo original da Tarefa 14

Implementar tela de partidas:

- Consumir `GET /matches`.
- Listar partidas ordenadas por data.
- Destacar "jogos do dia".
- Exibir selecoes:
  - bandeira;
  - nome.
- Exibir data/hora no fuso definido.
- Exibir fase.
- Exibir status.
- Exibir resultado quando houver.
- Indicar visualmente se a janela de palpite esta aberta (`isBettingOpen`).
- Layout responsivo.

## Dependencia base

A Tarefa 14 depende das Tarefas 12 e 06.

Cada etapa abaixo deve ser independente das outras etapas deste arquivo, mas pode exigir que exista:

- frontend React + Vite + TypeScript;
- Tailwind configurado;
- React Router configurado;
- cliente HTTP central;
- TanStack Query configurado;
- contrato de `GET /matches`;
- API retornando dados de selecoes, data, fase, status, resultado e `isBettingOpen`.

Se esse contrato nao existir, a etapa deve registrar bloqueio objetivo e nao deve implementar T06 ou T12 dentro da Tarefa 14.

## Decisoes assumidas para a Tarefa 14

- A tela de partidas sera uma rota do app, por exemplo `/matches` ou a home autenticada.
- A tela pode ser protegida se o app ja tiver fluxo autenticado; se o produto decidir que partidas sao publicas, a rota pode ser publica sem alterar o escopo visual.
- `GET /matches` deve ser a unica fonte da lista.
- A ordenacao principal deve seguir a ordem por data recebida da API; se necessario, o frontend pode garantir ordenacao por `matchDate`.
- "Jogos do dia" sera definido pela data local exibida ao usuario, enquanto a duvida 3.4 nao tiver decisao final.
- Datas vindas da API devem ser tratadas como UTC.
- Formatacao de data/hora deve usar `Intl.DateTimeFormat` ou helper equivalente, sem hardcode de fuso quando a decisao ainda estiver aberta.
- `isBettingOpen` vem da API e deve ser exibido, nao recalculado como regra principal no frontend.
- Resultado so aparece quando houver gols dos dois times.
- Estados de loading, erro e lista vazia devem existir.
- Formulario de palpite fica para a Tarefa 15.

## Regra de independencia

Cada etapa deve poder ser implementada sozinha, sem depender da execucao previa de outra etapa deste arquivo.

Para preservar essa independencia:

- A etapa deve inspecionar o estado atual antes de alterar arquivos.
- A etapa deve preservar rotas, hooks, componentes e estilos existentes quando forem compativeis.
- A etapa deve criar apenas o minimo necessario para seu proprio objetivo compilar.
- Se faltar contrato de `GET /matches` ou scaffold frontend, registrar bloqueio e parar.
- A etapa nao deve criar formularios de palpite, chamadas de Bets, ranking ou estatisticas.
- Validacoes globais da Tarefa 14 so devem ser exigidas quando todas as etapas forem executadas.

## Etapa 01 - Pre-validacao das dependencias da tela de partidas

**Objetivo:** Confirmar que o frontend e a API possuem base para a tela.

**Escopo:**

- Verificar se o frontend compila.
- Verificar se Tailwind esta configurado.
- Verificar se React Router existe.
- Verificar se TanStack Query existe.
- Verificar se cliente HTTP central existe.
- Verificar se contrato de `GET /matches` esta documentado ou implementado.

**Fora do escopo:**

- Criar tela de partidas.
- Criar hook de matches.
- Criar endpoint backend.
- Criar formulario de palpite.

**Criterios de aceite:**

- Dependencias minimas foram confirmadas.
- Bloqueios foram registrados objetivamente, se houver.
- Nenhum componente de partidas foi criado.

**Prompt sugerido:**

```text
Valide somente as dependencias da Tarefa 14. Confira build do frontend, Tailwind, React Router, TanStack Query, cliente HTTP central e contrato de GET /matches. Se faltar algo, registre bloqueio objetivamente. Nao crie tela de partidas, hook de matches, endpoint backend ou formulario de palpite.
```

## Etapa 02 - Tipos TypeScript de Matches

**Objetivo:** Definir contratos tipados para dados de partidas no frontend.

**Escopo:**

- Criar tipo de selecao/time resumido.
- Criar tipo de partida retornada por `GET /matches`.
- Incluir `id`, times, `matchDate`, `stage`, `status`, gols opcionais e `isBettingOpen`.
- Refletir nomes reais do contrato da API.

**Fora do escopo:**

- Criar chamada HTTP.
- Criar tela.
- Criar tipos de Bets.
- Criar tipos de ranking ou estatisticas.

**Criterios de aceite:**

- Tipos de Matches existem.
- Tipos refletem o contrato da API.
- Campos opcionais de resultado aceitam nulo/ausencia.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente os tipos TypeScript de Matches no frontend. Inclua tipo de Team resumido e tipo de Match com id, homeTeam, awayTeam, matchDate, stage, status, homeGoals, awayGoals e isBettingOpen, respeitando o contrato real da API. Nao crie chamada HTTP, tela, tipos de Bets, ranking ou estatisticas.
```

## Etapa 03 - Funcao de API para GET /matches

**Objetivo:** Criar funcao isolada para buscar partidas.

**Escopo:**

- Criar funcao `fetchMatches`.
- Usar cliente HTTP central.
- Tipar retorno.
- Suportar filtros opcionais por `Stage` e `Status` apenas se o contrato frontend ja precisar.

**Fora do escopo:**

- Criar hook TanStack Query.
- Criar tela.
- Criar formulario de palpite.
- Criar chamada de detalhe `GET /matches/{id}`.

**Criterios de aceite:**

- Funcao chama `GET /matches`.
- Funcao usa cliente HTTP central.
- Retorno e tipado.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente a funcao de API fetchMatches no frontend, usando o cliente HTTP central e retorno tipado. Suporte filtros opcionais por Stage/Status apenas se o contrato local ja exigir. Nao crie hook TanStack Query, tela, formulario de palpite ou chamada GET /matches/{id}.
```

## Etapa 04 - Hook de listagem com TanStack Query

**Objetivo:** Expor a busca de partidas para a tela.

**Escopo:**

- Criar hook `useMatches` ou equivalente.
- Usar `fetchMatches`.
- Definir query key estavel.
- Expor loading, erro e dados.
- Configurar staleTime simples, se fizer sentido.

**Fora do escopo:**

- Criar tela.
- Criar mutations.
- Criar cache manual complexo.
- Criar integracao com Bets.

**Criterios de aceite:**

- Hook de matches existe.
- Estados de query sao acessiveis.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente o hook de listagem de partidas com TanStack Query, como useMatches. Ele deve usar fetchMatches, ter query key estavel e expor loading, erro e dados, com staleTime simples se fizer sentido. Nao crie tela, mutations, cache manual complexo ou integracao com Bets.
```

## Etapa 05 - Helper de formatacao de data/hora

**Objetivo:** Padronizar exibicao de data/hora das partidas.

**Escopo:**

- Criar helper para formatar `matchDate`.
- Tratar entrada como UTC.
- Usar fuso do usuario ou fuso definido pelo produto quando existir.
- Usar `Intl.DateTimeFormat` ou equivalente.
- Documentar a pendencia da duvida 3.4 se ainda aberta.

**Fora do escopo:**

- Alterar backend.
- Recalcular horario de fechamento.
- Criar configuracao de timezone do usuario.
- Criar tela.

**Criterios de aceite:**

- Datas sao exibidas de forma consistente.
- Nao ha conversao manual fragil.
- Pendencia de fuso esta documentada.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente um helper de formatacao de data/hora para MatchDate. Trate a entrada como UTC, use Intl.DateTimeFormat e o fuso do usuario ou fuso definido pelo produto quando existir. Documente a pendencia da duvida 3.4 se ainda aberta. Nao altere backend, nao recalcule fechamento, nao crie configuracao de timezone ou tela.
```

## Etapa 06 - Helper de jogos do dia

**Objetivo:** Identificar partidas que devem receber destaque de "jogos do dia".

**Escopo:**

- Criar helper `isTodayMatch` ou equivalente.
- Comparar data da partida com a data local exibida ao usuario.
- Considerar fuso definido pelo helper de data/hora.
- Manter regra simples e testavel.

**Fora do escopo:**

- Criar filtros de calendario.
- Criar agrupamento por rodada.
- Criar calendario interativo.
- Criar tela completa.

**Criterios de aceite:**

- Partidas do dia atual sao identificadas.
- Partidas de outros dias nao sao marcadas.
- Regra e testavel.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente um helper para identificar jogos do dia. Compare MatchDate com a data local exibida ao usuario, usando o mesmo criterio/fuso do helper de data. A regra deve ser simples e testavel. Nao crie filtros de calendario, agrupamento por rodada, calendario interativo ou tela completa.
```

## Etapa 07 - Helper de exibicao de resultado

**Objetivo:** Padronizar quando e como mostrar placar final.

**Escopo:**

- Criar helper para detectar se ha resultado.
- Exibir resultado apenas quando `homeGoals` e `awayGoals` estiverem preenchidos.
- Formatar placar como texto simples.

**Fora do escopo:**

- Criar regra de pontuacao.
- Criar lancamento de resultado.
- Criar tela completa.
- Criar status automatico.

**Criterios de aceite:**

- Resultado aparece apenas quando completo.
- Resultado incompleto nao quebra a UI.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente helper de exibicao de resultado. Ele deve detectar resultado completo quando homeGoals e awayGoals estiverem preenchidos e formatar placar simples. Resultado incompleto nao deve quebrar a UI. Nao crie regra de pontuacao, lancamento de resultado, tela completa ou status automatico.
```

## Etapa 08 - Mapeamento visual de fase e status

**Objetivo:** Padronizar labels de `Stage` e `Status`.

**Escopo:**

- Criar mapa de labels para fases.
- Criar mapa de labels para status.
- Manter labels curtos e claros.
- Tratar valor desconhecido de forma segura.

**Fora do escopo:**

- Alterar enums no backend.
- Criar filtros por status.
- Criar regra de negocio.
- Criar tela completa.

**Criterios de aceite:**

- Fase e status sao exibidos com texto claro.
- Valor desconhecido nao quebra a UI.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente o mapeamento visual de Stage e Status no frontend. Labels devem ser curtos e claros, com fallback seguro para valor desconhecido. Nao altere enums no backend, nao crie filtros, regra de negocio ou tela completa.
```

## Etapa 09 - Componente MatchTeams

**Objetivo:** Exibir selecoes de uma partida de forma reutilizavel.

**Escopo:**

- Criar componente para mostrar homeTeam e awayTeam.
- Exibir bandeira e nome.
- Tratar bandeira ausente com fallback visual simples.
- Manter layout responsivo.

**Fora do escopo:**

- Criar card completo de partida.
- Criar formulario de palpite.
- Criar detalhe de partida.
- Criar ranking.

**Criterios de aceite:**

- Nomes e bandeiras aparecem corretamente.
- Fallback de bandeira nao quebra layout.
- Componente e tipado.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente o componente MatchTeams para exibir homeTeam e awayTeam com bandeira e nome, fallback simples para bandeira ausente e layout responsivo. Nao crie card completo de partida, formulario de palpite, detalhe de partida ou ranking.
```

## Etapa 10 - Componente BettingWindowBadge

**Objetivo:** Exibir visualmente se a janela de palpite esta aberta.

**Escopo:**

- Criar componente para `isBettingOpen`.
- Exibir estados "aberta" e "fechada" com texto curto.
- Usar estilos acessiveis e responsivos.
- Nao recalcular a janela no componente.

**Fora do escopo:**

- Habilitar formulario de palpite.
- Chamar endpoint de Bets.
- Recalcular `AllowBetUntil`.
- Criar regras de permissao.

**Criterios de aceite:**

- Estado aberto/fechado fica visivel.
- Componente usa `isBettingOpen` recebido da API.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente o componente BettingWindowBadge para exibir isBettingOpen como janela aberta ou fechada, com texto curto, estilos acessiveis e responsivos. Nao recalcule a janela no componente, nao habilite formulario de palpite, nao chame Bets e nao recalcule AllowBetUntil.
```

## Etapa 11 - Componente MatchCard

**Objetivo:** Exibir uma partida individual com todos os dados necessarios.

**Escopo:**

- Compor `MatchTeams`, data/hora, fase, status, resultado e badge de janela.
- Destacar visualmente quando for jogo do dia.
- Manter card compacto e responsivo.
- Garantir que textos nao sobreponham controles.

**Fora do escopo:**

- Criar formulario de palpite.
- Criar link para palpite.
- Criar modal.
- Criar ranking ou estatisticas.

**Criterios de aceite:**

- Card exibe dados essenciais da partida.
- Jogo do dia tem destaque claro.
- Resultado aparece quando houver.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente o componente MatchCard. Ele deve compor selecoes, data/hora, fase, status, resultado quando houver e badge de isBettingOpen, com destaque para jogo do dia e layout compacto/responsivo. Nao crie formulario de palpite, link de palpite, modal, ranking ou estatisticas.
```

## Etapa 12 - Estados de loading, erro e vazio

**Objetivo:** Definir estados de UI para a tela de partidas.

**Escopo:**

- Criar estado de carregamento.
- Criar estado de erro com opcao de tentar novamente, se TanStack Query permitir.
- Criar estado de lista vazia.
- Manter mensagens curtas.

**Fora do escopo:**

- Criar sistema global de notificacoes.
- Criar tratamento especifico de backend.
- Criar skeleton complexo.
- Criar telas de outras features.

**Criterios de aceite:**

- Loading, erro e vazio estao cobertos.
- Mensagens cabem em mobile.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente estados de loading, erro e lista vazia para a tela de partidas. Erro pode ter tentar novamente se TanStack Query permitir. Mensagens devem ser curtas e caber em mobile. Nao crie sistema global de notificacoes, tratamento backend especifico, skeleton complexo ou telas de outras features.
```

## Etapa 13 - Tela de lista de partidas

**Objetivo:** Implementar a tela que consome e renderiza `GET /matches`.

**Escopo:**

- Criar rota/tela de partidas.
- Usar `useMatches`.
- Renderizar lista ordenada por data.
- Renderizar `MatchCard` para cada item.
- Exibir estados de loading, erro e vazio.
- Manter layout mobile-first.

**Fora do escopo:**

- Criar formulario de palpite.
- Criar historico de palpites.
- Criar ranking.
- Criar estatisticas.

**Criterios de aceite:**

- Tela lista partidas da API.
- Lista aparece ordenada por data.
- Dados das selecoes aparecem.
- Projeto compila.

**Prompt sugerido:**

```text
Implemente somente a tela/rota de partidas. Use useMatches, renderize lista ordenada por data com MatchCard para cada item e cubra loading, erro e vazio. Layout deve ser mobile-first. Nao crie formulario de palpite, historico de palpites, ranking ou estatisticas.
```

## Etapa 14 - Destaque de jogos do dia

**Objetivo:** Garantir que partidas do dia tenham destaque visivel.

**Escopo:**

- Aplicar helper de jogos do dia na tela.
- Destacar visualmente cards do dia.
- Opcionalmente criar secao "Jogos do dia" sem duplicar dados, se o layout comportar.
- Manter comportamento claro quando nao houver jogos hoje.

**Fora do escopo:**

- Criar calendario completo.
- Criar filtros interativos.
- Criar agrupamento complexo.
- Criar formulario de palpite.

**Criterios de aceite:**

- Jogos do dia ficam visualmente destacados.
- Ausencia de jogos do dia nao quebra a tela.
- Projeto compila.

**Prompt sugerido:**

```text
Implemente somente o destaque de jogos do dia na tela de partidas. Use o helper de data, destaque cards do dia e trate o caso sem jogos hoje. Crie secao separada apenas se nao duplicar dados nem complicar o layout. Nao crie calendario completo, filtros interativos, agrupamento complexo ou formulario de palpite.
```

## Etapa 15 - Responsividade e refinamento visual

**Objetivo:** Ajustar a tela para mobile e desktop.

**Escopo:**

- Garantir layout mobile-first.
- Ajustar espacamento, largura e quebra de texto.
- Garantir que bandeiras, nomes, placar e badges nao se sobreponham.
- Garantir boa leitura em desktop.

**Fora do escopo:**

- Criar design system completo.
- Criar animacoes complexas.
- Criar tema final detalhado.
- Criar telas de outras features.

**Criterios de aceite:**

- Tela funciona em mobile.
- Tela funciona em desktop.
- Nao ha sobreposicao visual.
- Projeto compila.

**Prompt sugerido:**

```text
Ajuste somente responsividade e refinamento visual da tela de partidas. Garanta layout mobile-first, espacamento, largura, quebra de texto e ausencia de sobreposicao entre bandeiras, nomes, placar e badges em mobile e desktop. Nao crie design system completo, animacoes complexas, tema final detalhado ou outras telas.
```

## Etapa 16 - Acessibilidade basica da lista

**Objetivo:** Melhorar leitura e navegacao da lista de partidas.

**Escopo:**

- Garantir textos alternativos para bandeiras quando forem imagens.
- Usar estrutura semantica para lista de partidas.
- Garantir contraste adequado nos badges.
- Garantir foco visivel em elementos interativos, se houver.

**Fora do escopo:**

- Auditoria WCAG completa.
- Internacionalizacao.
- Criar interacoes complexas.
- Criar formulario de palpite.

**Criterios de aceite:**

- Bandeiras tem alt text adequado.
- Lista tem estrutura semantica.
- Badges sao legiveis.
- Projeto compila.

**Prompt sugerido:**

```text
Ajuste somente acessibilidade basica da tela de partidas. Garanta alt text para bandeiras, estrutura semantica de lista, contraste adequado nos badges e foco visivel se houver elementos interativos. Nao faca auditoria WCAG completa, internacionalizacao, interacoes complexas ou formulario de palpite.
```

## Etapa 17 - Testes dos helpers de partidas

**Objetivo:** Cobrir regras locais de exibicao sem depender da API real.

**Escopo:**

- Testar formatacao de data/hora.
- Testar identificacao de jogos do dia.
- Testar exibicao de resultado completo/incompleto.
- Testar mapeamento de fase/status.

**Fora do escopo:**

- Testar backend.
- Testar formulario de palpite.
- Testar ranking.
- Testar estatisticas.

**Criterios de aceite:**

- Helpers principais estao cobertos.
- Testes nao dependem de API real.
- Testes passam.

**Prompt sugerido:**

```text
Crie somente testes dos helpers da tela de partidas. Cubra formatacao de data/hora, identificacao de jogos do dia, resultado completo/incompleto e mapeamento de fase/status. Nao teste backend, formulario de palpite, ranking ou estatisticas.
```

## Etapa 18 - Testes da tela de partidas

**Objetivo:** Cobrir renderizacao da lista com dados mockados.

**Escopo:**

- Testar loading.
- Testar erro.
- Testar lista vazia.
- Testar renderizacao de partidas.
- Testar destaque de jogos do dia.
- Testar badge de janela aberta/fechada.
- Testar resultado quando houver.

**Fora do escopo:**

- Teste E2E contra backend real.
- Testar Bets.
- Testar ranking.
- Testar estatisticas.

**Criterios de aceite:**

- Estados principais da tela estao cobertos.
- Testes usam mocks.
- Testes passam.

**Prompt sugerido:**

```text
Crie somente testes da tela de partidas com dados mockados. Cubra loading, erro, lista vazia, renderizacao de partidas, destaque de jogos do dia, badge de janela aberta/fechada e resultado quando houver. Nao crie E2E contra backend real, testes de Bets, ranking ou estatisticas.
```

## Etapa 19 - Validacao manual contra API

**Objetivo:** Confirmar a tela funcionando com `GET /matches` real em ambiente local.

**Escopo:**

- Subir API local.
- Subir frontend.
- Acessar tela de partidas.
- Confirmar chamada a `GET /matches`.
- Conferir dados das selecoes.
- Conferir data/hora exibida.
- Conferir fase, status e resultado.
- Conferir `isBettingOpen`.

**Fora do escopo:**

- Criar palpite.
- Testar ranking.
- Testar estatisticas.
- Testar deploy.

**Criterios de aceite:**

- Tela consome API real local.
- Lista renderiza dados esperados.
- Estados visuais estao corretos.
- Pendencias foram registradas objetivamente, se houver.

**Prompt sugerido:**

```text
Valide somente a tela de partidas contra API local. Suba API e frontend, acesse a tela, confirme chamada GET /matches, dados das selecoes, data/hora, fase, status, resultado quando houver e isBettingOpen. Nao crie palpite, ranking, estatisticas ou deploy.
```

## Etapa 20 - Documentacao local da tela de partidas

**Objetivo:** Documentar como testar e manter a tela.

**Escopo:**

- Atualizar README ou documentacao frontend.
- Documentar rota da tela.
- Documentar dependencia de `GET /matches`.
- Documentar regra de jogos do dia.
- Documentar fuso/data e pendencia da duvida 3.4.
- Documentar que formulario de palpite fica para T15.

**Fora do escopo:**

- Documentar palpites como prontos.
- Documentar ranking ou estatisticas.
- Documentar deploy.
- Criar guia visual extenso.

**Criterios de aceite:**

- Documentacao permite validar a tela localmente.
- Dependencias e limites estao claros.
- Escopo futuro esta claro.

**Prompt sugerido:**

```text
Atualize somente a documentacao local da tela de partidas. Documente rota da tela, dependencia de GET /matches, regra de jogos do dia, formatacao de data/fuso e pendencia da duvida 3.4. Deixe claro que formulario de palpite fica para T15. Nao documente palpites, ranking, estatisticas ou deploy como prontos.
```

## Etapa 21 - Validacao final da Tarefa 14

**Objetivo:** Verificar se a tela de partidas atende aos criterios originais.

**Escopo:**

- Rodar build/typecheck/lint/testes disponiveis.
- Subir frontend.
- Subir API.
- Acessar tela de partidas.
- Confirmar lista ordenada por data.
- Confirmar dados das selecoes.
- Confirmar destaque de jogos do dia.
- Confirmar data/hora formatada.
- Confirmar fase e status.
- Confirmar resultado quando houver.
- Confirmar indicacao visual de `isBettingOpen`.
- Confirmar layout responsivo.

**Fora do escopo:**

- Formulario de palpite.
- Criacao/edicao de palpite.
- Ranking.
- Estatisticas.
- Deploy.

**Criterios de aceite:**

- Lista renderiza ordenada por data com dados das selecoes.
- Estado da janela de palpite fica visivel por partida.
- Jogos do dia ficam destacados.
- Layout e responsivo.
- Pendencias foram registradas objetivamente, se houver.

**Prompt sugerido:**

```text
Valide somente a Tarefa 14. Rode build/typecheck/lint/testes disponiveis, suba frontend e API, acesse a tela de partidas e confira lista ordenada por data, dados das selecoes, destaque de jogos do dia, data/hora, fase, status, resultado quando houver, indicacao visual de isBettingOpen e layout responsivo. Se algo falhar, registre pendencia objetivamente. Nao implemente formulario de palpite, criacao/edicao de palpite, ranking, estatisticas ou deploy.
```

## Mapeamento do escopo original para as etapas

| Item da Tarefa 14 | Etapa responsavel |
|---|---|
| Validar dependencias T12/T06 | Etapa 01 |
| Tipos de Matches | Etapa 02 |
| Consumir `GET /matches` | Etapas 03, 04, 13, 19 e 21 |
| Data/hora no fuso definido | Etapas 05, 17, 19, 20 e 21 |
| Destaque de jogos do dia | Etapas 06, 14, 18, 19 e 21 |
| Resultado quando houver | Etapas 07, 11, 18, 19 e 21 |
| Labels de fase/status | Etapa 08 |
| Exibir selecoes com bandeira e nome | Etapas 09, 11, 18, 19 e 21 |
| Indicador `isBettingOpen` | Etapas 10, 11, 18, 19 e 21 |
| Card/lista de partidas | Etapas 11 e 13 |
| Loading, erro e vazio | Etapa 12 |
| Layout responsivo | Etapas 15 e 21 |
| Acessibilidade basica | Etapa 16 |
| Testes de helpers | Etapa 17 |
| Testes da tela | Etapa 18 |
| Validacao manual contra API | Etapa 19 |
| Documentacao | Etapa 20 |
| Validacao dos criterios de aceite originais | Etapa 21 |

