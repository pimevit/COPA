# Tarefa 12 - Divisao em etapas independentes

Fonte: `C:\Repo\bolao-copa-backlog.md`, secao **Tarefa 12 - Scaffold do frontend (Tailwind, router, API client, auth store)**.

Este arquivo quebra a Tarefa 12 em etapas pequenas, fechadas e implementaveis separadamente. Nenhuma etapa abaixo deve implementar telas de feature, login/cadastro completos, partidas, palpites, ranking, estatisticas, backend, endpoints ou regras de negocio.

## Escopo original da Tarefa 12

Preparar a base do frontend:

- Tailwind configurado.
- Estilo mobile-first.
- Tema escuro opcional.
- React Router com rotas publicas e protegidas.
- Cliente HTTP central.
- Cliente HTTP injeta JWT no header `Authorization: Bearer`.
- Cliente HTTP trata `401` limpando a sessao.
- Store de autenticacao com Zustand.
- Store guarda token e usuario.
- Persistencia de sessao em memoria/localStorage.
- TanStack Query configurado.

## Dependencia base

A Tarefa 12 depende da Tarefa 01 e do contrato de autenticacao da Tarefa 04.

Cada etapa abaixo deve ser independente das outras etapas deste arquivo, mas pode exigir que exista:

- app frontend Vite + React + TypeScript;
- `package.json` no `/frontend`;
- TypeScript configurado;
- contrato de auth definido pela API:
  - formato do retorno de `POST /auth/login`;
  - formato do usuario publico;
  - regra de token JWT;
  - comportamento de `401`.

Se esse contrato nao existir, a etapa deve registrar bloqueio objetivo ou criar apenas estrutura neutra, sem inventar tela de login/cadastro.

## Decisoes assumidas para a Tarefa 12

- Usar TypeScript estrito, se o projeto permitir.
- Usar Tailwind como camada principal de estilos.
- Mobile-first por padrao.
- Tema escuro pode ser preparado por classe (`dark`) sem criar tela de configuracao.
- Usar React Router para rotas publicas e protegidas.
- Rotas protegidas redirecionam para `/login` quando nao houver token.
- `/login` pode existir como rota/placeholder tecnico; tela real fica na Tarefa 13.
- Usar Zustand para estado de autenticacao.
- Persistencia pode usar localStorage, sabendo que isso e tradeoff simples para MVP; nao implementar refresh token nesta tarefa.
- Cliente HTTP pode ser `fetch` wrapper ou `axios`, mas deve ser centralizado.
- `API_BASE_URL` deve vir de variavel de ambiente do Vite.
- TanStack Query deve ficar disponivel para tarefas futuras, sem criar queries de feature agora.

## Regra de independencia

Cada etapa deve poder ser implementada sozinha, sem depender da execucao previa de outra etapa deste arquivo.

Para preservar essa independencia:

- A etapa deve inspecionar o estado atual antes de alterar arquivos.
- A etapa deve preservar configuracoes, providers e stores existentes quando forem compativeis.
- A etapa deve criar apenas o minimo necessario para seu proprio objetivo compilar.
- Se faltar scaffold do frontend, registrar bloqueio e nao implementar a Tarefa 01 dentro da Tarefa 12.
- A etapa nao deve criar telas de feature nem consumir endpoints de negocio.
- Validacoes globais da Tarefa 12 so devem ser exigidas quando todas as etapas forem executadas.

## Etapa 01 - Pre-validacao do scaffold frontend

**Objetivo:** Confirmar que existe base frontend para receber as configuracoes.

**Escopo:**

- Verificar se `/frontend` existe.
- Verificar se `package.json` existe.
- Verificar se Vite + React + TypeScript estao configurados.
- Verificar se `npm install`/instalacao equivalente e possivel.
- Verificar se `npm run dev` existe.

**Fora do escopo:**

- Criar app Vite.
- Configurar Tailwind.
- Configurar router.
- Criar auth store.
- Criar API client.

**Criterios de aceite:**

- Base frontend foi confirmada.
- Bloqueios foram registrados objetivamente, se houver.
- Nenhuma configuracao da Tarefa 12 foi criada.

**Prompt sugerido:**

```text
Valide somente a base do frontend para a Tarefa 12. Confira se /frontend existe, package.json existe, Vite + React + TypeScript estao configurados, instalacao de dependencias e possivel e npm run dev existe. Se faltar algo, registre bloqueio objetivamente. Nao crie app Vite, Tailwind, router, auth store ou API client.
```

## Etapa 02 - Dependencias base do frontend

**Objetivo:** Adicionar dependencias necessarias para a fundacao do frontend.

**Escopo:**

- Adicionar Tailwind e dependencias relacionadas.
- Adicionar React Router.
- Adicionar Zustand.
- Adicionar TanStack Query.
- Adicionar cliente HTTP, se for usar biblioteca como `axios`.
- Preservar gerenciador de pacotes existente.

**Fora do escopo:**

- Configurar cada biblioteca.
- Criar telas.
- Criar chamadas de API.
- Criar componentes de feature.

**Criterios de aceite:**

- Dependencias necessarias estao no `package.json`.
- Lockfile foi atualizado pelo gerenciador correto.
- Projeto ainda compila.

**Prompt sugerido:**

```text
Adicione somente as dependencias base da Tarefa 12 no /frontend: Tailwind e dependencias relacionadas, React Router, Zustand, TanStack Query e axios apenas se o projeto escolher axios em vez de fetch wrapper. Use o gerenciador de pacotes existente. Nao configure as bibliotecas, nao crie telas, chamadas de API ou componentes de feature.
```

## Etapa 03 - Configuracao do Tailwind

**Objetivo:** Habilitar Tailwind no projeto Vite.

**Escopo:**

- Criar ou ajustar `tailwind.config`.
- Criar ou ajustar `postcss.config`, se necessario.
- Configurar paths de conteudo.
- Importar diretivas Tailwind no CSS global.
- Garantir que estilos Tailwind sejam aplicados.

**Fora do escopo:**

- Criar design system completo.
- Criar telas de feature.
- Criar tema visual final.
- Adicionar biblioteca de componentes.

**Criterios de aceite:**

- Tailwind compila.
- Classes Tailwind aplicam estilo.
- App ainda sobe com `npm run dev`.

**Prompt sugerido:**

```text
Configure somente Tailwind no frontend Vite. Crie ou ajuste tailwind.config, postcss.config se necessario, paths de conteudo e CSS global com diretivas Tailwind. Garanta que classes Tailwind apliquem estilo. Nao crie design system completo, telas de feature, tema visual final ou biblioteca de componentes.
```

## Etapa 04 - Base de estilos mobile-first e tema escuro

**Objetivo:** Preparar estilos globais minimos e responsivos.

**Escopo:**

- Definir estilos globais basicos.
- Garantir mobile-first.
- Preparar suporte a tema escuro por classe ou configuracao Tailwind.
- Evitar layout final de feature.
- Manter tela inicial neutra.

**Fora do escopo:**

- Criar tela de login.
- Criar dashboard.
- Criar componentes complexos.
- Criar alternador de tema completo.

**Criterios de aceite:**

- Estilos globais nao quebram o app.
- Tema escuro fica tecnicamente preparado, se incluido.
- Nao ha tela de feature criada.
- Projeto compila.

**Prompt sugerido:**

```text
Prepare somente estilos globais minimos mobile-first e suporte tecnico a tema escuro no Tailwind, preferencialmente por classe dark. Mantenha a tela inicial neutra. Nao crie login, dashboard, componentes complexos, alternador de tema completo ou telas de feature.
```

## Etapa 05 - Estrutura de pastas do frontend

**Objetivo:** Organizar a base de codigo para as proximas telas.

**Escopo:**

- Criar estrutura minima para `app`, `routes`, `shared`, `api`, `stores` ou padrao equivalente do projeto.
- Mover arquivos existentes apenas quando necessario.
- Preservar imports funcionando.
- Manter estrutura simples.

**Fora do escopo:**

- Criar modulos de feature completos.
- Criar telas reais.
- Criar componentes visuais finais.
- Criar domain layer complexa no frontend.

**Criterios de aceite:**

- Estrutura de pastas esta clara.
- App compila apos reorganizacao.
- Nenhuma tela de feature foi implementada.

**Prompt sugerido:**

```text
Crie somente uma estrutura minima de pastas para o frontend, como app, routes, shared, api e stores, ou padrao equivalente ja usado no projeto. Preserve imports e mantenha simples. Nao crie modulos de feature completos, telas reais, componentes finais ou camada de dominio complexa.
```

## Etapa 06 - Configuracao do React Router

**Objetivo:** Habilitar roteamento base no frontend.

**Escopo:**

- Configurar `BrowserRouter` ou router equivalente.
- Criar mapa de rotas base.
- Criar rotas publicas e protegidas em estrutura.
- Criar placeholders tecnicos minimos para rotas quando necessario.
- Manter sem telas de feature.

**Fora do escopo:**

- Criar tela de login/cadastro real.
- Criar telas de partidas, palpites, ranking ou estatisticas.
- Integrar com API.
- Criar layout visual final.

**Criterios de aceite:**

- Router esta configurado.
- Rotas base renderizam sem erro.
- Projeto compila.

**Prompt sugerido:**

```text
Configure somente o React Router no frontend. Crie BrowserRouter ou equivalente, mapa de rotas base e separacao estrutural entre rotas publicas e protegidas, usando placeholders tecnicos minimos se necessario. Nao crie login/cadastro real, partidas, palpites, ranking, estatisticas, integracao com API ou layout visual final.
```

## Etapa 07 - Componente de rota protegida

**Objetivo:** Bloquear rotas privadas quando nao houver sessao.

**Escopo:**

- Criar `ProtectedRoute` ou equivalente.
- Ler estado de autenticacao.
- Redirecionar para `/login` quando nao houver token.
- Preservar rota de destino quando fizer sentido.
- Renderizar children/outlet quando autenticado.

**Fora do escopo:**

- Criar tela real de login.
- Validar token no servidor.
- Implementar roles.
- Criar permissoes por perfil.

**Criterios de aceite:**

- Sem token, rota protegida redireciona para `/login`.
- Com token, rota protegida renderiza conteudo.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente o componente ProtectedRoute ou equivalente. Ele deve ler o estado de autenticacao, redirecionar para /login quando nao houver token e renderizar children/outlet quando autenticado. Preserve rota de destino se o padrao do projeto permitir. Nao crie login real, validacao de token no servidor, roles ou permissoes por perfil.
```

## Etapa 08 - Modelo de sessao e usuario autenticado

**Objetivo:** Definir tipos TypeScript para a sessao do frontend.

**Escopo:**

- Criar tipo de usuario autenticado conforme contrato da Tarefa 04.
- Criar tipo de sessao.
- Criar tipo de resposta de login, se necessario.
- Evitar campos sensiveis como `passwordHash`.

**Fora do escopo:**

- Criar chamadas de login.
- Criar tela de login.
- Criar validacao de formulario.
- Criar refresh token.

**Criterios de aceite:**

- Tipos de auth existem.
- Tipos nao contem campos sensiveis.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente os tipos TypeScript de autenticacao do frontend: usuario autenticado conforme contrato da Tarefa 04, sessao e resposta de login se necessario. Nao inclua passwordHash ou campos sensiveis. Nao crie chamadas de login, tela de login, validacao de formulario ou refresh token.
```

## Etapa 09 - Store de autenticacao com Zustand

**Objetivo:** Centralizar estado de autenticacao no frontend.

**Escopo:**

- Criar store Zustand.
- Guardar token.
- Guardar usuario publico.
- Criar acoes para definir sessao.
- Criar acao para limpar sessao.
- Expor seletores simples.

**Fora do escopo:**

- Chamar endpoint de login.
- Criar formulario de login.
- Validar token no servidor.
- Implementar refresh token.

**Criterios de aceite:**

- Store de auth existe.
- Token e usuario podem ser definidos e limpos.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente a store de autenticacao com Zustand. Ela deve guardar token e usuario publico, expor acoes para definir sessao e limpar sessao, e seletores simples. Nao chame endpoint de login, nao crie formulario, nao valide token no servidor e nao implemente refresh token.
```

## Etapa 10 - Persistencia da sessao

**Objetivo:** Persistir sessao de forma simples para refresh da pagina.

**Escopo:**

- Configurar persistencia da store em localStorage ou mecanismo definido.
- Reidratar token e usuario ao iniciar app.
- Permitir limpar persistencia no logout.
- Evitar persistir dados sensiveis alem do token e usuario publico.

**Fora do escopo:**

- Implementar refresh token.
- Implementar cookie httpOnly.
- Criar logout visual.
- Criar tela de perfil.

**Criterios de aceite:**

- Sessao persiste apos refresh.
- Limpar sessao remove persistencia.
- Projeto compila.

**Prompt sugerido:**

```text
Configure somente a persistencia da store de auth, usando localStorage ou mecanismo ja definido. Reidrate token/usuario ao iniciar app e remova a persistencia ao limpar sessao. Nao implemente refresh token, cookie httpOnly, logout visual ou tela de perfil.
```

## Etapa 11 - Variaveis de ambiente do frontend

**Objetivo:** Definir configuracao da URL base da API.

**Escopo:**

- Criar ou documentar `VITE_API_BASE_URL`.
- Criar helper de leitura de env.
- Validar ausencia de configuracao com erro claro em desenvolvimento.
- Evitar URL hardcoded no cliente HTTP.

**Fora do escopo:**

- Configurar deploy.
- Criar chamadas de feature.
- Alterar backend.
- Criar secrets no repositorio.

**Criterios de aceite:**

- API base URL vem de env.
- Nao ha segredo real versionado.
- Projeto compila.

**Prompt sugerido:**

```text
Configure somente variaveis de ambiente do frontend para API base URL. Use VITE_API_BASE_URL, crie helper de leitura se necessario e evite URL hardcoded no cliente HTTP. Nao configure deploy, chamadas de feature, backend ou secrets reais no repositorio.
```

## Etapa 12 - Cliente HTTP central

**Objetivo:** Criar ponto unico para chamadas HTTP.

**Escopo:**

- Criar cliente HTTP central usando `fetch` wrapper ou `axios`.
- Configurar `baseURL` por env.
- Padronizar serializacao JSON.
- Padronizar tratamento basico de erro.
- Manter cliente desacoplado de telas.

**Fora do escopo:**

- Criar chamadas de endpoints especificos.
- Criar hooks de feature.
- Criar retry complexo.
- Criar interceptacao de refresh token.

**Criterios de aceite:**

- Existe cliente HTTP central.
- URL base vem de configuracao.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente o cliente HTTP central do frontend, usando fetch wrapper ou axios. Configure base URL por VITE_API_BASE_URL, JSON e tratamento basico de erro. Mantenha desacoplado de telas. Nao crie chamadas de endpoints especificos, hooks de feature, retry complexo ou refresh token.
```

## Etapa 13 - Injecao de Bearer token no cliente HTTP

**Objetivo:** Enviar JWT automaticamente em requisicoes autenticadas.

**Escopo:**

- Ler token da store de auth.
- Adicionar header `Authorization: Bearer <token>` quando houver token.
- Nao adicionar header quando nao houver token.
- Manter comportamento centralizado.

**Fora do escopo:**

- Implementar login.
- Renovar token.
- Criar chamadas de feature.
- Criar roles.

**Criterios de aceite:**

- Requisicoes com token enviam Bearer automaticamente.
- Requisicoes sem token nao enviam Bearer.
- Projeto compila.

**Prompt sugerido:**

```text
Configure somente a injecao automatica do Bearer token no cliente HTTP central. Leia o token da store de auth e envie Authorization: Bearer <token> quando existir; sem token, nao envie o header. Nao implemente login, renovacao de token, chamadas de feature ou roles.
```

## Etapa 14 - Tratamento centralizado de 401

**Objetivo:** Limpar sessao quando a API retornar nao autorizado.

**Escopo:**

- Detectar resposta HTTP `401` no cliente HTTP.
- Limpar store de autenticacao.
- Remover persistencia da sessao.
- Opcionalmente redirecionar para `/login` de forma segura.

**Fora do escopo:**

- Implementar refresh token.
- Implementar retry automatico de login.
- Criar tela de login.
- Criar notificacoes visuais finais.

**Criterios de aceite:**

- `401` limpa sessao.
- Persistencia tambem e removida.
- Projeto compila.

**Prompt sugerido:**

```text
Implemente somente o tratamento centralizado de 401 no cliente HTTP. Quando a API retornar 401, limpe a store de autenticacao e remova a persistencia; redirecione para /login apenas se isso estiver seguro no padrao do router. Nao implemente refresh token, retry de login, tela de login ou notificacoes finais.
```

## Etapa 15 - TanStack Query Provider

**Objetivo:** Disponibilizar TanStack Query para as futuras telas.

**Escopo:**

- Criar `QueryClient`.
- Configurar `QueryClientProvider`.
- Definir defaults basicos de retry/staleTime, se necessario.
- Integrar provider na raiz do app.
- Opcionalmente adicionar Devtools apenas em desenvolvimento.

**Fora do escopo:**

- Criar queries de feature.
- Criar mutations de login.
- Criar cache manual complexo.
- Criar telas.

**Criterios de aceite:**

- TanStack Query esta disponivel no app.
- App compila.
- Nenhuma feature query foi criada.

**Prompt sugerido:**

```text
Configure somente o TanStack Query Provider. Crie QueryClient, aplique QueryClientProvider na raiz, defina defaults basicos se necessario e adicione Devtools apenas em desenvolvimento se fizer sentido. Nao crie queries de feature, mutations de login, cache manual complexo ou telas.
```

## Etapa 16 - Composicao de providers da aplicacao

**Objetivo:** Organizar providers globais do frontend.

**Escopo:**

- Compor Router, QueryClientProvider e providers existentes.
- Garantir ordem correta.
- Manter `App` limpo.
- Preservar renderizacao inicial.

**Fora do escopo:**

- Criar layout final.
- Criar telas de feature.
- Criar providers de dominio ainda inexistentes.
- Criar theme switcher completo.

**Criterios de aceite:**

- Providers globais estao centralizados.
- App renderiza sem erro.
- Projeto compila.

**Prompt sugerido:**

```text
Organize somente a composicao de providers globais do frontend. Inclua Router, QueryClientProvider e providers existentes na ordem correta, mantendo App limpo e renderizacao inicial funcionando. Nao crie layout final, telas de feature, providers de dominio ou theme switcher completo.
```

## Etapa 17 - Rotas placeholder sem feature

**Objetivo:** Permitir validar roteamento sem construir telas futuras.

**Escopo:**

- Criar placeholders tecnicos minimos para rotas necessarias.
- Criar rota `/login` apenas como placeholder se a protecao precisar redirecionar.
- Criar uma rota protegida tecnica para validar `ProtectedRoute`, se necessario.
- Manter placeholders simples e temporarios.

**Fora do escopo:**

- Implementar login/cadastro.
- Implementar home real.
- Implementar partidas, palpites, ranking ou estatisticas.
- Criar design final.

**Criterios de aceite:**

- Rotas podem ser navegadas para validar router.
- Rota protegida redireciona sem token.
- Nenhuma tela de feature foi criada.

**Prompt sugerido:**

```text
Crie somente placeholders tecnicos minimos para validar rotas. Pode existir /login como placeholder e uma rota protegida tecnica para validar ProtectedRoute. Nao implemente login/cadastro, home real, partidas, palpites, ranking, estatisticas ou design final.
```

## Etapa 18 - Tipagem e padroes de erro do cliente HTTP

**Objetivo:** Padronizar tipos de erro consumidos por telas futuras.

**Escopo:**

- Definir tipo de erro da API.
- Suportar `ProblemDetails`, se backend ja usa esse padrao.
- Expor erro de forma previsivel para TanStack Query.
- Evitar acoplamento com uma feature especifica.

**Fora do escopo:**

- Criar mensagens visuais finais.
- Criar tratamento especifico de login.
- Criar tratamento especifico de bets.
- Criar telas.

**Criterios de aceite:**

- Erros HTTP tem tipo previsivel.
- `ProblemDetails` pode ser lido quando existir.
- Projeto compila.

**Prompt sugerido:**

```text
Padronize somente a tipagem de erros do cliente HTTP. Defina tipo de erro da API, suporte ProblemDetails se o backend ja usar esse padrao e exponha erros de forma previsivel para TanStack Query. Nao crie mensagens visuais finais, tratamento especifico de login/bets ou telas.
```

## Etapa 19 - Testes ou validacao tecnica do scaffold frontend

**Objetivo:** Verificar o comportamento base sem testar features inexistentes.

**Escopo:**

- Validar build do frontend.
- Validar que Tailwind aplica estilo.
- Validar router publico.
- Validar rota protegida sem token redirecionando para `/login`.
- Validar cliente HTTP injetando Bearer com token.
- Validar tratamento de `401` limpando sessao.
- Validar TanStack Query Provider disponivel.

**Fora do escopo:**

- Testar login real.
- Testar chamadas reais de feature.
- Testar UI final.
- Testar backend.

**Criterios de aceite:**

- Build passa.
- Comportamentos fundacionais foram validados.
- Pendencias foram registradas objetivamente, se houver.

**Prompt sugerido:**

```text
Valide somente o scaffold frontend da Tarefa 12. Rode build, confirme Tailwind aplicando estilo, router publico, rota protegida sem token redirecionando para /login, cliente HTTP injetando Bearer com token, 401 limpando sessao e TanStack Query Provider disponivel. Nao teste login real, chamadas de feature, UI final ou backend.
```

## Etapa 20 - Documentacao local do scaffold frontend

**Objetivo:** Documentar como usar a fundacao criada na Tarefa 12.

**Escopo:**

- Atualizar README ou documentacao frontend.
- Documentar comandos de instalacao/dev/build.
- Documentar `VITE_API_BASE_URL`.
- Documentar estrutura de rotas.
- Documentar auth store.
- Documentar cliente HTTP e tratamento de `401`.
- Documentar TanStack Query.
- Reforcar que telas de feature ficam para tarefas futuras.

**Fora do escopo:**

- Documentar login/cadastro como prontos.
- Documentar telas futuras como prontas.
- Documentar deploy.
- Criar guia visual completo.

**Criterios de aceite:**

- Documentacao permite rodar e entender o scaffold.
- Variaveis de ambiente estao claras.
- Escopo futuro esta claro.

**Prompt sugerido:**

```text
Atualize somente a documentacao local do frontend para a Tarefa 12. Documente comandos de instalacao/dev/build, VITE_API_BASE_URL, estrutura de rotas, auth store, cliente HTTP, tratamento de 401 e TanStack Query. Deixe claro que login/cadastro e telas de feature ficam para tarefas futuras. Nao documente deploy ou telas futuras como prontas.
```

## Etapa 21 - Validacao final da Tarefa 12

**Objetivo:** Verificar se o scaffold frontend atende aos criterios originais.

**Escopo:**

- Rodar instalacao de dependencias, se necessario.
- Rodar build/typecheck/lint disponiveis.
- Subir `npm run dev`.
- Confirmar que Tailwind aplica estilos.
- Confirmar rotas publicas e protegidas.
- Confirmar redirecionamento para `/login` sem token.
- Confirmar cliente HTTP enviando Bearer automaticamente com token.
- Confirmar `401` limpando sessao.
- Confirmar TanStack Query disponivel.

**Fora do escopo:**

- Criar telas de feature.
- Integrar login/cadastro real.
- Integrar partidas, palpites, ranking ou estatisticas.
- Alterar backend.
- Criar deploy.

**Criterios de aceite:**

- Tailwind aplica estilos.
- Rotas protegidas redirecionam para login quando sem token.
- Requisicoes autenticadas enviam header Bearer automaticamente.
- `401` limpa a sessao.
- TanStack Query esta disponivel para uso.
- Pendencias foram registradas objetivamente, se houver.

**Prompt sugerido:**

```text
Valide somente a Tarefa 12. Rode instalacao se necessario, build/typecheck/lint disponiveis, suba npm run dev, confirme Tailwind aplicando estilos, rotas publicas/protegidas, redirecionamento para /login sem token, cliente HTTP enviando Bearer com token, 401 limpando sessao e TanStack Query disponivel. Se algo falhar, registre pendencia objetivamente. Nao crie telas de feature, login/cadastro real, integracoes de partidas/palpites/ranking/estatisticas, backend ou deploy.
```

## Mapeamento do escopo original para as etapas

| Item da Tarefa 12 | Etapa responsavel |
|---|---|
| Validar dependencia T01 | Etapa 01 |
| Dependencias do frontend | Etapa 02 |
| Tailwind configurado | Etapas 03, 04, 19 e 21 |
| Mobile-first | Etapa 04 |
| Tema escuro opcional | Etapa 04 |
| Estrutura de pastas | Etapa 05 |
| React Router | Etapas 06, 16, 17 e 21 |
| Rotas publicas e protegidas | Etapas 06, 07, 17, 19 e 21 |
| Redirecionar sem token para login | Etapas 07, 17, 19 e 21 |
| Tipos de auth | Etapa 08 |
| Store Zustand | Etapa 09 |
| Persistencia de token/sessao | Etapa 10 |
| API base URL por env | Etapa 11 |
| Cliente HTTP central | Etapa 12 |
| Header Bearer automatico | Etapas 13, 19 e 21 |
| Tratar `401` limpando sessao | Etapas 14, 19 e 21 |
| TanStack Query | Etapas 15, 16, 19 e 21 |
| Tipagem de erros HTTP | Etapa 18 |
| Documentacao | Etapa 20 |
| Validacao dos criterios de aceite originais | Etapa 21 |

