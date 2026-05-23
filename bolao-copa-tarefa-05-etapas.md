# Tarefa 05 - Divisao em etapas independentes

Fonte: `C:\Repo\bolao-copa-backlog.md`, secao **Tarefa 05 - Transversais: Swagger, FluentValidation, erros e logging**.

Este arquivo quebra a Tarefa 05 em etapas pequenas, fechadas e implementaveis separadamente. Nenhuma etapa abaixo deve implementar regras de negocio, validacoes especificas de feature, endpoints de negocio, autenticacao nova, banco, seed, frontend ou deploy.

## Escopo original da Tarefa 05

Padronizar infraestrutura transversal da Web API:

- Swagger/OpenAPI habilitado.
- Swagger com suporte a autenticacao Bearer.
- FluentValidation integrado ao pipeline de requests.
- Requests invalidos retornando `400` com `ProblemDetails` padronizado.
- Middleware global de excecoes retornando `ProblemDetails`.
- Excecoes nao tratadas retornando `500` sem stack trace exposto em producao.
- Logging basico estruturado de requisicoes e erros.

## Dependencia base

A Tarefa 05 depende da Tarefa 01 e idealmente deve ser executada depois da Tarefa 04 ter ao menos um endpoint autenticado.

Cada etapa abaixo deve ser independente das outras etapas deste arquivo, mas pode exigir que exista:

- projeto `Api` executavel;
- pipeline ASP.NET Core configuravel;
- ao menos um endpoint para aparecer no Swagger;
- configuracao JWT existente para validar fluxo Bearer completo, se a etapa envolver autenticacao;
- DTOs/validators existentes para validar fluxo FluentValidation completo.

Se algum contrato ainda nao existir, a etapa deve registrar bloqueio ou validacao parcial objetiva, sem implementar a tarefa dependente dentro da Tarefa 05.

## Decisoes assumidas para a Tarefa 05

- Usar Swagger/OpenAPI com pacote padrao do ecossistema ASP.NET Core, como Swashbuckle, salvo se o projeto ja usar outro.
- Configurar Bearer token no Swagger apenas como suporte documental/interativo, sem implementar login novamente.
- Usar FluentValidation para validacoes de DTOs, mas nao criar regras especificas de features nesta tarefa.
- Respostas de erro devem seguir `ProblemDetails` ou `ValidationProblemDetails`.
- Em producao, `ProblemDetails` nao deve expor stack trace.
- Logs devem ser estruturados e uteis, sem registrar senha, token JWT, `PasswordHash` ou payload sensivel.
- Logging deve cobrir requisicoes e excecoes, sem adicionar observabilidade complexa.

## Regra de independencia

Cada etapa deve poder ser implementada sozinha, sem depender da execucao previa de outra etapa deste arquivo.

Para preservar essa independencia:

- A etapa deve inspecionar o estado atual antes de alterar arquivos.
- A etapa deve preservar configuracoes existentes compativeis.
- A etapa deve criar apenas o minimo necessario para seu proprio objetivo compilar.
- Se faltar endpoint, autenticacao ou validator para validacao completa, registrar pendencia em vez de criar feature nova.
- A etapa nao deve criar regra de negocio para "testar" a infraestrutura.
- Validacoes globais da Tarefa 05 so devem ser exigidas quando todas as etapas forem executadas.

## Etapa 01 - Pre-validacao da API e pipeline

**Objetivo:** Confirmar que existe uma API configuravel para receber transversais.

**Escopo:**

- Verificar se a solucao backend compila.
- Verificar se o projeto `Api` executa.
- Verificar arquivo principal de configuracao do pipeline.
- Verificar se ha pelo menos um endpoint para aparecer no Swagger.
- Verificar se autenticacao JWT ja existe, se o objetivo for validar Bearer end-to-end.

**Fora do escopo:**

- Criar endpoint de negocio.
- Implementar autenticacao JWT.
- Configurar Swagger.
- Configurar FluentValidation.
- Criar middleware de erro.

**Criterios de aceite:**

- Estado atual da API foi confirmado.
- Bloqueios foram registrados objetivamente, se houver.
- Nenhuma infraestrutura transversal foi alterada.

**Prompt sugerido:**

```text
Valide somente a base para a Tarefa 05. Confira build, execucao do projeto Api, arquivo/ponto de configuracao do pipeline, existencia de pelo menos um endpoint e, se aplicavel, existencia de autenticacao JWT para validar Bearer depois. Se faltar algo, registre bloqueio objetivamente. Nao configure Swagger, FluentValidation, ProblemDetails, logging, endpoints ou autenticacao.
```

## Etapa 02 - Swagger/OpenAPI base

**Objetivo:** Habilitar Swagger/OpenAPI para documentar endpoints existentes.

**Escopo:**

- Adicionar pacote Swagger/OpenAPI se necessario.
- Registrar geracao de OpenAPI.
- Habilitar UI em ambiente de desenvolvimento.
- Garantir que endpoints existentes aparecem em `/swagger`.
- Manter configuracao simples.

**Fora do escopo:**

- Configurar Bearer token.
- Criar endpoints para aparecer no Swagger.
- Documentar regras de negocio.
- Adicionar filtros complexos.

**Criterios de aceite:**

- `/swagger` abre em ambiente de desenvolvimento.
- Endpoints existentes aparecem na UI.
- A solucao compila.

**Prompt sugerido:**

```text
Configure somente o Swagger/OpenAPI base na Web API. Adicione pacote se necessario, registre a geracao OpenAPI e habilite a UI em desenvolvimento. Garanta que endpoints existentes aparecam em /swagger. Nao configure Bearer token, nao crie endpoints novos, nao documente regra de negocio e nao adicione filtros complexos.
```

## Etapa 03 - Suporte Bearer token no Swagger

**Objetivo:** Permitir informar JWT na UI do Swagger.

**Escopo:**

- Configurar security scheme Bearer.
- Configurar security requirement para endpoints protegidos.
- Adicionar botao de autorizacao na UI.
- Reutilizar configuracao JWT existente, se houver.

**Fora do escopo:**

- Implementar login.
- Emitir JWT.
- Criar endpoint protegido.
- Criar roles/policies.
- Alterar regras de autorizacao.

**Criterios de aceite:**

- Swagger exibe suporte a Bearer token.
- Usuario consegue informar token na UI.
- Endpoints protegidos podem ser chamados com token valido quando autenticacao ja existir.
- A solucao compila.

**Prompt sugerido:**

```text
Configure somente suporte a Bearer token no Swagger. Adicione security scheme e security requirement para permitir informar JWT na UI. Reutilize autenticacao JWT existente se houver. Nao implemente login, emissao de token, endpoint protegido, roles, policies ou regras de autorizacao.
```

## Etapa 04 - Registro do FluentValidation

**Objetivo:** Integrar FluentValidation ao pipeline da API.

**Escopo:**

- Adicionar pacotes FluentValidation necessarios.
- Registrar discovery de validators no assembly correto.
- Integrar validacao automatica aos requests, conforme padrao do projeto.
- Garantir que a configuracao nao exige validators de negocio nesta etapa.

**Fora do escopo:**

- Criar validadores especificos de features.
- Criar regras de negocio.
- Alterar DTOs de auth, matches ou bets.
- Criar endpoints.

**Criterios de aceite:**

- FluentValidation esta registrado no DI/pipeline.
- Validators existentes, se houver, sao descobertos.
- A solucao compila.
- Nenhuma regra especifica de feature foi adicionada.

**Prompt sugerido:**

```text
Integre somente FluentValidation ao pipeline da Web API. Adicione pacotes necessarios, registre discovery de validators no assembly correto e habilite validacao automatica de requests conforme o padrao do projeto. Nao crie validadores especificos de feature, regras de negocio, DTOs novos ou endpoints.
```

## Etapa 05 - Padrao de resposta para erro de validacao

**Objetivo:** Padronizar requests invalidos como `400` com `ProblemDetails`.

**Escopo:**

- Configurar resposta de validacao para `ValidationProblemDetails` ou formato compativel.
- Incluir `status`, `title`, `traceId` e erros por campo.
- Garantir retorno HTTP `400`.
- Evitar vazamento de dados sensiveis no erro.

**Fora do escopo:**

- Criar validadores especificos de feature.
- Criar mensagem de negocio detalhada.
- Criar middleware global de excecoes.
- Alterar endpoints de negocio.

**Criterios de aceite:**

- Request invalido retorna `400`.
- Corpo segue `ProblemDetails`/`ValidationProblemDetails`.
- Erros por campo ficam claros.
- Dados sensiveis nao aparecem na resposta.

**Prompt sugerido:**

```text
Padronize somente a resposta de validacao invalida como 400 com ValidationProblemDetails ou formato compativel. Inclua status, title, traceId e erros por campo, sem expor dados sensiveis. Nao crie validadores especificos de feature, regras de negocio, middleware global de excecoes ou endpoints.
```

## Etapa 06 - Contrato de ProblemDetails para excecoes

**Objetivo:** Definir o formato padrao de erro para excecoes.

**Escopo:**

- Definir como excecoes serao convertidas para `ProblemDetails`.
- Padronizar campos basicos: `status`, `title`, `detail`, `traceId`.
- Separar comportamento de desenvolvimento e producao.
- Garantir que stack trace nao apareca em producao.

**Fora do escopo:**

- Implementar catalogo amplo de excecoes de negocio.
- Criar regras de negocio.
- Alterar validacao de DTOs.
- Criar endpoints.

**Criterios de aceite:**

- Contrato de erro esta definido no codigo de infraestrutura.
- Producao nao expoe stack trace.
- Desenvolvimento pode ter detalhe suficiente para debug, se o projeto permitir.
- A solucao compila.

**Prompt sugerido:**

```text
Defina somente o contrato de ProblemDetails para excecoes. Padronize status, title, detail e traceId, separando comportamento de desenvolvimento e producao, sem expor stack trace em producao. Nao crie catalogo amplo de excecoes de negocio, regras de negocio, validators ou endpoints.
```

## Etapa 07 - Middleware global de excecoes

**Objetivo:** Capturar excecoes nao tratadas e responder de forma consistente.

**Escopo:**

- Criar middleware global de excecoes.
- Registrar middleware no pipeline na ordem correta.
- Converter excecoes nao tratadas em `500 ProblemDetails`.
- Registrar erro no log.
- Nao expor stack trace em producao.

**Fora do escopo:**

- Criar excecoes especificas de negocio.
- Criar mapeamento de todos os erros possiveis.
- Alterar controllers/endpoints de feature.
- Criar retry ou resiliencia.

**Criterios de aceite:**

- Excecao nao tratada vira HTTP `500`.
- Resposta usa `ProblemDetails`.
- Stack trace nao aparece em producao.
- Erro e registrado em log.

**Prompt sugerido:**

```text
Crie somente o middleware global de excecoes. Ele deve capturar excecoes nao tratadas, registrar erro em log e retornar 500 com ProblemDetails, sem stack trace em producao. Registre o middleware na ordem correta do pipeline. Nao crie excecoes de negocio, mapeamento amplo de erros, retry, resiliencia ou endpoints.
```

## Etapa 08 - Logging estruturado basico

**Objetivo:** Configurar logging estruturado simples para a API.

**Escopo:**

- Configurar logging com campos estruturados basicos.
- Garantir logs de inicializacao, requisicoes e erros.
- Manter nivel de log por ambiente.
- Evitar registro de senha, token, `PasswordHash` ou payload sensivel.

**Fora do escopo:**

- Observabilidade avancada.
- Tracing distribuido.
- Exportadores externos.
- Dashboard.
- Alertas.

**Criterios de aceite:**

- Logs sao emitidos em formato estruturado ou com propriedades estruturadas.
- Requisicoes e erros aparecem nos logs.
- Dados sensiveis nao sao logados.
- A solucao compila.

**Prompt sugerido:**

```text
Configure somente logging estruturado basico na API. Garanta logs de inicializacao, requisicoes e erros, com nivel por ambiente e sem registrar senha, token JWT, PasswordHash ou payload sensivel. Nao implemente tracing distribuido, exportadores externos, dashboard, alertas ou observabilidade avancada.
```

## Etapa 09 - Logging de requisicoes HTTP

**Objetivo:** Registrar requisicoes de forma consistente e segura.

**Escopo:**

- Adicionar middleware/configuracao para log de requisicoes.
- Registrar metodo, caminho, status code, duracao e correlation/trace id se disponivel.
- Evitar body e headers sensiveis.
- Integrar com o logging estruturado existente.

**Fora do escopo:**

- Logar payload completo.
- Logar Authorization header.
- Criar auditoria de negocio.
- Criar tracing distribuido.

**Criterios de aceite:**

- Cada requisicao gera log util.
- Erros e status codes ficam visiveis.
- Headers sensiveis e body nao sao logados por padrao.
- A solucao compila.

**Prompt sugerido:**

```text
Configure somente logging seguro de requisicoes HTTP. Registre metodo, caminho, status code, duracao e trace/correlation id quando disponivel. Nao logue body, Authorization header, senha, token ou dados sensiveis. Nao crie auditoria de negocio nem tracing distribuido.
```

## Etapa 10 - Testes de transversais

**Objetivo:** Cobrir os comportamentos transversais principais.

**Escopo:**

- Testar que Swagger/OpenAPI esta disponivel em desenvolvimento, se houver suporte a teste.
- Testar request invalido retornando `400 ProblemDetails`, usando validator ja existente.
- Testar excecao nao tratada retornando `500 ProblemDetails`, usando endpoint de teste apenas se ja existir infraestrutura apropriada.
- Testar endpoint protegido com Bearer no Swagger apenas manualmente se nao houver automacao adequada.

**Fora do escopo:**

- Criar regra de negocio para teste.
- Criar endpoint publico permanente apenas para quebrar.
- Testar features de negocio.
- Testar frontend.

**Criterios de aceite:**

- Comportamentos transversais criticos foram testados ou tiveram pendencia registrada.
- Testes nao dependem de dados sensiveis.
- Nenhuma feature de negocio foi criada para viabilizar teste.

**Prompt sugerido:**

```text
Crie somente testes para transversais existentes. Cubra Swagger/OpenAPI em desenvolvimento se aplicavel, request invalido retornando 400 ProblemDetails com validator ja existente, e excecao nao tratada retornando 500 ProblemDetails quando houver infraestrutura de teste apropriada. Nao crie regra de negocio, endpoint permanente apenas para falhar, testes de feature ou frontend.
```

## Etapa 11 - Documentacao dos transversais

**Objetivo:** Documentar como usar e validar a infraestrutura transversal.

**Escopo:**

- Atualizar README ou documentacao tecnica.
- Documentar acesso ao Swagger.
- Documentar como usar Bearer token no Swagger.
- Documentar padrao de `ProblemDetails`.
- Documentar comportamento de logs e dados que nao devem ser logados.
- Documentar como adicionar validators em features futuras.

**Fora do escopo:**

- Documentar regras de negocio futuras como prontas.
- Criar exemplos de endpoints nao implementados.
- Documentar deploy ou observabilidade externa.

**Criterios de aceite:**

- Documentacao explica como validar Swagger, Bearer, erros e logs.
- A documentacao nao afirma features futuras como implementadas.
- Orientacao para validators futuros esta clara e curta.

**Prompt sugerido:**

```text
Atualize somente a documentacao dos transversais. Explique acesso ao Swagger, uso de Bearer token na UI, padrao de ProblemDetails, comportamento de logs, dados que nao devem ser logados e como adicionar validators em features futuras. Nao documente endpoints futuros como prontos, deploy ou observabilidade externa.
```

## Etapa 12 - Validacao final da Tarefa 05

**Objetivo:** Verificar se a infraestrutura transversal atende aos criterios originais.

**Escopo:**

- Rodar build e testes.
- Subir a API em desenvolvimento.
- Abrir `/swagger`.
- Confirmar que endpoints aparecem.
- Confirmar suporte a Bearer token no Swagger.
- Enviar request invalido e conferir `400 ProblemDetails`.
- Forcar ou simular excecao nao tratada e conferir `500 ProblemDetails`.
- Confirmar que stack trace nao aparece em comportamento de producao.
- Conferir logs de requisicoes e erros.

**Fora do escopo:**

- Implementar regras de negocio.
- Criar validators especificos de feature.
- Criar endpoints de negocio.
- Implementar autenticacao que ainda nao exista.
- Criar frontend.

**Criterios de aceite:**

- `/swagger` lista endpoints e permite Bearer.
- Request invalido retorna `400` com `ProblemDetails` padronizado.
- Excecao nao tratada retorna `500` com `ProblemDetails`.
- Stack trace nao e exposto em producao.
- Logs registram requisicoes e erros.
- Pendencias foram registradas objetivamente, se houver.

**Prompt sugerido:**

```text
Valide somente a Tarefa 05. Rode build e testes, suba a API, abra /swagger, confirme endpoints e suporte a Bearer, envie request invalido para obter 400 ProblemDetails, simule excecao nao tratada para obter 500 ProblemDetails sem stack trace em producao, e confira logs de requisicoes e erros. Se algo falhar, registre pendencia objetivamente. Nao implemente regras de negocio, validators especificos de feature, endpoints de negocio, autenticacao nova ou frontend.
```

## Mapeamento do escopo original para as etapas

| Item da Tarefa 05 | Etapa responsavel |
|---|---|
| Validar base da API | Etapa 01 |
| Swagger/OpenAPI habilitado | Etapa 02 |
| `/swagger` lista endpoints | Etapas 02 e 12 |
| Suporte a Bearer token no Swagger | Etapa 03 |
| FluentValidation no pipeline | Etapa 04 |
| Request invalido retorna `400 ProblemDetails` | Etapa 05 |
| Contrato padrao de `ProblemDetails` | Etapa 06 |
| Middleware global de excecoes | Etapa 07 |
| Excecao nao tratada retorna `500` | Etapas 07 e 12 |
| Stack trace nao exposto em producao | Etapas 06, 07 e 12 |
| Logging estruturado basico | Etapa 08 |
| Logs de requisicoes | Etapa 09 |
| Logs de erros | Etapas 07, 08 e 12 |
| Testes de transversais | Etapa 10 |
| Documentacao | Etapa 11 |
| Validacao dos criterios de aceite originais | Etapa 12 |

