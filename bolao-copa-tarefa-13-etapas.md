# Tarefa 13 - Divisao em etapas independentes

Fonte: `C:\Repo\bolao-copa-backlog.md`, secao **Tarefa 13 - Telas de autenticacao**.

Este arquivo quebra a Tarefa 13 em etapas pequenas, fechadas e implementaveis separadamente. Nenhuma etapa abaixo deve implementar recuperacao de senha, telas de partidas, palpites, ranking, estatisticas, backend, endpoints novos ou regras de negocio fora de autenticacao.

## Escopo original da Tarefa 13

Implementar login e cadastro consumindo a API:

- Tela de login.
- Tela de cadastro.
- Validacao de formulario.
- Feedback de erro.
- Integracao com `POST /auth/login`.
- Integracao com `POST /auth/register`.
- Uso de TanStack Query.
- Persistencia de sessao.
- Redirecionamento pos-login.
- Exibicao clara de erros:
  - e-mail duplicado;
  - credencial invalida.

## Dependencia base

A Tarefa 13 depende das Tarefas 12 e 04.

Cada etapa abaixo deve ser independente das outras etapas deste arquivo, mas pode exigir que exista:

- frontend React + Vite + TypeScript;
- Tailwind configurado;
- React Router configurado;
- rotas publicas e protegidas;
- cliente HTTP central;
- auth store com persistencia;
- TanStack Query configurado;
- contrato da API de auth:
  - `POST /auth/login`;
  - `POST /auth/register`;
  - formato da resposta de token;
  - formato do usuario publico;
  - formato dos erros.

Se esse contrato nao existir, a etapa deve registrar bloqueio objetivo e nao deve implementar T04 ou T12 dentro da Tarefa 13.

## Decisoes assumidas para a Tarefa 13

- Login e cadastro sao rotas publicas.
- Usuario autenticado deve ser redirecionado para a home ou rota original pretendida.
- Se ja houver token valido no store, acessar `/login` ou `/register` pode redirecionar para a area protegida.
- Sessao deve ser salva no auth store existente.
- Persistencia da sessao deve usar o mecanismo definido na Tarefa 12.
- Erros da API devem ser exibidos em linguagem clara, sem expor stack trace ou detalhes tecnicos.
- `401` no login deve ser mostrado como credenciais invalidas.
- E-mail duplicado no cadastro deve ser mostrado como e-mail ja cadastrado.
- Formularios devem validar campos obrigatorios antes de chamar a API.
- Nao implementar recuperacao de senha nesta tarefa.
- Nao criar tela final de dashboard; usar rota existente ou placeholder tecnico da Tarefa 12 para redirecionamento.

## Regra de independencia

Cada etapa deve poder ser implementada sozinha, sem depender da execucao previa de outra etapa deste arquivo.

Para preservar essa independencia:

- A etapa deve inspecionar o estado atual antes de alterar arquivos.
- A etapa deve preservar componentes, rotas, stores e hooks existentes quando forem compativeis.
- A etapa deve criar apenas o minimo necessario para seu proprio objetivo compilar.
- Se faltar base da Tarefa 12 ou contrato da Tarefa 04, registrar bloqueio e parar.
- A etapa nao deve criar telas ou chamadas de features fora de autenticacao.
- Validacoes globais da Tarefa 13 so devem ser exigidas quando todas as etapas forem executadas.

## Etapa 01 - Pre-validacao das dependencias de autenticacao frontend

**Objetivo:** Confirmar que o frontend possui base para implementar login e cadastro.

**Escopo:**

- Verificar se o frontend compila.
- Verificar se Tailwind esta configurado.
- Verificar se React Router existe.
- Verificar se auth store existe.
- Verificar se cliente HTTP central existe.
- Verificar se TanStack Query esta configurado.
- Verificar se contrato de `POST /auth/login` e `POST /auth/register` esta documentado ou implementado na API.

**Fora do escopo:**

- Criar telas.
- Criar chamadas de API.
- Criar auth store.
- Configurar TanStack Query.
- Implementar backend.

**Criterios de aceite:**

- Dependencias minimas foram confirmadas.
- Bloqueios foram registrados objetivamente, se houver.
- Nenhuma tela de autenticacao foi criada.

**Prompt sugerido:**

```text
Valide somente as dependencias da Tarefa 13. Confira build do frontend, Tailwind, React Router, auth store, cliente HTTP central, TanStack Query e contrato de POST /auth/login e POST /auth/register. Se faltar algo, registre bloqueio objetivamente. Nao crie telas, chamadas de API, store, configuracao de Query ou backend.
```

## Etapa 02 - Tipos TypeScript de requests e responses de auth

**Objetivo:** Definir contratos tipados para login e cadastro.

**Escopo:**

- Criar tipo de request de login.
- Criar tipo de request de cadastro.
- Criar tipo de response de autenticacao.
- Criar tipo de usuario publico, se ainda nao existir.
- Garantir que `PasswordHash` nao aparece nos tipos de frontend.

**Fora do escopo:**

- Criar chamadas HTTP.
- Criar forms.
- Criar telas.
- Criar store.

**Criterios de aceite:**

- Tipos de auth existem.
- Tipos refletem o contrato da API.
- Nenhum tipo expoe campo sensivel.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente os tipos TypeScript de auth para a Tarefa 13: LoginRequest, RegisterRequest, AuthResponse e User publico se ainda nao existir. Reflita o contrato da API e nao inclua PasswordHash ou campos sensiveis. Nao crie chamadas HTTP, forms, telas ou store.
```

## Etapa 03 - Funcoes de API para login e cadastro

**Objetivo:** Criar funcoes isoladas para chamar a API de autenticacao.

**Escopo:**

- Criar funcao `login`.
- Criar funcao `register`.
- Usar cliente HTTP central.
- Tipar requests e responses.
- Nao acessar diretamente componentes ou store dentro das funcoes, salvo padrao existente.

**Fora do escopo:**

- Criar mutations.
- Criar telas.
- Salvar sessao.
- Tratar redirecionamento.

**Criterios de aceite:**

- Funcoes chamam `POST /auth/login` e `POST /auth/register`.
- Funcoes usam cliente HTTP central.
- Funcoes sao tipadas.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente as funcoes de API de autenticacao no frontend: login chamando POST /auth/login e register chamando POST /auth/register, usando o cliente HTTP central e tipos TypeScript. Nao crie mutations, telas, salvamento de sessao ou redirecionamento.
```

## Etapa 04 - Mapeamento de erros de autenticacao

**Objetivo:** Padronizar mensagens de erro exibidas nas telas.

**Escopo:**

- Mapear `401` de login para credenciais invalidas.
- Mapear e-mail duplicado no cadastro para e-mail ja cadastrado.
- Mapear erro de validacao da API para campos ou mensagem geral.
- Criar helper de erro reutilizavel para auth.
- Evitar expor mensagens tecnicas cruas.

**Fora do escopo:**

- Criar UI de erro final.
- Criar telas.
- Alterar backend.
- Criar tratamento global novo.

**Criterios de aceite:**

- Erros comuns de auth possuem mensagens claras.
- Helper nao vaza detalhes tecnicos.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente um mapeamento/helper de erros para autenticacao. Login com 401 deve virar mensagem de credenciais invalidas; cadastro com email duplicado deve virar email ja cadastrado; erros de validacao devem virar mensagem de campo ou geral. Nao crie telas, UI final, backend ou tratamento global novo.
```

## Etapa 05 - Validacao do formulario de login

**Objetivo:** Definir validacao local para entrada de login.

**Escopo:**

- Validar e-mail obrigatorio.
- Validar formato basico de e-mail.
- Validar senha obrigatoria.
- Retornar mensagens claras.
- Manter validacao simples.

**Fora do escopo:**

- Criar tela completa.
- Chamar API.
- Salvar sessao.
- Criar recuperacao de senha.

**Criterios de aceite:**

- Campos invalidos impedem submit.
- Mensagens sao claras.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente a validacao local do formulario de login. E-mail deve ser obrigatorio e ter formato basico valido; senha deve ser obrigatoria; mensagens devem ser claras. Nao crie tela completa, chamada de API, salvamento de sessao ou recuperacao de senha.
```

## Etapa 06 - Validacao do formulario de cadastro

**Objetivo:** Definir validacao local para entrada de cadastro.

**Escopo:**

- Validar nome obrigatorio.
- Validar e-mail obrigatorio.
- Validar formato basico de e-mail.
- Validar senha obrigatoria.
- Definir regra minima de tamanho de senha, se o contrato da API exigir.
- Retornar mensagens claras.

**Fora do escopo:**

- Criar tela completa.
- Chamar API.
- Salvar sessao.
- Criar verificacao de e-mail.
- Criar recuperacao de senha.

**Criterios de aceite:**

- Campos invalidos impedem submit.
- Mensagens sao claras.
- Regras nao contradizem a API.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente a validacao local do formulario de cadastro. Nome, email e senha devem ser obrigatorios; email deve ter formato basico valido; senha deve seguir a regra minima exigida pela API, se houver. Nao crie tela completa, chamada de API, salvamento de sessao, verificacao de email ou recuperacao de senha.
```

## Etapa 07 - Componentes base de formulario de auth

**Objetivo:** Criar componentes pequenos e reutilizaveis para login/cadastro.

**Escopo:**

- Criar componentes simples para campo de texto/senha, erro de campo e submit button, se ainda nao existirem.
- Aplicar Tailwind mobile-first.
- Garantir estados de disabled/loading.
- Manter componentes genericos o suficiente para auth.

**Fora do escopo:**

- Criar design system completo.
- Criar telas de feature.
- Criar componentes de partidas/ranking.
- Criar biblioteca visual externa.

**Criterios de aceite:**

- Componentes compila e sao reutilizaveis.
- Texto cabe nos containers em mobile.
- Estados basicos existem.

**Prompt sugerido:**

```text
Crie somente componentes base simples para formularios de autenticacao, como input de texto/senha, erro de campo e submit button, usando Tailwind mobile-first e estados disabled/loading. Nao crie design system completo, telas de feature, componentes de partidas/ranking ou biblioteca visual externa.
```

## Etapa 08 - Layout visual das telas de autenticacao

**Objetivo:** Preparar a estrutura visual comum para login e cadastro.

**Escopo:**

- Criar layout simples para telas publicas de auth.
- Garantir responsividade mobile-first.
- Evitar marketing/landing page.
- Incluir area para formulario e feedback.
- Manter estilo consistente com scaffold.

**Fora do escopo:**

- Criar hero ou landing page.
- Criar dashboard.
- Criar telas protegidas.
- Criar fluxo de recuperacao de senha.

**Criterios de aceite:**

- Layout funciona em mobile e desktop.
- Nao ha sobreposicao de textos/controles.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente o layout visual comum das telas de autenticacao. Ele deve ser simples, responsivo, mobile-first e conter area para formulario e feedback. Nao crie landing page, hero, dashboard, telas protegidas ou recuperacao de senha.
```

## Etapa 09 - Mutation de login com TanStack Query

**Objetivo:** Conectar login ao fluxo de mutation.

**Escopo:**

- Criar hook/mutation para login.
- Usar funcao de API `login`.
- Expor loading, sucesso e erro.
- Nao salvar sessao diretamente se essa decisao ficar em etapa separada.

**Fora do escopo:**

- Criar tela.
- Criar cadastro.
- Redirecionar usuario.
- Criar refresh token.

**Criterios de aceite:**

- Mutation de login existe.
- Estados de loading/erro/sucesso sao acessiveis.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente a mutation de login com TanStack Query. Use a funcao de API login e exponha loading, sucesso e erro. Nao crie tela, cadastro, redirecionamento, refresh token ou salvamento de sessao se isso estiver separado.
```

## Etapa 10 - Mutation de cadastro com TanStack Query

**Objetivo:** Conectar cadastro ao fluxo de mutation.

**Escopo:**

- Criar hook/mutation para cadastro.
- Usar funcao de API `register`.
- Expor loading, sucesso e erro.
- Definir se cadastro tambem autentica automaticamente conforme contrato da API.

**Fora do escopo:**

- Criar tela.
- Criar login.
- Criar verificacao de e-mail.
- Criar recuperacao de senha.

**Criterios de aceite:**

- Mutation de cadastro existe.
- Estados de loading/erro/sucesso sao acessiveis.
- Projeto compila.

**Prompt sugerido:**

```text
Crie somente a mutation de cadastro com TanStack Query. Use a funcao de API register, exponha loading, sucesso e erro, e documente se o cadastro autentica automaticamente conforme resposta da API. Nao crie tela, login, verificacao de email ou recuperacao de senha.
```

## Etapa 11 - Salvamento de sessao apos autenticacao

**Objetivo:** Atualizar auth store quando login/cadastro retornar sessao.

**Escopo:**

- Receber resposta de auth.
- Salvar token no auth store.
- Salvar usuario publico no auth store.
- Acionar persistencia existente.
- Garantir que dados sensiveis nao sejam salvos.

**Fora do escopo:**

- Criar auth store.
- Criar tela.
- Criar refresh token.
- Validar token no servidor.

**Criterios de aceite:**

- Sessao e salva no store.
- Sessao persiste conforme Tarefa 12.
- `PasswordHash` ou dados sensiveis nao sao salvos.
- Projeto compila.

**Prompt sugerido:**

```text
Implemente somente o salvamento de sessao apos autenticacao. Ao receber AuthResponse, salve token e usuario publico no auth store e use a persistencia existente. Nao salve PasswordHash ou dados sensiveis. Nao crie auth store, tela, refresh token ou validacao de token no servidor.
```

## Etapa 12 - Redirecionamento pos-login

**Objetivo:** Definir destino apos autenticacao bem-sucedida.

**Escopo:**

- Redirecionar para rota originalmente solicitada quando existir.
- Caso contrario, redirecionar para home/rota protegida padrao.
- Evitar loop entre `/login` e rota protegida.
- Manter compatibilidade com React Router.

**Fora do escopo:**

- Criar home real.
- Criar dashboard.
- Criar fluxo de logout.
- Criar tela de erro global.

**Criterios de aceite:**

- Login bem-sucedido redireciona corretamente.
- Rota original e respeitada quando houver.
- Nao ha loop de redirecionamento.
- Projeto compila.

**Prompt sugerido:**

```text
Implemente somente o redirecionamento pos-login. Apos autenticacao bem-sucedida, redirecione para a rota originalmente solicitada quando existir; caso contrario, para a home/rota protegida padrao. Evite loop com /login. Nao crie home real, dashboard, logout ou tela de erro global.
```

## Etapa 13 - Tela de login

**Objetivo:** Implementar a tela de login consumindo API.

**Escopo:**

- Criar rota/tela `/login`.
- Renderizar formulario de e-mail e senha.
- Aplicar validacao local.
- Usar mutation de login.
- Exibir erro de credenciais invalidas.
- Exibir loading durante submit.
- Salvar sessao em sucesso.
- Redirecionar apos sucesso.
- Linkar para cadastro.

**Fora do escopo:**

- Recuperacao de senha.
- Cadastro na mesma etapa.
- Social login.
- MFA.
- Dashboard real.

**Criterios de aceite:**

- Login funciona contra API.
- Credenciais invalidas exibem erro claro.
- Sessao e salva apos sucesso.
- Usuario e redirecionado apos sucesso.

**Prompt sugerido:**

```text
Implemente somente a tela /login. Ela deve ter formulario de email/senha, validacao local, mutation de login, loading, erro claro para credenciais invalidas, salvar sessao em sucesso, redirecionar apos sucesso e link para cadastro. Nao implemente recuperacao de senha, cadastro nesta etapa, social login, MFA ou dashboard real.
```

## Etapa 14 - Tela de cadastro

**Objetivo:** Implementar a tela de cadastro consumindo API.

**Escopo:**

- Criar rota/tela `/register`.
- Renderizar formulario de nome, e-mail e senha.
- Aplicar validacao local.
- Usar mutation de cadastro.
- Exibir erro de e-mail duplicado.
- Exibir loading durante submit.
- Salvar sessao e redirecionar se a API retornar token.
- Se a API nao autenticar no cadastro, redirecionar para login com feedback.
- Linkar para login.

**Fora do escopo:**

- Verificacao de e-mail.
- Recuperacao de senha.
- Campos de perfil extras.
- Termos/consentimento complexo.

**Criterios de aceite:**

- Cadastro funciona contra API.
- E-mail duplicado exibe erro claro.
- Fluxo apos sucesso segue contrato da API.
- Projeto compila.

**Prompt sugerido:**

```text
Implemente somente a tela /register. Ela deve ter formulario de nome/email/senha, validacao local, mutation de cadastro, loading, erro claro para email duplicado, fluxo de sucesso conforme contrato da API (salvar sessao e redirecionar se vier token, ou ir para login com feedback se nao vier) e link para login. Nao implemente verificacao de email, recuperacao de senha, campos extras ou termos complexos.
```

## Etapa 15 - Guards para usuario ja autenticado em rotas publicas

**Objetivo:** Evitar que usuario autenticado veja login/cadastro sem necessidade.

**Escopo:**

- Criar wrapper de rota publica ou logica equivalente.
- Se houver token/sessao, redirecionar `/login` e `/register` para rota protegida padrao.
- Preservar comportamento quando nao houver sessao.

**Fora do escopo:**

- Validar token no servidor.
- Criar refresh token.
- Criar roles.
- Criar dashboard real.

**Criterios de aceite:**

- Usuario autenticado nao fica preso nas telas publicas de auth.
- Usuario sem sessao acessa login/cadastro normalmente.
- Projeto compila.

**Prompt sugerido:**

```text
Implemente somente o guard de rotas publicas de auth. Se houver token/sessao, /login e /register devem redirecionar para a rota protegida padrao; sem sessao, devem renderizar normalmente. Nao valide token no servidor, nao crie refresh token, roles ou dashboard real.
```

## Etapa 16 - Feedback visual e estados de formulario

**Objetivo:** Garantir que login e cadastro comuniquem estados ao usuario.

**Escopo:**

- Estado de loading no botao.
- Desabilitar submit durante mutation.
- Erro geral no topo do formulario.
- Erros por campo quando houver validacao local.
- Feedback de sucesso de cadastro quando aplicavel.

**Fora do escopo:**

- Sistema global de toast completo.
- Animacoes complexas.
- Tela de recuperacao de senha.
- Notificacoes push.

**Criterios de aceite:**

- Loading e erro sao claros.
- Usuario nao consegue submeter duplicado durante loading.
- Mensagens cabem em mobile.
- Projeto compila.

**Prompt sugerido:**

```text
Implemente somente feedback visual e estados dos formularios de auth. Inclua loading no botao, submit desabilitado durante mutation, erro geral, erros por campo e feedback de sucesso de cadastro quando aplicavel. Nao crie sistema global de toast, animacoes complexas, recuperacao de senha ou notificacoes push.
```

## Etapa 17 - Acessibilidade basica dos formularios

**Objetivo:** Garantir uso adequado por teclado e leitores de tela.

**Escopo:**

- Labels associados aos inputs.
- Mensagens de erro associadas aos campos.
- Foco visivel.
- Submit por teclado.
- Ordem de tab previsivel.
- Atributos `autocomplete` adequados.

**Fora do escopo:**

- Auditoria completa WCAG.
- Internacionalizacao.
- Tema visual final.
- Componentes complexos.

**Criterios de aceite:**

- Campos possuem labels.
- Erros sao acessiveis.
- Navegacao por teclado funciona.
- Projeto compila.

**Prompt sugerido:**

```text
Ajuste somente acessibilidade basica dos formularios de login/cadastro. Garanta labels associados, erros ligados aos campos, foco visivel, submit por teclado, ordem de tab previsivel e autocomplete adequado. Nao faca auditoria WCAG completa, internacionalizacao, tema final ou componentes complexos.
```

## Etapa 18 - Responsividade mobile-first

**Objetivo:** Validar e ajustar as telas em mobile e desktop.

**Escopo:**

- Ajustar layout para mobile-first.
- Garantir largura adequada dos formularios.
- Evitar texto quebrado de forma ruim.
- Evitar sobreposicao de controles.
- Manter visual simples.

**Fora do escopo:**

- Criar landing page.
- Criar hero visual.
- Criar dashboard.
- Criar tema completo.

**Criterios de aceite:**

- Login e cadastro funcionam em mobile.
- Login e cadastro funcionam em desktop.
- Nao ha sobreposicao de UI.
- Projeto compila.

**Prompt sugerido:**

```text
Ajuste somente responsividade mobile-first das telas de login e cadastro. Garanta largura adequada, textos sem quebra ruim, controles sem sobreposicao e visual simples em mobile e desktop. Nao crie landing page, hero, dashboard ou tema completo.
```

## Etapa 19 - Testes de formularios de autenticacao

**Objetivo:** Cobrir comportamento das telas sem depender de backend real.

**Escopo:**

- Testar validacao local de login.
- Testar validacao local de cadastro.
- Testar chamada de login com dados validos usando mock.
- Testar erro de credenciais invalidas.
- Testar chamada de cadastro com dados validos usando mock.
- Testar erro de e-mail duplicado.
- Testar salvamento de sessao em sucesso.
- Testar redirecionamento em sucesso.

**Fora do escopo:**

- Teste E2E contra backend real.
- Testar partidas, palpites, ranking ou estatisticas.
- Testar recuperacao de senha.
- Testar backend.

**Criterios de aceite:**

- Fluxos principais de auth estao cobertos.
- Erros principais estao cobertos.
- Testes nao dependem de API real.
- Testes passam.

**Prompt sugerido:**

```text
Crie somente testes das telas de autenticacao com mocks. Cubra validacao local de login/cadastro, chamada de login, erro de credenciais invalidas, chamada de cadastro, erro de email duplicado, salvamento de sessao e redirecionamento em sucesso. Nao crie E2E contra backend real, testes de features, recuperacao de senha ou backend.
```

## Etapa 20 - Validacao manual fim a fim contra API

**Objetivo:** Confirmar login e cadastro integrados com a API real em ambiente local.

**Escopo:**

- Subir frontend.
- Subir API.
- Testar cadastro com usuario novo.
- Testar cadastro com e-mail duplicado.
- Testar login com credenciais validas.
- Testar login com credenciais invalidas.
- Confirmar sessao persistindo apos refresh.
- Confirmar redirecionamento pos-login.

**Fora do escopo:**

- Testar recuperacao de senha.
- Testar outras telas.
- Testar ranking/palpites.
- Testar deploy.

**Criterios de aceite:**

- Cadastro funciona fim a fim.
- Login funciona fim a fim.
- Erros esperados aparecem claramente.
- Sessao persiste em refresh.

**Prompt sugerido:**

```text
Valide somente login e cadastro fim a fim contra a API local. Suba frontend e API, teste cadastro novo, cadastro com email duplicado, login valido, login invalido, persistencia da sessao apos refresh e redirecionamento pos-login. Nao teste recuperacao de senha, outras telas, ranking, palpites ou deploy.
```

## Etapa 21 - Documentacao local das telas de autenticacao

**Objetivo:** Documentar como usar e validar login/cadastro.

**Escopo:**

- Atualizar README ou documentacao frontend.
- Documentar rotas `/login` e `/register`.
- Documentar dependencia da API.
- Documentar variavel `VITE_API_BASE_URL`.
- Documentar comportamento de sessao.
- Documentar erros esperados.
- Registrar que recuperacao de senha esta fora do escopo.

**Fora do escopo:**

- Documentar telas futuras como prontas.
- Documentar deploy.
- Documentar detalhes internos do backend.
- Criar guia de produto extenso.

**Criterios de aceite:**

- Documentacao permite validar login/cadastro localmente.
- Erros esperados estao claros.
- Escopo fora da tarefa esta claro.

**Prompt sugerido:**

```text
Atualize somente a documentacao local das telas de autenticacao. Documente /login, /register, dependencia da API, VITE_API_BASE_URL, comportamento de sessao, erros esperados e que recuperacao de senha esta fora do escopo. Nao documente telas futuras como prontas, deploy ou detalhes internos do backend.
```

## Etapa 22 - Validacao final da Tarefa 13

**Objetivo:** Verificar se as telas de autenticacao atendem aos criterios originais.

**Escopo:**

- Rodar build/typecheck/lint/testes disponiveis.
- Subir frontend.
- Subir API.
- Testar cadastro contra `/auth/register`.
- Testar login contra `/auth/login`.
- Testar e-mail duplicado.
- Testar credencial invalida.
- Confirmar salvamento da sessao.
- Confirmar persistencia apos refresh.
- Confirmar redirecionamento pos-login.
- Confirmar responsividade basica.

**Fora do escopo:**

- Recuperacao de senha.
- Telas de partidas.
- Fluxo de palpites.
- Ranking.
- Estatisticas.
- Deploy.

**Criterios de aceite:**

- Cadastro e login funcionam fim a fim contra a API.
- Erros de e-mail duplicado e credencial invalida sao exibidos de forma clara.
- Apos login, usuario e redirecionado.
- Sessao persiste em refresh.
- Layout e responsivo.
- Pendencias foram registradas objetivamente, se houver.

**Prompt sugerido:**

```text
Valide somente a Tarefa 13. Rode build/typecheck/lint/testes disponiveis, suba frontend e API, teste cadastro em /auth/register, login em /auth/login, email duplicado, credencial invalida, salvamento de sessao, persistencia apos refresh, redirecionamento pos-login e responsividade basica. Se algo falhar, registre pendencia objetivamente. Nao implemente recuperacao de senha, partidas, palpites, ranking, estatisticas ou deploy.
```

## Mapeamento do escopo original para as etapas

| Item da Tarefa 13 | Etapa responsavel |
|---|---|
| Validar dependencias T12/T04 | Etapa 01 |
| Tipos de auth | Etapa 02 |
| Integracao com `/auth/login` | Etapas 03, 09, 13, 20 e 22 |
| Integracao com `/auth/register` | Etapas 03, 10, 14, 20 e 22 |
| Mapeamento de erros | Etapa 04 |
| Validacao do formulario de login | Etapa 05 |
| Validacao do formulario de cadastro | Etapa 06 |
| Componentes pequenos e tipados | Etapa 07 |
| Layout das telas | Etapa 08 |
| TanStack Query | Etapas 09 e 10 |
| Persistencia de sessao | Etapas 11, 20 e 22 |
| Redirecionamento pos-login | Etapas 12, 13, 20 e 22 |
| Tela de login | Etapa 13 |
| Tela de cadastro | Etapa 14 |
| Usuario autenticado em rotas publicas | Etapa 15 |
| Feedback de erro/loading | Etapa 16 |
| Acessibilidade basica | Etapa 17 |
| Responsividade | Etapa 18 |
| Testes das telas | Etapa 19 |
| Validacao fim a fim contra API | Etapa 20 |
| Documentacao | Etapa 21 |
| Validacao dos criterios de aceite originais | Etapa 22 |

