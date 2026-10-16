# Encontre um Professor — desafio técnico

Uma escola de idiomas precisa descobrir quais professores conseguem atender um aluno, dado o
idioma, os dias, a faixa de horário e a duração da aula. Este repositório entrega a **API em C#
(.NET 8)** e a **tela em React + TypeScript**.

- **API:** `POST /api/professores/busca` — responde quem serve, em quais horários, e quem ficou de
  fora com o motivo.
- **Tela:** formulário do pedido, lista de professores disponíveis e lista dos que ficaram de fora.

O enunciado original está no [anexo](#anexo-enunciado-original). O registro de uso de IA está em
[`USO_DE_IA.md`](USO_DE_IA.md).

---

## Como rodar

### Com Docker Compose (recomendado)

Sobe a API em .NET 8 e a tela juntos, sem precisar de SDK instalado na máquina.

Pré-requisito: Docker com Compose.

```bash
docker compose up --build
```

- Tela: <http://localhost:5173>
- API: <http://localhost:8080> (verificação rápida: `GET /health` → `{"status":"ok"}`)

Para parar: `Ctrl+C`, ou `docker compose down` em outro terminal.

Rodar os testes automatizados (image com o SDK 8):

```bash
docker compose --profile testes run --rm testes
```

### Sem Docker

Pré-requisitos: **.NET 8 SDK** e **Node 20+**.

```bash
# API — sobe em http://localhost:8080
cd api
dotnet run --project src/TeacherFinder.Api
```

```bash
# Tela — sobe em http://localhost:5173
cd web
npm install
npm run dev
```

Testes em C#:

```bash
cd api
dotnet test
```

> **Atenção:** use o .NET 8 SDK. Se a máquina só tiver um SDK mais novo e os testes de integração
> rodarem via *roll-forward* no runtime 10, o `TestHost` quebra com
> `PipeWriter 'ResponseBodyPipeWriter' does not implement PipeWriter.UnflushedBytes` — é artefato do
> runtime, não do código. No .NET 8 (Docker) os 44 testes passam.

### Configuração

| Variável | Onde | Padrão | Para que serve |
|---|---|---|---|
| `VITE_API_URL` | tela | `http://localhost:8080` | URL da API |
| `CORS_ORIGEM` | API | `http://localhost:5173` | origem(ns) liberada(s), separadas por vírgula |
| `PROFESSORES_ARQUIVO` | API | `Data/professores.json` | caminho do arquivo de professores |
| `ASPNETCORE_HTTP_PORTS` | API | `8080` (Docker) | porta da API |

---

## A API

### `POST /api/professores/busca`

**Requisição**

```json
{
  "idioma": "Espanhol",
  "dias": ["segunda"],
  "inicioFaixa": "18:00",
  "fimFaixa": "20:00",
  "duracaoMinutos": 60
}
```

**Resposta 200**

```json
{
  "professores": [
    { "id": 1, "nome": "Ana",   "aulasNaSemana": 1, "horarios": ["18:00"] },
    { "id": 4, "nome": "Diego", "aulasNaSemana": 1, "horarios": ["18:00", "18:30", "19:00"] }
  ],
  "naoElegiveis": [
    { "id": 3, "nome": "Carla",   "motivo": "Não ensina o idioma" },
    { "id": 5, "nome": "Elisa",   "motivo": "Não ensina o idioma" },
    { "id": 6, "nome": "Fábio",   "motivo": "Não ensina o idioma" },
    { "id": 2, "nome": "Bruno",   "motivo": "Não ensina o idioma" },
    { "id": 7, "nome": "Gabriela","motivo": "Não ensina o idioma" },
    { "id": 8, "nome": "Heitor",  "motivo": "Não ensina o idioma" }
  ]
}
```

`naoElegiveis` sempre vem preenchida com todos os professores que não atenderam, com um dos três
motivos exatos do enunciado.

**Resposta 400** — sempre no mesmo formato, com todas as mensagens de uma vez:

```json
{
  "mensagem": "Dados inválidos.",
  "erros": [
    "O idioma é obrigatório.",
    "Dia inválido: 'feriado'.",
    "Horário inválido: '25:00'. Use o formato HH:mm.",
    "A duração da aula deve ser 30, 60 ou 90 minutos."
  ]
}
```

Corpo ausente, JSON malformado ou tipo errado também respondem 400 nesse formato.

Exemplo com `curl`:

```bash
curl -s -X POST http://localhost:8080/api/professores/busca \
  -H 'Content-Type: application/json' \
  -d '{"idioma":"Espanhol","dias":["segunda"],"inicioFaixa":"18:00","fimFaixa":"20:00","duracaoMinutos":60}'
```

### `GET /health`

Retorna `{"status":"ok"}`. Serve para checagem rápida e para orquestração.

---

## Organização do código

```
api/
  TeacherFinder.sln
  src/
    TeacherFinder.Core/          # regra de negócio, sem nada de ASP.NET
      Model/                     # Weekday, TimeOfDay, AvailabilityWindow, ScheduledClass, Teacher
      Search/                    # TeacherSearch, SearchRequestValidator, SearchRequest/Result, motivos
    TeacherFinder.Api/           # camada HTTP
      Controllers/TeachersController.cs   # só traduz HTTP e delega ao Core
      Contracts/                 # DTOs do contrato (chaves em português via JsonPropertyName)
      Infrastructure/            # TeachersJsonReader + TeacherCatalog (carrega o professores.json)
  tests/
    TeacherFinder.Core.Tests/    # 28 testes das regras e da validação (não sobem API)
    TeacherFinder.Api.Tests/     # 16 testes de integração + leitura do arquivo real
web/                             # tela Vite + React + TypeScript
  src/
    api.ts                       # fetch + conversão do contrato para o modelo da tela
    types.ts                     # tipos do contrato (JSON) e do modelo interno
    components/                  # SearchForm, TeacherList, IneligibleList
professores.json                 # fonte única dos dados (na raiz, como veio no desafio)
docker-compose.yml               # api + web (+ perfil de testes)
```

A regra de negócio mora em `TeacherFinder.Core`, que **não referencia ASP.NET**. O controller só
valida o transporte, chama o motor e monta a resposta. Por isso os testes das regras rodam sem
subir a API.

---

## Como cada regra foi implementada

Tudo em `api/src/TeacherFinder.Core/Search/TeacherSearch.cs`.

| Regra | Onde |
|---|---|
| 1. Precisa ensinar o idioma | `TeachesLanguage` (comparação sem caixa, com `Trim`) |
| 2. Agenda bloqueada fica de fora | checagem de `teacher.ScheduleBlocked` |
| 3. Mesmo horário de início em todos os dias | `IsFree` roda para cada dia pedido e o horário só entra se passar em **todos** |
| 4. Cabe inteira no bloco e na faixa | `AvailabilityWindow.FitsWithin` + limites da faixa no laço de candidatos |
| 5. Não sobrepõe aula marcada; encostar pode | `ScheduledClass.OverlapsWith` (comparação estrita dos dois lados, então encostar não conflita) |
| 6. Início em `:00` ou `:30` | candidatos em passo de 30 min, alinhados para cima a partir do início da faixa |
| 7. Ordenação | `OrderBy(ClassesPerWeek).ThenBy(Name, comparador pt-BR).ThenBy(Id)` |
| Motivo do `naoElegiveis` | `IneligibilityReasons` — a ordem dos `if` com `continue` garante "o primeiro motivo nessa ordem" |

**Exemplo do encostar (regra 5):** Ana tem aula na segunda das 19:00 às 20:00. Na busca de segunda,
18:00–20:00, 60 min, só sobra 18:00 (18:30 e 19:00 bateriam na aula). Confere com o enunciado.

---

## Decisões que tomei

Onde o enunciado não definiu, decidi e registro aqui.

**Idioma do código.** Identificadores (tipos, métodos, variáveis, pastas) em inglês; em português
ficam apenas as strings do contrato e dos dados (`idioma`, `naoElegiveis`, `motivo`, os dias, os
motivos). No C# isso é explícito com `[JsonPropertyName]`; na tela, `types.ts` tem o tipo do
contrato (chaves em português) e um tipo interno em inglês, com a conversão em `api.ts`. Assim o
contrato fica travado por teste e o código segue o vocabulário de mercado.

**Ordenação de `naoElegiveis`.** O enunciado só define a ordem dos elegíveis. Apliquei a **mesma
regra** (menos aulas na semana, depois nome) para manter a lista coerente com a de cima.
Desempate final por `id`, para a saída ser determinística.

**Comparação de nomes.** Comparação pt-BR, então `Fábio` vem antes de `Felipe` (o acento não conta
como letra separada). Desempate por `id`.

**Blocos desalinhados e a grade de `:00`/`:30`.** A regra 6 diz que a aula começa em hora cheia ou
meia hora; os dados têm blocos que começam em `18:15` (Fábio). Então eu **alinho o primeiro
candidato para cima**: com faixa começando 18:15, o primeiro horário testado é 18:30. Sem isso, um
bloco em `18:15` nunca teria aula válida. Um teste cobre exatamente esse caso
(`Aula_precisa_caber_inteira_no_bloco_de_disponibilidade`).

**Dias.** Aceito as formas do enunciado e, por conveniência, também as acentuadas (`terça`,
`sábado`) e qualquer caixa (`Terça`, `SÁBADO`). Dias repetidos são deduplicados.

**Validação acumulativa.** A API devolve **todos** os erros do pedido de uma vez, não só o primeiro,
para a tela mostrar tudo junto. O corpo é `{ mensagem, erros[] }`. Escolhi este formato (em vez de
`ProblemDetails`) por ser direto de consumir na tela.

**Erros de corpo.** Corpo ausente, JSON malformado ou tipo errado caem em 400 no mesmo formato, com
mensagem em português — as mensagens padrão do framework saem em inglês, então eu as substituo.

**Carga dos dados.** `professores.json` é a fonte única: o `csproj` o copia da raiz para a saída do
build, sem duplicar o arquivo no repositório. Carrego uma vez e mantenho em memória, como o
enunciado permite. O caminho é configurável (`Professores:Arquivo` / `PROFESSORES_ARQUIVO`), o que
os testes usam para apontar ao arquivo real.

**Validação do arquivo.** A leitura confere dia e horário e falha com uma mensagem que aponta o
professor e o campo (`Professor 6: horário inválido '25:00'`). É fronteira de dados: vale falhar
alto em vez de gerar resultado errado.

**`naoElegiveis` sempre preenchida.** O enunciado marca isso como diferencial; entreguei como
comportamento padrão, sem flag.

**Docker Compose para tudo.** Como o desafio pede .NET 8 e a máquina tem outro SDK, o Compose é o
caminho garantido: `sdk:8.0` para build e `aspnet:8.0` para runtime. A tela roda em modo de
desenvolvimento (`node:22-alpine`) — é a forma de servir rápido, sem passo de build de produção.

**Extras pequenos.** `GET /health`; CORS liberado para a tela (configurável); animação nenhuma,
CSS puro (sem biblioteca de UI).

---

## Testes

**44 testes em C#**, todos passando em .NET 8:

- **28 unitários** (`TeacherFinder.Core.Tests`) — as regras da busca e a validação. **Não dependem de
  subir a API** (atendem ao requisito de no mínimo 5, com folga).
- **16 de integração** (`TeacherFinder.Api.Tests`) — o endpoint em memória com `WebApplicationFactory`,
  incluindo o exemplo do enunciado ponta a ponta, a ordem e os motivos de `naoElegiveis`, a mistura de
  motivos com os dados reais, os 400 (campos e JSON quebrado) e o `/health`. Mais a leitura do
  `professores.json` real (8 professores, mapeamento de blocos e aulas, e falhas de arquivo torto).

Casos de fronteira cobertos, entre outros: encostar em aula marcada; sobreposição; interseção de
horários entre vários dias; bloco começando em `18:15`; aula que não cabe na faixa; durações de 30,
60 e 90; professor que não ensina o idioma *e* está bloqueado (o motivo é o do idioma, que vem
primeiro); ordenação com empate e acento; idioma sem nenhum professor.

```bash
# em Docker (.NET 8)
docker compose --profile testes run --rm testes

# ou com o .NET 8 SDK instalado
cd api && dotnet test
```

---

## O que ficou faltando

- **Testes da tela.** Não há teste automatizado de frontend (nem de componente, nem de navegador). A
  tela foi conferida manualmente, contra a API rodando. É a lacuna mais relevante se houvesse mais
  tempo.
- **Build de produção da tela.** O `web` roda com o servidor de desenvolvimento do Vite
  (`npm run dev`), que é o suficiente para o desafio, mas não é como se publica: faltaria um build
  (`vite build`) servido por Nginx/estático.
- **Persistência.** Tudo em memória, como o enunciado permite; não há banco nem escrita de dados.
- **Sem autenticação, paginação ou logs estruturados.** Fora do escopo pedido.
- **Sem cache de busca ou índice por idioma.** Para 8 professores, varrer a lista é irrelevante.
- **Casos do enunciado que ficaram de fora por não existirem nos dados:** professor que ensina vários
  idiomas e blocos que se encostam no mesmo dia (a garantia diz que não acontece; o código não
  depende disso, mas não há teste específico porque não dá para montar o caso com o arquivo real).

---

## Anexo: enunciado original

<details>
<summary>Clique para ver o enunciado completo</summary>

### Contexto

Uma escola de idiomas precisa encontrar professores disponíveis para cada novo aluno. O aluno
informa o idioma, os dias da semana, a faixa de horário em que pode ter aula e a duração da aula; o
sistema devolve os professores que conseguem atendê-lo e em quais horários.

Você vai construir uma API em C# que resolve essa busca e uma tela em React para usá-la.

**Estimativa:** 6 a 8 horas de trabalho. O prazo de entrega é informado no convite do processo
seletivo.

### Dados

Os professores estão em `professores.json`. Carregue-os desse arquivo; você pode mantê-los em
memória.

| Campo | Descrição |
|---|---|
| `id`, `nome` | Identificação do professor |
| `idiomas` | Idiomas que ele ensina |
| `agendaBloqueada` | Quando `true`, o professor não pode receber novos alunos |
| `disponibilidade` | Blocos semanais em que ele pode dar aula (`dia`, `inicio`, `fim`) |
| `aulasMarcadas` | Aulas que ele já tem na semana (`dia`, `inicio`, `duracaoMinutos`) |

**Garantias dos dados:** horários em `HH:mm`; nada atravessa a meia-noite; blocos do mesmo dia nunca
se sobrepõem nem se encostam; dias da semana `segunda`..`domingo`.

### Regras da busca

1. O professor precisa ensinar o idioma pedido.
2. Professor com agenda bloqueada não aparece.
3. **O aluno quer horário fixo:** o mesmo horário de início precisa estar livre em **todos** os dias
   pedidos.
4. A aula precisa caber **inteira** dentro de um bloco de disponibilidade do professor e dentro da
   faixa pedida pelo aluno.
5. A aula não pode se sobrepor a uma aula já marcada. **Encostar é permitido:** uma aula que termina
   às 19:00 permite outra começando às 19:00.
6. A aula só pode começar em hora cheia ou meia hora (`:00` ou `:30`).
7. **Ordenação:** primeiro quem tem menos aulas marcadas na semana; em caso de empate, ordem
   alfabética do nome. Os horários de cada professor vêm em ordem crescente.

**Diferencial:** preencher `naoElegiveis` com o motivo de cada professor que ficou de fora, usando
exatamente um destes textos: `Não ensina o idioma`, `Agenda bloqueada`,
`Sem horário em comum nos dias pedidos`. Quando houver mais de um motivo, informe o primeiro nessa
ordem.

### Validação

A API responde **400** com uma mensagem clara quando: o idioma está vazio; nenhum dia foi informado;
algum dia é inválido; algum horário está fora do formato `HH:mm`; o fim da faixa é igual ou anterior
ao início; a duração é diferente de 30, 60 ou 90 minutos.

### Tela (React, de preferência com TypeScript)

- **Formulário:** idioma (Inglês, Espanhol ou Francês), dias da semana, faixa de horário e duração.
- **Resultado:** cada professor com os horários possíveis e a quantidade de aulas na semana.
- **Estados:** carregando, nenhum professor encontrado, falha de comunicação com a API e erros de
  validação devolvidos pela API.
- **Diferencial:** mostrar os professores que ficaram de fora e o motivo.

### Testes

No mínimo **5 testes automatizados em C#** sobre as regras da busca. Os testes não devem depender de
subir a API.

### Entrega

`README.md` (como rodar, decisões, o que ficou faltando) e `USO_DE_IA.md` (ferramentas de IA usadas,
como usou e pelo menos um caso em que a IA errou e como percebeu).

</details>
