# Tarefa Logout - Divisao em etapas independentes

Fonte: `C:\Users\felip\OneDrive\Documentos\ToruGol\bolao-copa-backlog.md`, secoes **Tarefa 12 - Scaffold do frontend (Tailwind, router, API client, auth store)** e **Tarefa 13 - Telas de autenticacao**.

Este arquivo quebra a implementacao de logout em etapas pequenas, fechadas e implementaveis separadamente. Nenhuma etapa abaixo deve implementar backend novo, refresh token, blacklist de JWT, recuperacao de senha, perfil de usuario, partidas, palpites, ranking, estatisticas ou redesign da aplicacao.

## Escopo da Tarefa Logout

Implementar logout no frontend usando a base de autenticacao ja existente:

- Acao visual de logout em area autenticada.
- Limpeza da sessao no auth store.
- Remocao da persistencia em `localStorage`.
- Limpeza ou invalidacao de dados sensiveis em cache do TanStack Query.
- Redirecionamento para `/login` ou rota publica definida.
- Bloqueio de retorno para rotas protegidas apos logout.
- Comportamento idempotente quando o usuario ja estiver sem sessao.

## Dependencia base

A Tarefa Logout depende das Tarefas 12 e 13.

Cada etapa abaixo deve ser independente das outras etapas deste arquivo, mas pode exigir que exista:

- frontend React + Vite + TypeScript;
- React Router configurado;
- rotas protegidas funcionando;
- auth store com token, usuario e acao para limpar sessao;
- persistencia de sessao em `localStorage`;
- TanStack Query configurado;
- telas de login/cadastro ja integradas.

Se essa base nao existir, a etapa deve registrar bloqueio objetivo e nao deve implementar T12 ou T13 dentro desta tarefa.

## Decisoes assumidas para a Tarefa Logout

- O logout do MVP e client-side, pois o backend usa JWT stateless.
- Nao criar endpoint `POST /auth/logout` nesta tarefa.
- Nao implementar invalidacao server-side de token.
- Nao implementar refresh token.
- Nao implementar blacklist de JWT.
- Limpar a sessao deve remover token, usuario e expiracao do auth store.
- Limpar a sessao deve remover a chave persistida `bolao-copa-auth` do `localStorage`, se esse for o storage configurado.
- Apos logout, o usuario deve ir para `/login` ou para a rota publica padrao do projeto.
- Apos logout, tentar acessar `/app`, `/matches`, `/ranking` ou outra rota protegida deve redirecionar para `/login`.
- Caches de dados autenticados devem ser limpos ou invalidados para evitar exibir informacoes da sessao anterior.
- O logout deve ser seguro para execucao repetida, sem erro quando nao houver sessao ativa.

## Regra de independencia

Cada etapa deve poder ser implementada sozinha, sem depender da execucao previa de outra etapa deste arquivo.

Para preservar essa independencia:

- A etapa deve inspecionar o estado atual antes de alterar arquivos.
- A etapa deve preservar rotas, stores, hooks, layouts e componentes existentes quando forem compativeis.
- A etapa deve criar apenas o minimo necessario para seu proprio objetivo compilar.
- Se faltar base de autenticacao frontend, registrar bloqueio e parar.
- A etapa nao deve criar novas regras de negocio no backend.
- A etapa nao deve alterar fluxos de login/cadastro alem do necessario para coexistir com logout.
- Validacoes globais da Tarefa Logout so devem ser exigidas quando todas as etapas forem executadas.

## Etapa 01 - Pre-validacao da base de logout

**Objetivo:** Confirmar que o frontend possui a base necessaria para implementar logout.

**Escopo:**

- Verificar se o frontend compila.
- Verificar se React Router existe.
- Verificar se existem rotas protegidas.
- Verificar se o auth store existe.
- Verificar se o auth store possui acao para limpar sessao ou comportamento equivalente.
- Verificar se a sessao e persistida em `localStorage`.
- Verificar se TanStack Query esta configurado.

**Fora do escopo:**

- Criar botao de logout.
- Alterar auth store.
- Alterar rotas.
- Criar testes.
- Alterar backend.

**Criterios de aceite:**

- Dependencias minimas foram confirmadas.
- Bloqueios foram registrados objetivamente, se houver.
- Nenhuma funcionalidade de logout foi implementada.

**Prompt sugerido:**

```text
Valide somente a base para implementar logout no frontend. Confira build, React Router, rotas protegidas, auth store, acao para limpar sessao ou equivalente, persistencia em localStorage e TanStack Query. Se faltar algo, registre bloqueio objetivamente. Nao crie botao de logout, nao altere store, rotas, testes ou backend.
```

## Etapa 02 - Contrato da acao de logout no auth store

**Objetivo:** Garantir uma acao unica e idempotente para limpar a sessao.

**Escopo:**

- Reutilizar acao existente de limpeza de sessao, se houver.
- Criar ou ajustar acao de logout/limpeza somente se nao existir comportamento equivalente.
- Limpar token.
- Limpar usuario.
- Limpar expiracao da sessao.
- Remover persistencia da sessao.
- Manter a acao segura para chamadas repetidas.

**Fora do escopo:**

- Criar UI de logout.
- Redirecionar rotas.
- Limpar cache do TanStack Query.
- Criar endpoint de backend.
- Alterar login/cadastro.

**Criterios de aceite:**

- Existe uma acao central para limpar sessao.
- A acao remove dados persistidos.
- Chamar a acao sem sessao ativa nao gera erro.
- Projeto compila.

**Prompt sugerido:**

```text
Garanta somente o contrato da acao de logout no auth store. Reutilize a acao existente de limpar sessao se ela ja atender; caso contrario, ajuste o minimo para limpar token, usuario, expiracao e persistencia. A acao deve ser idempotente. Nao crie UI, redirecionamento, limpeza de cache, endpoint backend ou alteracoes em login/cadastro.
```

## Etapa 03 - Helper ou hook de logout

**Objetivo:** Centralizar o fluxo de logout usado pela interface.

**Escopo:**

- Criar helper/hook simples para executar logout, se o projeto usar esse padrao.
- Chamar a acao de limpeza do auth store.
- Expor uma funcao clara para componentes.
- Manter o helper desacoplado de layout especifico.

**Fora do escopo:**

- Criar botao visual.
- Redirecionar automaticamente, se isso ficar em etapa separada.
- Limpar cache do TanStack Query, se isso ficar em etapa separada.
- Criar modal de confirmacao.
- Alterar backend.

**Criterios de aceite:**

- Componentes conseguem acionar logout por uma funcao central.
- O helper nao duplica regra de limpeza de sessao.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente um helper ou hook simples para logout, seguindo o padrao existente do frontend. Ele deve chamar a acao central de limpeza de sessao e expor uma funcao para componentes, sem duplicar regra. Nao crie botao visual, redirecionamento automatico, limpeza de cache, modal de confirmacao ou backend.
```

## Etapa 04 - Limpeza de cache autenticado

**Objetivo:** Evitar que dados da sessao anterior fiquem visiveis apos logout.

**Escopo:**

- Identificar uso atual de TanStack Query em telas autenticadas.
- Limpar ou invalidar queries autenticadas ao executar logout.
- Preferir `queryClient.clear()` ou invalidacao seletiva conforme padrao existente.
- Nao apagar estado de UI que nao seja sensivel se o projeto ja tiver separacao clara.

**Fora do escopo:**

- Alterar chamadas de API.
- Criar novas queries.
- Alterar regras de cache de features.
- Criar backend.
- Criar UI de logout.

**Criterios de aceite:**

- Dados autenticados nao permanecem visiveis apos logout.
- O fluxo nao quebra login posterior.
- Projeto compila.

**Prompt sugerido:**

```text
Implemente somente a limpeza de cache autenticado no logout. Identifique as queries usadas em telas autenticadas e limpe ou invalide o cache com o QueryClient conforme o padrao existente. Garanta que dados da sessao anterior nao fiquem visiveis apos logout e que login posterior continue funcionando. Nao altere chamadas de API, nao crie queries, backend ou UI de logout.
```

## Etapa 05 - Redirecionamento apos logout

**Objetivo:** Enviar o usuario para uma rota publica apos encerrar a sessao.

**Escopo:**

- Redirecionar para `/login` ou rota publica padrao apos logout.
- Evitar loop de redirecionamento.
- Preservar comportamento das rotas protegidas existentes.
- Garantir que o historico do navegador nao permita reabrir tela protegida com sessao antiga.

**Fora do escopo:**

- Criar tela de login.
- Alterar validacao de login.
- Criar modal de confirmacao.
- Alterar backend.
- Criar dashboard.

**Criterios de aceite:**

- Logout leva o usuario para rota publica definida.
- Voltar no navegador nao exibe rota protegida autenticada sem token.
- Projeto compila.

**Prompt sugerido:**

```text
Implemente somente o redirecionamento apos logout. Apos limpar a sessao, envie o usuario para /login ou rota publica padrao, evitando loops. Garanta que voltar no navegador nao reabra rota protegida autenticada sem token. Nao crie tela de login, modal, backend, dashboard ou alteracoes de validacao.
```

## Etapa 06 - Acao visual de logout na area autenticada

**Objetivo:** Disponibilizar um controle claro de logout para usuario autenticado.

**Escopo:**

- Adicionar botao ou item de menu de logout em layout/area autenticada existente.
- Exibir o controle apenas quando houver sessao autenticada.
- Usar texto claro como `Sair` ou `Logout`, conforme padrao do projeto.
- Garantir estado de foco e acessibilidade basica.
- Manter responsividade mobile-first.

**Fora do escopo:**

- Redesenhar o layout inteiro.
- Criar menu de perfil completo.
- Criar pagina de perfil.
- Criar confirmacao obrigatoria.
- Alterar backend.

**Criterios de aceite:**

- Usuario autenticado ve uma acao de logout.
- Usuario sem sessao nao ve acao autenticada indevida.
- Controle funciona por mouse e teclado.
- Texto e controles cabem em mobile.
- Projeto compila.

**Prompt sugerido:**

```text
Adicione somente uma acao visual de logout na area autenticada existente. Mostre o controle apenas com sessao ativa, use texto claro como Sair ou Logout, garanta foco/acessibilidade basica e responsividade mobile-first. Nao redesenhe o layout inteiro, nao crie menu de perfil completo, pagina de perfil, confirmacao obrigatoria ou backend.
```

## Etapa 07 - Integracao do clique de logout

**Objetivo:** Conectar a acao visual ao fluxo real de logout.

**Escopo:**

- Conectar o botao/item de logout ao helper ou acao central.
- Limpar sessao.
- Limpar cache autenticado, se a etapa correspondente ja estiver disponivel.
- Redirecionar para rota publica definida.
- Tratar chamadas repetidas sem erro.

**Fora do escopo:**

- Criar endpoint de logout.
- Criar confirmacao complexa.
- Alterar login/cadastro.
- Alterar telas de partidas, palpites, ranking ou estatisticas.

**Criterios de aceite:**

- Clique em logout encerra a sessao.
- Persistencia local e removida.
- Usuario e redirecionado.
- Executar logout mais de uma vez nao gera erro.
- Projeto compila.

**Prompt sugerido:**

```text
Conecte somente a acao visual de logout ao fluxo real. Ao clicar, limpe a sessao, remova persistencia, limpe cache autenticado se ja existir esse helper e redirecione para a rota publica definida. O fluxo deve ser idempotente. Nao crie endpoint de logout, confirmacao complexa, alteracoes de login/cadastro ou features de partidas/palpites/ranking/estatisticas.
```

## Etapa 08 - Comportamento de rotas protegidas apos logout

**Objetivo:** Confirmar que o roteamento bloqueia acesso autenticado depois da limpeza da sessao.

**Escopo:**

- Verificar `/app` apos logout.
- Verificar `/matches` apos logout, se existir.
- Verificar `/ranking` apos logout, se existir como rota protegida.
- Garantir redirecionamento para `/login` sem token.
- Ajustar apenas o necessario no guard de rota se houver falha.

**Fora do escopo:**

- Alterar conteudo das telas protegidas.
- Criar novas rotas.
- Alterar API.
- Alterar login/cadastro.
- Criar regras de permissao por role.

**Criterios de aceite:**

- Rotas protegidas nao renderizam conteudo autenticado apos logout.
- Usuario sem token e redirecionado para `/login`.
- Projeto compila.

**Prompt sugerido:**

```text
Valide e ajuste somente o comportamento de rotas protegidas apos logout. Verifique /app, /matches e /ranking quando existirem; sem token, devem redirecionar para /login e nao renderizar conteudo autenticado. Nao altere conteudo das telas, nao crie rotas, API, login/cadastro ou permissoes por role.
```

## Etapa 09 - Testes automatizados do logout

**Objetivo:** Cobrir o fluxo principal de logout sem depender de backend real.

**Escopo:**

- Testar limpeza do auth store.
- Testar remocao da chave persistida.
- Testar clique no controle de logout.
- Testar redirecionamento para rota publica.
- Testar bloqueio de rota protegida apos logout.
- Testar que cache autenticado e limpo ou invalidado, se houver helper implementado.

**Fora do escopo:**

- Teste E2E contra backend real.
- Testar login completo novamente.
- Testar partidas, palpites, ranking ou estatisticas em detalhe.
- Testar backend.

**Criterios de aceite:**

- Fluxo principal de logout esta coberto.
- Testes nao dependem da API real.
- Testes passam.

**Prompt sugerido:**

```text
Crie somente testes automatizados do logout no frontend com mocks. Cubra limpeza do auth store, remocao da chave persistida, clique no controle de logout, redirecionamento para rota publica, bloqueio de rota protegida apos logout e limpeza/invalidation de cache autenticado se existir. Nao crie E2E contra backend real, testes de backend ou testes detalhados de partidas/palpites/ranking/estatisticas.
```

## Etapa 10 - Validacao manual do logout

**Objetivo:** Confirmar o comportamento do logout no navegador.

**Escopo:**

- Subir frontend.
- Entrar com usuario autenticado ou simular sessao valida conforme padrao de teste do projeto.
- Acionar logout.
- Confirmar remocao de `bolao-copa-auth` do `localStorage`.
- Confirmar redirecionamento para `/login`.
- Confirmar que refresh nao restaura sessao.
- Confirmar que acessar rota protegida redireciona para `/login`.

**Fora do escopo:**

- Testar cadastro completo.
- Testar endpoints de negocio.
- Testar deploy.
- Criar dados novos no backend sem necessidade.

**Criterios de aceite:**

- Logout funciona no navegador.
- Sessao nao volta apos refresh.
- Rotas protegidas ficam bloqueadas sem token.
- Pendencias foram registradas objetivamente, se houver.

**Prompt sugerido:**

```text
Valide somente o logout no navegador. Suba o frontend, entre com usuario autenticado ou simule sessao valida conforme o padrao do projeto, acione logout, confirme remocao de bolao-copa-auth do localStorage, redirecionamento para /login, refresh sem restaurar sessao e bloqueio de rotas protegidas sem token. Nao teste cadastro completo, endpoints de negocio ou deploy.
```

## Etapa 11 - Documentacao local do logout

**Objetivo:** Documentar como o logout funciona e como validar localmente.

**Escopo:**

- Atualizar README ou documentacao frontend.
- Documentar que logout e client-side no MVP.
- Documentar que nao existe invalidacao server-side de JWT nesta tarefa.
- Documentar que `bolao-copa-auth` e removido do `localStorage`.
- Documentar destino apos logout.
- Documentar validacao manual basica.

**Fora do escopo:**

- Documentar refresh token como implementado.
- Documentar blacklist de JWT como implementada.
- Documentar endpoint inexistente.
- Criar guia extenso de seguranca.

**Criterios de aceite:**

- Documentacao explica o comportamento real.
- Escopo fora da tarefa esta claro.
- Validacao manual pode ser repetida por outra pessoa.

**Prompt sugerido:**

```text
Atualize somente a documentacao local do logout. Documente que o logout do MVP e client-side, que nao ha invalidacao server-side de JWT nesta tarefa, que a chave bolao-copa-auth e removida do localStorage, o destino apos logout e a validacao manual basica. Nao documente refresh token, blacklist ou endpoint inexistente como implementados.
```

## Etapa 12 - Validacao final da Tarefa Logout

**Objetivo:** Verificar se o conjunto atual atende aos criterios do logout.

**Escopo:**

- Rodar build/typecheck/lint/testes disponiveis do frontend.
- Subir frontend.
- Validar usuario autenticado vendo acao de logout.
- Acionar logout.
- Confirmar limpeza de auth store.
- Confirmar remocao de `bolao-copa-auth` do `localStorage`.
- Confirmar limpeza ou invalidacao de cache autenticado.
- Confirmar redirecionamento para rota publica.
- Confirmar bloqueio de rotas protegidas apos logout.
- Confirmar responsividade basica do controle de logout.

**Fora do escopo:**

- Criar endpoint de logout.
- Implementar refresh token.
- Implementar blacklist de JWT.
- Alterar backend.
- Testar deploy.
- Implementar novas telas de feature.

**Criterios de aceite:**

- Logout encerra a sessao no frontend.
- Persistencia local e removida.
- Dados autenticados da sessao anterior nao ficam visiveis.
- Usuario e redirecionado para rota publica.
- Rotas protegidas exigem login novamente.
- Pendencias foram registradas objetivamente, se houver.

**Prompt sugerido:**

```text
Valide somente a Tarefa Logout. Rode build/typecheck/lint/testes disponiveis do frontend, suba o frontend, confirme usuario autenticado vendo a acao de logout, acione logout, confirme limpeza do auth store, remocao de bolao-copa-auth do localStorage, limpeza ou invalidacao de cache autenticado, redirecionamento para rota publica, bloqueio de rotas protegidas e responsividade basica. Se algo falhar, registre pendencia objetivamente. Nao crie endpoint de logout, refresh token, blacklist, backend, deploy ou novas telas de feature.
```

## Mapeamento do escopo para as etapas

| Item da Tarefa Logout | Etapa responsavel |
|---|---|
| Validar base T12/T13 | Etapa 01 |
| Acao central para limpar sessao | Etapa 02 |
| Helper/hook de logout | Etapa 03 |
| Limpeza de cache autenticado | Etapa 04 |
| Redirecionamento apos logout | Etapa 05 |
| Acao visual de logout | Etapa 06 |
| Clique executando logout real | Etapa 07 |
| Rotas protegidas apos logout | Etapa 08 |
| Testes automatizados | Etapa 09 |
| Validacao manual no navegador | Etapa 10 |
| Documentacao | Etapa 11 |
| Validacao final dos criterios de aceite | Etapa 12 |
