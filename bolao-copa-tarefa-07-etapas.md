# Tarefa 07 - Divisao em etapas independentes

Fonte: `C:\Repo\bolao-copa-backlog.md`, secao **Tarefa 07 - Servico de pontuacao (dominio puro) + testes unitarios**.

Este arquivo quebra a Tarefa 07 em etapas pequenas, fechadas e implementaveis separadamente. Nenhuma etapa abaixo deve implementar persistencia, EF Core, endpoints, recálculo em massa, autenticação, bets, ranking, frontend ou seed.

## Escopo original da Tarefa 07

Implementar o calculo de pontos como regra pura de dominio:

- Funcao pura `calculateScore(prediction, result, stage)`.
- Retorno dos pontos finais: pontuacao base multiplicada pelo multiplicador da fase.
- Regra base nao cumulativa, onde prevalece o maior acerto:
  - placar exato: 5 pontos;
  - vencedor/empate correto: 2 pontos;
  - gols de exatamente um time: 1 ponto;
  - erro total: 0 pontos.
- Multiplicadores:
  - `Groups`: x1;
  - `RoundOf16`: x2;
  - `QuarterFinals`: x3;
  - `SemiFinals`: x4;
  - `Final`: x5.
- Testes unitarios cobrindo placar exato, vencedor, gols de um time, erro total, empate e multiplicadores.
- Teste explicito para o caso ambiguo do Exemplo 3: palpite `2x1`, resultado `2x0`, pendente de confirmacao da duvida 3.1.

## Dependencia base

A Tarefa 07 depende da Tarefa 02 apenas para tipos/enums do dominio.

Cada etapa abaixo deve ser independente das outras etapas deste arquivo, mas pode exigir que exista:

- projeto `Domain`;
- enum `Stage`;
- solucao backend compilavel;
- projeto de testes, ou possibilidade de criar um projeto de testes unitarios.

Se esse contrato nao existir, a etapa deve registrar bloqueio objetivo e nao deve implementar T02 dentro da Tarefa 07.

## Decisoes assumidas para a Tarefa 07

- O servico deve ficar no dominio e nao deve depender de banco, HTTP, DI obrigatorio ou horario.
- O metodo sugerido e `calculateScore(homeGoalsPrediction, awayGoalsPrediction, homeGoalsResult, awayGoalsResult, stage)`.
- Entradas de gols devem ser inteiros nao negativos; validacao de request HTTP fica fora desta tarefa.
- A regra e nao cumulativa: uma mesma partida recebe apenas a maior categoria aplicavel.
- Precedencia assumida ate confirmacao da duvida 3.1:
  - placar exato;
  - vencedor/empate correto;
  - gols de exatamente um time;
  - erro total.
- Caso ambiguo `palpite 2x1` vs. `resultado 2x0`: assumir 2 pontos base por vencedor correto, documentando a duvida.
- Multiplicador e aplicado depois da pontuacao base.
- Resultado sem gols finais nao deve ser calculado por esta tarefa; chamadas devem fornecer resultado completo.

## Regra de independencia

Cada etapa deve poder ser implementada sozinha, sem depender da execucao previa de outra etapa deste arquivo.

Para preservar essa independencia:

- A etapa deve inspecionar o estado atual antes de alterar arquivos.
- A etapa deve preservar servicos, testes e contratos existentes quando forem compativeis.
- A etapa deve criar apenas o minimo necessario para seu proprio objetivo compilar.
- Se faltar `Stage` ou projeto `Domain`, registrar bloqueio e parar.
- A etapa nao deve criar endpoints, migrations, persistencia, bets ou recálculo em massa.
- Validacoes globais da Tarefa 07 so devem ser exigidas quando todas as etapas forem executadas.

## Etapa 01 - Pre-validacao do dominio para pontuacao

**Objetivo:** Confirmar que existe base minima para implementar a regra de pontuacao.

**Escopo:**

- Verificar se a solucao backend compila.
- Verificar se o projeto `Domain` existe.
- Verificar se o enum `Stage` existe com os valores esperados.
- Verificar se ja existe algum servico de pontuacao para preservar ou ajustar.
- Verificar se ja existe projeto de testes unitarios.

**Fora do escopo:**

- Criar `ScoreCalculator`.
- Criar testes.
- Alterar enum `Stage`.
- Criar endpoints ou persistencia.

**Criterios de aceite:**

- Dependencias minimas foram confirmadas.
- Bloqueios foram registrados objetivamente, se houver.
- Nenhuma regra de pontuacao foi implementada.

**Prompt sugerido:**

```text
Valide somente a base para a Tarefa 07. Confira build, existencia do projeto Domain, enum Stage com Groups/RoundOf16/QuarterFinals/SemiFinals/Final, existencia de servico de pontuacao previo e projeto de testes unitarios. Se faltar algo, registre bloqueio objetivamente. Nao crie ScoreCalculator, testes, endpoints, EF, persistencia ou recálculo em massa.
```

## Etapa 02 - Contrato publico do ScoreCalculator

**Objetivo:** Definir a assinatura publica do calculo de pontuacao.

**Escopo:**

- Criar ou ajustar o contrato do `ScoreCalculator` no dominio.
- Definir metodo `calculateScore`.
- Receber gols previstos, gols reais e `Stage`.
- Retornar pontos finais como inteiro.
- Manter o contrato sem dependencia de EF, HTTP ou Application.

**Fora do escopo:**

- Implementar todas as regras.
- Criar testes completos.
- Persistir pontos.
- Criar endpoints.

**Criterios de aceite:**

- Contrato do calculador existe no projeto `Domain`.
- Metodo tem entradas e retorno claros.
- O projeto `Domain` compila.
- Nao ha dependencia de infraestrutura.

**Prompt sugerido:**

```text
Crie somente o contrato publico do ScoreCalculator no projeto Domain, com metodo calculateScore recebendo homeGoalsPrediction, awayGoalsPrediction, homeGoalsResult, awayGoalsResult e stage, retornando pontos finais como inteiro. Mantenha sem dependencia de EF, HTTP, Application ou Infrastructure. Nao implemente todas as regras, testes completos, persistencia ou endpoints.
```

## Etapa 03 - Regra de placar exato

**Objetivo:** Implementar e testar apenas a categoria de placar exato.

**Escopo:**

- Reconhecer quando gols previstos sao iguais ao resultado.
- Retornar base 5 antes de multiplicador, ou pontos finais se o multiplicador ja estiver no contrato.
- Criar teste unitario para placar exato em fase `Groups`.

**Fora do escopo:**

- Vencedor/empate correto.
- Gols de um time.
- Erro total.
- Todos os multiplicadores.
- Endpoints ou persistencia.

**Criterios de aceite:**

- Placar exato retorna 5 em `Groups`.
- Teste unitario do caso passa.
- Nenhum acesso a banco ou HTTP foi criado.

**Prompt sugerido:**

```text
Implemente somente a categoria de placar exato no ScoreCalculator e um teste unitario simples em Groups. Quando palpite e resultado tiverem os mesmos gols, o retorno deve ser 5 em Groups. Nao implemente vencedor/empate, gols de um time, erro total, todos os multiplicadores, endpoints, EF ou persistencia.
```

## Etapa 04 - Regra de vencedor ou empate correto

**Objetivo:** Implementar a categoria de acerto de resultado da partida.

**Escopo:**

- Detectar vitoria do mandante, vitoria do visitante ou empate no palpite.
- Detectar vitoria do mandante, vitoria do visitante ou empate no resultado.
- Retornar base 2 quando o desfecho for correto e nao houver placar exato.
- Criar testes para vencedor mandante, vencedor visitante e empate.

**Fora do escopo:**

- Gols de um time.
- Multiplicadores de todas as fases.
- Persistencia.
- Endpoints.

**Criterios de aceite:**

- Vencedor correto retorna 2 em `Groups`.
- Empate correto sem placar exato retorna 2 em `Groups`.
- Placar exato continua tendo precedencia sobre vencedor.
- Testes unitarios passam.

**Prompt sugerido:**

```text
Implemente somente a categoria de vencedor/empate correto no ScoreCalculator. Cubra vitoria mandante, vitoria visitante e empate correto sem placar exato, retornando 2 em Groups. Garanta que placar exato continue tendo precedencia. Nao implemente gols de um time, todos os multiplicadores, persistencia, endpoints ou recálculo.
```

## Etapa 05 - Regra de gols de exatamente um time

**Objetivo:** Implementar a categoria de acerto de gols de um unico time.

**Escopo:**

- Detectar quando apenas um dos placares previstos bate com o resultado.
- Retornar base 1 quando nao houver placar exato nem vencedor/empate correto.
- Criar testes para acerto de gols do mandante e do visitante.

**Fora do escopo:**

- Alterar precedencia de vencedor.
- Multiplicadores de todas as fases.
- Endpoints.
- Persistencia.

**Criterios de aceite:**

- Acerto de gols de apenas um time retorna 1 em `Groups`.
- Acerto dos dois gols continua sendo placar exato.
- Vencedor/empate correto continua tendo precedencia sobre gols de um time.
- Testes unitarios passam.

**Prompt sugerido:**

```text
Implemente somente a categoria de gols de exatamente um time no ScoreCalculator. Ela deve retornar 1 em Groups apenas quando nao houver placar exato nem vencedor/empate correto. Cubra acerto de gols do mandante e visitante. Nao altere a precedencia, nao implemente todos os multiplicadores, endpoints, EF ou persistencia.
```

## Etapa 06 - Regra de erro total

**Objetivo:** Garantir retorno zero quando nao houver qualquer acerto pontuavel.

**Escopo:**

- Identificar caso sem placar exato, sem vencedor/empate correto e sem gols de um time.
- Retornar 0.
- Criar teste unitario de erro total.

**Fora do escopo:**

- Multiplicadores.
- Persistencia.
- Endpoints.
- Regras de ranking.

**Criterios de aceite:**

- Erro total retorna 0.
- Teste unitario passa.
- Nao ha dependencia de banco ou HTTP.

**Prompt sugerido:**

```text
Implemente somente o caso de erro total no ScoreCalculator: quando nao houver placar exato, vencedor/empate correto nem gols de um time, retornar 0. Crie teste unitario para esse caso. Nao implemente multiplicadores adicionais, persistencia, endpoints ou ranking.
```

## Etapa 07 - Precedencia nao cumulativa e caso ambiguo

**Objetivo:** Documentar e testar a ordem de precedencia entre categorias.

**Escopo:**

- Garantir que apenas a maior categoria aplicavel pontua.
- Testar precedencia de placar exato sobre vencedor.
- Testar precedencia de vencedor/empate sobre gols de um time.
- Incluir teste do caso ambiguo: palpite `2x1`, resultado `2x0`.
- Nomear ou comentar o teste indicando que a duvida 3.1 segue pendente de confirmacao.

**Fora do escopo:**

- Mudar a regra de negocio fora da decisao assumida.
- Persistir pontos.
- Criar endpoints.
- Recalcular apostas existentes.

**Criterios de aceite:**

- Regras nao sao cumulativas.
- Caso `2x1` vs. `2x0` documenta a decisao assumida.
- O teste do caso ambiguo assume vencedor correto como 2 pontos base.
- Testes unitarios passam.

**Prompt sugerido:**

```text
Garanta somente a precedencia nao cumulativa do ScoreCalculator. Teste placar exato acima de vencedor, vencedor/empate acima de gols de um time, e inclua o caso ambiguo palpite 2x1 vs resultado 2x0 com nome/comentario dizendo que a duvida 3.1 segue pendente; assuma vencedor correto = 2 pontos base. Nao persista pontos, nao crie endpoints e nao implemente recálculo em massa.
```

## Etapa 08 - Multiplicadores por fase

**Objetivo:** Aplicar o multiplicador correto a partir do `Stage`.

**Escopo:**

- Mapear multiplicadores:
  - `Groups`: x1;
  - `RoundOf16`: x2;
  - `QuarterFinals`: x3;
  - `SemiFinals`: x4;
  - `Final`: x5.
- Aplicar multiplicador sobre a pontuacao base.
- Criar testes para cada fase.

**Fora do escopo:**

- Criar novas fases.
- Incluir disputa de terceiro lugar.
- Alterar enum `Stage` sem confirmacao.
- Persistir pontos.

**Criterios de aceite:**

- Cada `Stage` aplica o multiplicador correto.
- Pontuacao final = base x multiplicador.
- Testes unitarios passam para todas as fases previstas.

**Prompt sugerido:**

```text
Implemente somente os multiplicadores por Stage no ScoreCalculator: Groups x1, RoundOf16 x2, QuarterFinals x3, SemiFinals x4 e Final x5. Aplique sobre a pontuacao base e crie testes para cada fase. Nao crie novas fases, disputa de terceiro lugar, persistencia, endpoints ou recálculo.
```

## Etapa 09 - Validacao de entradas do calculador

**Objetivo:** Definir comportamento para entradas invalidas no dominio.

**Escopo:**

- Definir tratamento para gols negativos.
- Definir tratamento para `Stage` invalido, se aplicavel.
- Garantir que resultado incompleto nao seja aceito silenciosamente.
- Criar testes unitarios para entradas invalidas.

**Fora do escopo:**

- Validacao de DTO HTTP.
- Mensagens de API.
- Persistencia.
- Alterar regras de janela de palpite.

**Criterios de aceite:**

- Entradas invalidas tem comportamento explicito.
- Gols negativos nao geram pontuacao silenciosa.
- Testes cobrem os casos definidos.
- O comportamento fica restrito ao dominio.

**Prompt sugerido:**

```text
Defina somente o comportamento do ScoreCalculator para entradas invalidas. Cubra gols negativos, Stage invalido se aplicavel e resultado incompleto, usando excecao ou retorno definido conforme padrao do dominio. Crie testes unitarios. Nao implemente validacao de DTO HTTP, mensagens de API, persistencia ou regras de janela de palpite.
```

## Etapa 10 - Projeto de testes unitarios do dominio

**Objetivo:** Garantir estrutura de testes para o `Domain`.

**Escopo:**

- Criar projeto de testes unitarios se nao existir.
- Usar framework padrao do projeto, preferencialmente xUnit se nao houver decisao previa.
- Referenciar o projeto `Domain`.
- Adicionar o projeto de testes a solucao.
- Nao depender de banco, API ou arquivos externos.

**Fora do escopo:**

- Testes de integracao.
- Testes de API.
- Testes de EF.
- Testes de frontend.

**Criterios de aceite:**

- Projeto de testes unitarios existe.
- Projeto referencia `Domain`.
- Testes rodam sem banco e sem API.
- `dotnet test` executa o projeto.

**Prompt sugerido:**

```text
Crie somente a estrutura de testes unitarios para o Domain, se ainda nao existir. Use xUnit ou o framework ja adotado, referencie o projeto Domain e adicione o projeto a solucao. Os testes nao devem depender de banco, API, EF, arquivos externos ou frontend.
```

## Etapa 11 - Suite completa de testes do ScoreCalculator

**Objetivo:** Consolidar cobertura dos cenarios exigidos pela Tarefa 07.

**Escopo:**

- Cobrir placar exato.
- Cobrir vencedor mandante e visitante.
- Cobrir empate correto.
- Cobrir gols de um time.
- Cobrir erro total.
- Cobrir cada multiplicador de fase.
- Cobrir caso ambiguo do Exemplo 3.

**Fora do escopo:**

- Testes de persistencia.
- Testes de endpoints.
- Testes de recálculo em massa.
- Testes de ranking.

**Criterios de aceite:**

- Todos os cenarios da Tarefa 07 tem testes.
- Testes documentam a decisao do caso ambiguo.
- Testes rodam rapido e sem infraestrutura externa.
- `dotnet test` passa.

**Prompt sugerido:**

```text
Crie somente a suite completa de testes unitarios do ScoreCalculator. Cubra placar exato, vencedor mandante/visitante, empate correto, gols de um time, erro total, todos os multiplicadores de fase e o caso ambiguo do Exemplo 3 documentando a duvida 3.1. Nao crie testes de persistencia, endpoints, recálculo, ranking ou frontend.
```

## Etapa 12 - Documentacao da regra de pontuacao

**Objetivo:** Documentar a regra implementada para futuras tarefas consumirem corretamente.

**Escopo:**

- Atualizar README ou documentacao tecnica.
- Descrever regra base nao cumulativa.
- Descrever multiplicadores.
- Registrar decisao temporaria do caso ambiguo.
- Declarar que persistencia e recálculo entram em tarefas futuras.

**Fora do escopo:**

- Documentar endpoints ainda nao implementados.
- Documentar ranking como pronto.
- Criar regra nova.
- Criar scripts.

**Criterios de aceite:**

- Regra de pontuacao esta documentada de forma curta e objetiva.
- Ambiguidade do Exemplo 3 esta registrada.
- Documentacao nao afirma que persistencia ou recálculo estao prontos.

**Prompt sugerido:**

```text
Atualize somente a documentacao da regra de pontuacao. Descreva a regra base nao cumulativa, multiplicadores por fase e a decisao temporaria do caso ambiguo 2x1 vs 2x0, mencionando a duvida 3.1. Deixe claro que persistencia e recálculo entram em tarefas futuras. Nao documente endpoints ou ranking como prontos.
```

## Etapa 13 - Validacao final da Tarefa 07

**Objetivo:** Verificar se o servico de pontuacao atende aos criterios originais.

**Escopo:**

- Rodar build da solucao.
- Rodar testes unitarios.
- Conferir que `ScoreCalculator` esta no dominio.
- Conferir ausencia de dependencia de EF, HTTP ou infraestrutura.
- Conferir cenarios de placar exato, vencedor, gols de um time, erro total, empate e multiplicadores.
- Conferir teste/documentacao do caso ambiguo.

**Fora do escopo:**

- Persistir `PointsEarned`.
- Criar endpoints.
- Recalcular apostas.
- Integrar com Bets.
- Alterar ranking.

**Criterios de aceite:**

- Servico e classe de dominio pura.
- Testes passam para todos os cenarios da regra.
- Multiplicadores estao corretos.
- Caso ambiguo documenta a decisao e a duvida 3.1.
- Nenhuma dependencia de banco ou HTTP foi introduzida.
- Pendencias foram registradas objetivamente, se houver.

**Prompt sugerido:**

```text
Valide somente a Tarefa 07. Rode build e testes unitarios, confirme que ScoreCalculator esta no Domain, sem dependencia de EF/HTTP/Infrastructure, e confira cobertura de placar exato, vencedor, gols de um time, erro total, empate, multiplicadores e caso ambiguo 2x1 vs 2x0 com duvida 3.1 documentada. Se algo falhar, registre pendencia objetivamente. Nao persista PointsEarned, nao crie endpoints, nao recalcule apostas, nao integre com Bets e nao altere ranking.
```

## Mapeamento do escopo original para as etapas

| Item da Tarefa 07 | Etapa responsavel |
|---|---|
| Validar dependencia da Tarefa 02 | Etapa 01 |
| Contrato `calculateScore` | Etapa 02 |
| Placar exato | Etapa 03 |
| Vencedor/empate correto | Etapa 04 |
| Gols de exatamente um time | Etapa 05 |
| Erro total | Etapa 06 |
| Regra nao cumulativa | Etapa 07 |
| Caso ambiguo do Exemplo 3 | Etapas 07, 11, 12 e 13 |
| Multiplicadores por fase | Etapa 08 |
| Entradas invalidas | Etapa 09 |
| Projeto de testes unitarios | Etapa 10 |
| Testes de todos os cenarios | Etapa 11 |
| Documentacao da regra | Etapa 12 |
| Validacao dos criterios de aceite originais | Etapa 13 |

