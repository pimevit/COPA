# Tarefa 04 - Divisao em etapas independentes

Fonte: `C:\Repo\bolao-copa-backlog.md`, secao **Tarefa 04 - Autenticacao (registro, login, JWT, hashing)**.

Este arquivo quebra a Tarefa 04 em etapas pequenas, fechadas e implementaveis separadamente. Nenhuma etapa abaixo deve implementar recuperacao de senha, roles/admin, verificacao de e-mail, regras de pontuacao, palpites, ranking, estatisticas ou frontend.

## Escopo original da Tarefa 04

Implementar autenticacao basica da Web API:

- `POST /auth/register` com `Name`, `Email` e `Password`.
- Hash seguro da senha.
- `POST /auth/login` retornando JWT.
- Validacao de e-mail unico.
- Mensagens claras para e-mail duplicado.
- Credenciais invalidas retornando `401`.
- Configuracao JWT no pipeline para habilitar `[Authorize]`.
- DTOs de request/response sem expor `PasswordHash`.
- Endpoint protegido de teste exigindo token.

## Dependencia base

A Tarefa 04 depende das Tarefas 02 e 03.

Cada etapa abaixo deve ser independente das outras etapas deste arquivo, mas pode exigir que exista:

- entidade `User` com `Name`, `Email`, `PasswordHash` e `CreatedAt`;
- indice unico em `User.Email`;
- `AppDbContext` registrado com Npgsql;
- migration aplicada em ambiente local/desenvolvimento;
- API executavel.

Se esse contrato nao existir, a etapa deve registrar bloqueio objetivo e nao deve implementar T02 ou T03 dentro da Tarefa 04.

## Decisoes assumidas para a Tarefa 04

- Nao usar ASP.NET Core Identity completo no MVP, salvo se ja estiver presente no projeto.
- Usar hashing seguro de senha, preferencialmente `PasswordHasher<TUser>` do ASP.NET Core ou alternativa equivalente.
- Nunca salvar senha em texto puro.
- Nunca retornar `PasswordHash` em DTOs.
- Normalizar e-mail para comparacao de unicidade.
- `POST /auth/register` deve retornar sucesso com dados publicos do usuario e/ou token, conforme decisao de implementacao local; o minimo esperado e criar o usuario com senha hasheada.
- `POST /auth/login` deve retornar JWT assinado.
- Configuracoes JWT devem vir de `appsettings`/variaveis de ambiente: chave, issuer, audience e expiracao.
- Roles/admin, recuperacao de senha e verificacao de e-mail ficam fora do MVP desta tarefa.

## Regra de independencia

Cada etapa deve poder ser implementada sozinha, sem depender da execucao previa de outra etapa deste arquivo.

Para preservar essa independencia:

- A etapa deve inspecionar o estado atual antes de alterar arquivos.
- A etapa deve preservar contratos e servicos existentes quando forem compativeis.
- A etapa deve criar apenas o minimo necessario para seu proprio objetivo compilar.
- Se faltar contrato das Tarefas 02/03, registrar bloqueio e parar.
- A etapa nao deve criar features de autorizacao por papel, recuperacao de senha ou verificacao de e-mail.
- Validacoes globais da Tarefa 04 so devem ser exigidas quando todas as etapas forem executadas.

## Etapa 01 - Pre-validacao das dependencias de autenticacao

**Objetivo:** Confirmar que o backend possui a base minima para implementar autenticacao.

**Escopo:**

- Verificar se a solucao backend compila.
- Verificar se `User` existe com os campos necessarios.
- Verificar se `AppDbContext` possui `DbSet<User>`.
- Verificar se `User.Email` tem indice unico no modelo EF.
- Verificar se a API executa.

**Fora do escopo:**

- Criar entidade `User`.
- Criar migration.
- Aplicar banco.
- Criar endpoints de autenticacao.

**Criterios de aceite:**

- Dependencias minimas foram confirmadas.
- Bloqueios foram registrados objetivamente, se houver.
- Nenhum endpoint ou servico de autenticacao foi criado.

**Prompt sugerido:**

```text
Valide somente as dependencias da Tarefa 04. Confira build, entidade User com Name/Email/PasswordHash/CreatedAt, DbSet<User>, indice unico em User.Email e API executavel. Se faltar algo, registre bloqueio objetivamente. Nao crie entidade, migration, seed, endpoints de auth, JWT ou hashing.
```

## Etapa 02 - DTOs e contratos de autenticacao

**Objetivo:** Definir os contratos publicos de request/response para autenticacao.

**Escopo:**

- Criar DTO de registro com `Name`, `Email`, `Password`.
- Criar DTO de login com `Email`, `Password`.
- Criar DTO de resposta sem `PasswordHash`.
- Criar contrato de resposta de token, se aplicavel.
- Manter nomes em ingles.

**Fora do escopo:**

- Validacao detalhada.
- Hashing.
- JWT.
- Persistencia.
- Endpoints.

**Criterios de aceite:**

- DTOs de request e response existem.
- Nenhum DTO expoe `PasswordHash`.
- A solucao compila.

**Prompt sugerido:**

```text
Crie somente os DTOs/contratos publicos de autenticacao: RegisterRequest com Name, Email e Password; LoginRequest com Email e Password; resposta publica sem PasswordHash; e resposta de token se necessario. Use nomes em ingles. Nao implemente validacao, hashing, JWT, persistencia, endpoints ou regras de negocio.
```

## Etapa 03 - Abstracao e implementacao de hashing de senha

**Objetivo:** Isolar o hashing e a verificacao de senha.

**Escopo:**

- Criar interface de hashing no `Application`.
- Criar implementacao no `Infrastructure`.
- Usar algoritmo seguro.
- Expor operacoes para gerar hash e verificar senha.
- Registrar o servico no DI.

**Fora do escopo:**

- Registro de usuario.
- Login.
- JWT.
- Endpoints.
- Politica complexa de senha.

**Criterios de aceite:**

- Senha em texto puro pode ser transformada em hash seguro.
- Verificacao de senha valida/invalida funciona.
- A implementacao nao salva senha em texto puro.
- A solucao compila.

**Prompt sugerido:**

```text
Crie somente a abstracao e implementacao de hashing de senha. A interface deve ficar em Application e a implementacao em Infrastructure, usando algoritmo seguro como PasswordHasher<T> ou equivalente. Exponha metodos para hash e verificacao e registre no DI. Nao implemente registro, login, JWT, endpoints ou politica complexa de senha.
```

## Etapa 04 - Configuracao de opcoes JWT

**Objetivo:** Preparar configuracoes necessarias para emissao e validacao de JWT.

**Escopo:**

- Criar modelo de configuracao `JwtOptions` ou equivalente.
- Adicionar chaves em `appsettings` para issuer, audience, secret e expiracao.
- Configurar bind/validacao basica das opcoes.
- Evitar secret hardcoded em codigo.

**Fora do escopo:**

- Emitir token.
- Configurar middleware JWT.
- Criar endpoints.
- Criar roles/claims de autorizacao por papel.

**Criterios de aceite:**

- Configuracao JWT existe e e lida via configuration/options.
- Secret nao esta hardcoded em classe de servico.
- A solucao compila.

**Prompt sugerido:**

```text
Configure somente as opcoes de JWT. Crie JwtOptions ou equivalente, adicione issuer, audience, secret e expiration em appsettings e registre o bind das opcoes. Nao emita token, nao configure middleware JWT, nao crie endpoints, roles ou claims de autorizacao por papel.
```

## Etapa 05 - Servico de emissao de token JWT

**Objetivo:** Criar o servico responsavel por gerar JWT assinado.

**Escopo:**

- Criar interface de token no `Application`.
- Criar implementacao no `Infrastructure`.
- Gerar JWT assinado com configuracoes de `JwtOptions`.
- Incluir claims minimas: identificador do usuario e e-mail.
- Retornar token e expiracao.
- Registrar o servico no DI.

**Fora do escopo:**

- Login.
- Registro.
- Middleware de autenticacao.
- Refresh token.
- Roles/admin.

**Criterios de aceite:**

- O servico gera JWT valido e assinado.
- Token usa issuer, audience, secret e expiracao de configuracao.
- Claims nao incluem dados sensiveis.
- A solucao compila.

**Prompt sugerido:**

```text
Crie somente o servico de emissao de JWT. A interface deve ficar em Application e a implementacao em Infrastructure. Gere token assinado usando JwtOptions, com claims minimas de user id e email, retornando token e expiracao. Nao implemente login, registro, middleware JWT, refresh token, roles ou endpoints.
```

## Etapa 06 - Acesso a usuario para autenticacao

**Objetivo:** Isolar consultas e persistencia de usuario usadas pelos casos de uso de auth.

**Escopo:**

- Criar abstracao no `Application` para buscar usuario por e-mail.
- Criar abstracao ou metodo para criar usuario.
- Implementar acesso usando EF Core no `Infrastructure`.
- Usar e-mail normalizado na busca.
- Preservar indice unico de `User.Email`.

**Fora do escopo:**

- Endpoint de registro.
- Endpoint de login.
- Hashing.
- JWT.
- Repositorio generico amplo.

**Criterios de aceite:**

- E possivel buscar usuario por e-mail.
- E possivel persistir novo usuario.
- A implementacao usa `AppDbContext`.
- A solucao compila.

**Prompt sugerido:**

```text
Crie somente o acesso a usuario necessario para autenticacao. Defina em Application uma abstracao para buscar User por email e criar User, e implemente em Infrastructure usando AppDbContext/EF Core. Use email normalizado na busca e preserve o indice unico de Email. Nao implemente endpoints, hashing, JWT, repositorio generico amplo ou regras fora de auth.
```

## Etapa 07 - Caso de uso de registro

**Objetivo:** Implementar a logica de cadastro de usuario.

**Escopo:**

- Criar servico/caso de uso de registro no `Application`.
- Validar duplicidade de e-mail.
- Gerar hash da senha.
- Criar `User` com `Name`, `Email`, `PasswordHash` e `CreatedAt`.
- Retornar resposta publica sem `PasswordHash`.

**Fora do escopo:**

- Endpoint HTTP.
- Emissao obrigatoria de token no registro, salvo se o contrato ja tiver sido decidido.
- Login.
- Validacao complexa de senha.
- Verificacao de e-mail.

**Criterios de aceite:**

- Registro cria usuario com senha hasheada.
- E-mail duplicado retorna erro de aplicacao claro.
- `PasswordHash` nao aparece na resposta.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente o caso de uso de registro no Application. Ele deve validar email duplicado, gerar hash da senha, criar User com Name, Email, PasswordHash e CreatedAt, e retornar resposta publica sem PasswordHash. Nao crie endpoint HTTP, nao implemente login, verificacao de email, recuperacao de senha, roles ou validacao complexa de senha.
```

## Etapa 08 - Caso de uso de login

**Objetivo:** Implementar a logica de login com credenciais.

**Escopo:**

- Criar servico/caso de uso de login no `Application`.
- Buscar usuario por e-mail.
- Verificar senha usando o servico de hashing.
- Gerar JWT usando o servico de token.
- Retornar token em credenciais validas.
- Retornar erro claro para credenciais invalidas.

**Fora do escopo:**

- Endpoint HTTP.
- Refresh token.
- Lockout.
- MFA.
- Roles/admin.

**Criterios de aceite:**

- Login valido retorna token.
- Login invalido nao revela se o e-mail existe ou se a senha esta errada.
- `PasswordHash` nao aparece na resposta.
- A solucao compila.

**Prompt sugerido:**

```text
Implemente somente o caso de uso de login no Application. Ele deve buscar usuario por email, verificar senha, gerar JWT via servico de token e retornar token para credenciais validas. Credenciais invalidas devem retornar erro unico sem revelar se email ou senha falharam. Nao crie endpoint HTTP, refresh token, lockout, MFA, roles ou admin.
```

## Etapa 09 - Endpoint POST /auth/register

**Objetivo:** Expor o cadastro via HTTP.

**Escopo:**

- Criar controller ou endpoint `POST /auth/register`.
- Receber DTO de registro.
- Chamar caso de uso de registro.
- Retornar sucesso com DTO publico.
- Mapear e-mail duplicado para `409 Conflict` ou `400 Bad Request` com mensagem clara.

**Fora do escopo:**

- Endpoint de login.
- JWT middleware.
- Recuperacao de senha.
- Verificacao de e-mail.
- Roles/admin.

**Criterios de aceite:**

- `POST /auth/register` cria usuario.
- Senha nao e retornada.
- `PasswordHash` nao e retornado.
- E-mail duplicado retorna erro claro.

**Prompt sugerido:**

```text
Crie somente o endpoint POST /auth/register. Ele deve receber RegisterRequest, chamar o caso de uso de registro, retornar DTO publico sem PasswordHash e mapear email duplicado para 409 Conflict ou 400 Bad Request com mensagem clara. Nao crie login, middleware JWT, recuperacao de senha, verificacao de email, roles ou admin.
```

## Etapa 10 - Endpoint POST /auth/login

**Objetivo:** Expor o login via HTTP.

**Escopo:**

- Criar endpoint `POST /auth/login`.
- Receber DTO de login.
- Chamar caso de uso de login.
- Retornar JWT para credenciais validas.
- Retornar `401 Unauthorized` para credenciais invalidas.

**Fora do escopo:**

- Registro.
- Refresh token.
- MFA.
- Roles/admin.
- Recuperacao de senha.

**Criterios de aceite:**

- `POST /auth/login` com credenciais validas retorna JWT.
- Credenciais invalidas retornam `401`.
- A resposta nao expoe `PasswordHash`.

**Prompt sugerido:**

```text
Crie somente o endpoint POST /auth/login. Ele deve receber LoginRequest, chamar o caso de uso de login, retornar JWT para credenciais validas e 401 Unauthorized para credenciais invalidas. Nao crie refresh token, MFA, roles, admin, recuperacao de senha ou endpoints de outras features.
```

## Etapa 11 - Middleware JWT e suporte a [Authorize]

**Objetivo:** Configurar autenticacao JWT no pipeline da API.

**Escopo:**

- Adicionar pacotes necessarios de autenticação JWT.
- Configurar `AddAuthentication` e `AddJwtBearer`.
- Usar issuer, audience e secret das opcoes.
- Adicionar `UseAuthentication` e `UseAuthorization` na ordem correta.
- Garantir que `[Authorize]` funcione.

**Fora do escopo:**

- Swagger com Bearer token.
- Roles/admin.
- Policies complexas.
- Refresh token.
- Endpoints de negocio.

**Criterios de aceite:**

- Token emitido pelo sistema e aceito pelo middleware.
- Requisicao sem token em endpoint protegido retorna `401`.
- A solucao compila.

**Prompt sugerido:**

```text
Configure somente o middleware JWT da API. Adicione pacotes se necessario, configure AddAuthentication/AddJwtBearer com issuer, audience e secret das opcoes, e aplique UseAuthentication/UseAuthorization na ordem correta para habilitar [Authorize]. Nao configure Swagger Bearer, roles, policies complexas, refresh token ou endpoints de negocio.
```

## Etapa 12 - Endpoint protegido de teste

**Objetivo:** Criar um endpoint minimo para validar `[Authorize]`.

**Escopo:**

- Criar endpoint tecnico protegido por `[Authorize]`.
- Retornar resposta simples sem dados sensiveis.
- Usar apenas para validar que token e exigido.

**Fora do escopo:**

- Criar endpoint de perfil completo.
- Criar roles/admin.
- Criar endpoints de palpites, ranking ou estatisticas.
- Expor claims sensiveis.

**Criterios de aceite:**

- Sem token, endpoint retorna `401`.
- Com token valido, endpoint retorna `200`.
- Endpoint nao expoe `PasswordHash` nem dados sensiveis.

**Prompt sugerido:**

```text
Crie somente um endpoint tecnico protegido por [Authorize] para validar autenticacao JWT. Ele deve retornar 401 sem token e 200 com token valido, sem expor dados sensiveis. Nao crie perfil completo, roles, admin, endpoints de negocio, palpites, ranking ou estatisticas.
```

## Etapa 13 - Testes de autenticacao

**Objetivo:** Cobrir os fluxos criticos de autenticacao.

**Escopo:**

- Testar hashing e verificacao de senha.
- Testar registro com sucesso.
- Testar registro com e-mail duplicado.
- Testar login com credenciais validas.
- Testar login com credenciais invalidas.
- Testar endpoint protegido com e sem token, se houver suporte a teste de integracao.

**Fora do escopo:**

- Testar regras de pontuacao.
- Testar ranking.
- Testar frontend.
- Criar suite E2E completa.

**Criterios de aceite:**

- Testes relevantes passam.
- Casos negativos principais estao cobertos.
- Nenhum teste depende de senha em texto puro no banco.

**Prompt sugerido:**

```text
Crie somente testes para autenticacao. Cubra hashing/verificacao, registro com sucesso, registro com email duplicado, login valido, login invalido e endpoint protegido com/sem token se houver suporte a teste de integracao. Nao teste pontuacao, ranking, frontend ou crie uma suite E2E ampla.
```

## Etapa 14 - Documentacao local de autenticacao

**Objetivo:** Documentar como configurar e validar a autenticacao em desenvolvimento.

**Escopo:**

- Atualizar README ou documentacao tecnica.
- Documentar configuracoes JWT necessarias.
- Documentar exemplos de chamadas para register e login.
- Documentar como testar endpoint protegido.
- Reforcar que roles, recuperacao de senha e verificacao de e-mail nao existem ainda.

**Fora do escopo:**

- Documentar funcionalidades futuras como prontas.
- Documentar deploy.
- Criar Swagger Bearer.
- Criar frontend.

**Criterios de aceite:**

- Documentacao permite testar registro, login e endpoint protegido.
- Nao ha segredo real exposto.
- O escopo ainda pendente esta claro.

**Prompt sugerido:**

```text
Atualize somente a documentacao local de autenticacao. Inclua configuracoes JWT necessarias, exemplos de POST /auth/register e POST /auth/login e como testar o endpoint protegido. Nao exponha segredo real e deixe claro que roles, recuperacao de senha e verificacao de email ainda nao existem. Nao implemente Swagger Bearer, deploy ou frontend.
```

## Etapa 15 - Validacao final da Tarefa 04

**Objetivo:** Verificar se a autenticacao atende aos criterios originais.

**Escopo:**

- Rodar build e testes.
- Aplicar migration/banco local se necessario para teste.
- Testar `POST /auth/register`.
- Confirmar que senha foi salva com hash.
- Testar e-mail duplicado.
- Testar `POST /auth/login` com credenciais validas.
- Testar login invalido retornando `401`.
- Validar JWT em endpoint protegido.
- Confirmar que `PasswordHash` nao aparece em respostas.

**Fora do escopo:**

- Recuperacao de senha.
- Roles/admin.
- Verificacao de e-mail.
- Swagger Bearer.
- Frontend.
- Palpites, ranking ou estatisticas.

**Criterios de aceite:**

- Registro cria usuario com senha hasheada.
- Login valido retorna JWT valido.
- Login invalido retorna `401`.
- E-mail duplicado retorna `409` ou `400` com mensagem clara.
- Endpoint protegido exige token.
- Nenhuma resposta expoe `PasswordHash`.
- Pendencias foram registradas objetivamente, se houver.

**Prompt sugerido:**

```text
Valide somente a Tarefa 04. Rode build e testes, teste POST /auth/register, confirme senha hasheada, teste email duplicado, teste POST /auth/login valido, login invalido 401, JWT em endpoint protegido e ausencia de PasswordHash nas respostas. Se algo falhar, registre a pendencia objetivamente. Nao implemente recuperacao de senha, roles/admin, verificacao de email, Swagger Bearer, frontend, palpites, ranking ou estatisticas.
```

## Mapeamento do escopo original para as etapas

| Item da Tarefa 04 | Etapa responsavel |
|---|---|
| Validar dependencias T02/T03 | Etapa 01 |
| DTOs de request/response | Etapa 02 |
| Hash seguro de senha | Etapa 03 |
| Configuracao JWT em `appsettings` | Etapa 04 |
| Emissao de JWT | Etapa 05 |
| Acesso a `User` para auth | Etapa 06 |
| Registro de usuario | Etapas 07 e 09 |
| Login de usuario | Etapas 08 e 10 |
| E-mail unico e erro claro | Etapas 06, 07, 09 e 15 |
| `[Authorize]` disponivel | Etapa 11 |
| Endpoint protegido de teste | Etapa 12 |
| Credenciais invalidas retornam `401` | Etapas 08, 10 e 15 |
| Nunca expor `PasswordHash` | Etapas 02, 07, 08, 09, 10, 12 e 15 |
| Testes de autenticacao | Etapa 13 |
| Documentacao local | Etapa 14 |
| Validacao dos criterios de aceite originais | Etapa 15 |

