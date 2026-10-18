# Uso de IA

## Ferramenta

Usei o **Verboo Code**, com o modelo **DeepSeek 4.1 Flash** (esforço de raciocínio *low*), como par
de programação dentro do próprio repositório: ele lia e escrevia arquivos, rodava `dotnet`,
`npm` e `docker`, e propunha o próximo passo. Não usei IA no navegador, nem copiei solução de outro
desafio.

## Como usei

Combinei, antes de começar, trabalhar **por etapas com commits convencionais**, para eu poder
revisar cada parte em vez de receber tudo de uma vez:

1. `chore` — estrutura do monorepo e Docker
2. `feat(core)` — domínio, motor de busca e validação
3. `test(core)` — testes das regras
4. `feat(api)` — endpoint, carga dos dados e erro 400
5. `test(api)` — integração e dados reais
6. `feat(web)` — tela em React
7. `docs` — este arquivo e o README

Em cada etapa eu li o diff, rodei os testes e o build, e só então commitei. O que a IA entregou foi
código e testes; a validação e as decisões foram minhas (abaixo).

## O que eu decidi (não a IA)

- **Docker Compose para tudo.** A máquina tem outro SDK .NET, e o desafio pede .NET 8 — preferi
  fixar a versão dentro de Docker a instalar um SDK extra.
- **Idioma dos identificadores em inglês**, com o contrato em português isolado nas bordas.
- **Ordenação de `naoElegiveis`** com a mesma regra dos elegíveis (menos aulas, depois nome).
- **Formato de erro** `{ mensagem, erros[] }` devolvendo todos os problemas de uma vez.
- **Escopo extra de testes** (integração + dados reais), além do mínimo pedido.
- **Layout `api/` + `web/`** e a separação da regra de negócio em um projeto sem ASP.NET.

## Casos em que a IA errou e como percebi

### 1. Identificadores em português com "remendos" para acento

A IA começou escrevendo o domínio em português — `Professor`, `Disponibilidade`, `AulaMarcada` — e,
para fugir de acento em identificador, batizou tipos como `Dia` e `Horario`. Ficou **inconsistente**:
um vocabulário em português com nomes mutilados para caber em `grep`/encoding. Questionei ("por que
não em inglês, não é boa prática?") e trocamos para identificadores em inglês, deixando o português
só onde é contrato (`[JsonPropertyName]` no C#, tipos de wire na tela). Renomeei os projetos de
`EncontraProfessor.*` para `TeacherFinder.*` antes de seguir.

### 2. A grade de `:00`/`:30` quebrava com blocos desalinhados

Na primeira versão do cálculo, os candidatos saíam de um laço que começava no início da faixa e
**pulava** o que não estivesse na grade (`if (t % 30 != 0) continue;`). Com uma faixa começando em
`18:15`, isso descartava **todos** os candidatos (18:15, 18:45, 19:15… nunca batem em 30) e o
professor aparecia como "Sem horário em comum" mesmo tendo horário livre.

Percebi conferindo à mão o caso do Fábio, que tem bloco `18:15–20:00`: o esperado era
`["18:30","19:00"]` e o laço nunca chegaria lá. A correção foi **alinhar o primeiro candidato para
cima** (primeiro múltiplo de 30 ≥ início da faixa) e adicionei um teste específico para isso
(`Aula_precisa_caber_inteira_no_bloco_de_disponibilidade`, com bloco começando 18:15).

### 3. Ordenação de `naoElegiveis` contrariando o combinado

A primeira leva ordenava os não elegíveis **só alfabeticamente**, apesar de eu já ter decidido usar a
mesma regra dos elegíveis (menos aulas primeiro). Peguei revisando o diff antes de commitar: o
código não fazia o que estava combinado. Corrigi para `OrderBy(ClassesPerWeek).ThenBy(Nome)`, com
desempate por `id`, e cobri com dois testes (unitário e de integração).

### 4. Solução em `.slnx`, que o .NET 8 não lê

A IA criou o arquivo de solução usando o SDK local (10), que gerou o formato novo `.slnx`. O SDK 8
usado no Docker **não entende `.slnx`**, então o build falharia lá dentro. Percebi pelo nome/`ls` do
diretório e mandei escrever um `.sln` clássico à mão.

### 5. Código que não compilava

Duas vezes o compilador pegou erro que a IA não previu:

- faltou o `global using Xunit;` nos projetos de teste → 20 erros `CS0246: FactAttribute não
  encontrado` (resolvido com um `Usings.cs`);
- `??` entre `ModelErrorCollection` e `List<ModelError>` no tratamento de erro → `CS0019` (resolvido
  trocando por `Where(...).SelectMany(...)`).

Nos dois casos o conserto veio da **mensagem do compilador**, não de tentativa e erro.

### 6. Mensagem de erro em inglês vazando na API

Chamei o endpoint com um JSON quebrado (`{"idioma": `) e a resposta 400 veio com texto do framework
em inglês: *"The request field is required."*, *"Expected depth to be zero at the end of the JSON
payload…"*. Como o README pede mensagem clara e o app é em português, troquei por uma mensagem em
pt-BR no mesmo formato `{ mensagem, erros[] }`.

### 7. Um "bug" que era do ambiente, não do código

Rodando os testes localmente (assembly `net8.0` sob o runtime 10, via *roll-forward*), **9 testes de
integração falharam** com a mesma exceção: `The PipeWriter 'ResponseBodyPipeWriter' does not
implement PipeWriter.UnflushedBytes`. O reflexo seria "consertar" o endpoint. O que me segurou foi o
rastro: **todos** os endpoints davam o mesmo erro, inclusive o `/health`, que não tem regra nenhuma.
Isso aponta para o `TestHost` (ASP.NET Core 8) brigando com o `System.Text.Json` 10, não para a
busca. Confirmei rodando em Docker no .NET 8 real: **44 testes passando**. Não mexi no código — e
deixei o aviso no README para ninguém cair nessa.

## O que eu conferi por conta própria

- `docker compose --profile testes run --rm testes` → 28 + 16 = **44 testes**, 0 falhas.
- O exemplo do enunciado, batido à mão no `curl`, comparado campo a campo com o esperado
  (Ana `["18:00"]`, Diego `["18:00","18:30","19:00"]`).
- Os 400 (campos faltando, dia inválido, `25:00`, `20h`, duração 45, fim antes do início, JSON
  quebrado) e o `GET /health`.
- Dois cenários que eu montei contra o `professores.json` para testar a regra 3 (interseção de dias)
  e a regra 5 (encostar), inclusive o Heitor, que tem a quarta inteira ocupada entre 18:00 e 20:00 e
  por isso cai em "Sem horário em comum".
