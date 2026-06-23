# Relatório Técnico — Sistema de Otimização de Malha de Distribuição

**Projeto:** Entrega Máxima Logística S.A. — Trabalho Prático de Teoria dos Grafos
**Plataforma:** .NET 9.0 (C#), aplicação de console interativa
**Namespace raiz:** `graph_tp`

---

## Sumário

- [Relatório Técnico — Sistema de Otimização de Malha de Distribuição](#relatório-técnico--sistema-de-otimização-de-malha-de-distribuição)
  - [Sumário](#sumário)
  - [1. Visão Geral](#1-visão-geral)
  - [2. Estrutura do Projeto](#2-estrutura-do-projeto)
  - [3. Modelagem do Domínio](#3-modelagem-do-domínio)
    - [`Vertex` (hub)](#vertex-hub)
    - [`Edge` (rota)](#edge-rota)
    - [`Graph` (fachada)](#graph-fachada)
    - [Interpretação do grafo: direcionado vs. não direcionado](#interpretação-do-grafo-direcionado-vs-não-direcionado)
  - [4. Representação dos Grafos](#4-representação-dos-grafos)
    - [4.1 As três representações implementadas](#41-as-três-representações-implementadas)
      - [Lista de Adjacência — `AdjacencyListRepresentation`](#lista-de-adjacência--adjacencylistrepresentation)
      - [Matriz de Adjacência — `AdjacencyMatrixRepresentation`](#matriz-de-adjacência--adjacencymatrixrepresentation)
      - [Matriz de Incidência — `IncidenceMatrixRepresentation`](#matriz-de-incidência--incidencematrixrepresentation)
    - [4.2 A lógica de seleção automática](#42-a-lógica-de-seleção-automática)
    - [4.3 Conversão de arquivos DIMACS](#43-conversão-de-arquivos-dimacs)
      - [Formato esperado](#formato-esperado)
      - [Etapas da conversão (`LoadFromFile`)](#etapas-da-conversão-loadfromfile)
      - [Escrita (`SaveToFile`)](#escrita-savetofile)
  - [5. Algoritmos Implementados](#5-algoritmos-implementados)
    - [Problema I — Roteamento de Menor Custo (Bellman-Ford)](#problema-i--roteamento-de-menor-custo-bellman-ford)
    - [Problema II — Capacidade Máxima de Escoamento (Ford-Fulkerson)](#problema-ii--capacidade-máxima-de-escoamento-ford-fulkerson)
    - [Problema III — Expansão da Rede de Comunicação (Prim)](#problema-iii--expansão-da-rede-de-comunicação-prim)
    - [Problema IV — Agendamento de Manutenções sem Conflito (Welsh-Powell)](#problema-iv--agendamento-de-manutenções-sem-conflito-welsh-powell)
    - [Problema V — Rota Única de Inspeção (Fleury e Backtracking)](#problema-v--rota-única-de-inspeção-fleury-e-backtracking)
      - [Cenário A — Percurso de Rotas: Caminho/Circuito Euleriano (Fleury)](#cenário-a--percurso-de-rotas-caminhocircuito-euleriano-fleury)
      - [Cenário B — Percurso de Hubs: Ciclo Hamiltoniano (Backtracking)](#cenário-b--percurso-de-hubs-ciclo-hamiltoniano-backtracking)
  - [6. Componentes de Suporte (Utils)](#6-componentes-de-suporte-utils)
    - [`QueryLogger` — estrutura de logging](#querylogger--estrutura-de-logging)
    - [`OutputFormatter` — estrutura de saída](#outputformatter--estrutura-de-saída)
  - [7. Como Utilizar o Sistema](#7-como-utilizar-o-sistema)
    - [Pré-requisitos](#pré-requisitos)
    - [Execução](#execução)
    - [Fluxo típico de uso](#fluxo-típico-de-uso)
    - [Preparando seus próprios dados](#preparando-seus-próprios-dados)

---

## 1. Visão Geral

O sistema modela a malha logística da empresa fictícia **Entrega Máxima Logística S.A.** como um grafo, onde os vértices representam *hubs* (centros de distribuição) e as arestas representam rotas com custo e, opcionalmente, capacidade.

A partir dessa modelagem, o programa resolve cinco problemas clássicos de teoria dos grafos, cada um associado a um algoritmo específico, acessíveis por meio de um menu interativo de console. Os grafos são carregados a partir de arquivos no formato DIMACS, e cada sessão de uso é registrada em arquivos de log.

---

## 2. Estrutura do Projeto

```
tp-grafos/
├── Program.cs                       # Ponto de entrada, menu interativo e orquestração
├── graph_tp.csproj                  # Configuração do projeto (.NET 9, nullable habilitado)
├── graph_tp.sln                     # Solution
│
├── Models/                          # Modelagem do domínio
│   ├── Vertex.cs                    # Vértice (hub)
│   ├── Edge.cs                      # Aresta (rota) com custo e capacidade
│   ├── Graph.cs                     # Fachada do grafo (delega à representação ativa)
│   ├── GraphRepresentationSelector.cs  # Seleciona/cria a representação ideal
│   └── Representations/             # Estruturas de dados de representação
│       ├── IGraphRepresentation.cs       # Contrato comum
│       ├── AdjacencyListRepresentation.cs
│       ├── AdjacencyMatrixRepresentation.cs
│       └── IncidenceMatrixRepresentation.cs
│
├── Algorithms/                      # Um arquivo por algoritmo
│   ├── BellmanFordAlgorithm.cs      # Problema I
│   ├── FordFulkersonAlgorithm.cs    # Problema II
│   ├── PrimAlgorithm.cs             # Problema III
│   ├── WelshPowellAlgorithm.cs      # Problema IV
│   ├── FleuryAlgorithm.cs           # Problema V (caminho/circuito euleriano)
│   └── HamiltonAlgorithm.cs         # Problema V (ciclo hamiltoniano)
│
├── Utils/                           # Componentes de infraestrutura
│   ├── DimacsParser.cs              # Leitura/escrita do formato DIMACS
│   ├── QueryLogger.cs               # Registro de sessão em arquivo de log
│   └── OutputFormatter.cs           # Saída padronizada no console
│
├── Data/                            # Arquivos de entrada (.dimacs) e pseudocódigos
└── logs/                            # Logs gerados em tempo de execução
```

A organização segue uma separação clara de responsabilidades em três camadas:

- **`Models`** — define *o que* é um grafo e como ele é armazenado em memória.
- **`Algorithms`** — define *o que* se calcula sobre o grafo; cada algoritmo é autocontido, com sua própria classe de resultado e método de impressão.
- **`Utils`** — infraestrutura transversal: entrada de dados (DIMACS), saída (console) e auditoria (logs).

`Program.cs` atua como camada de apresentação/orquestração, conectando as três.

---

## 3. Modelagem do Domínio

### `Vertex` (hub)

Representa um centro de distribuição. Possui um identificador inteiro (`_id`) e um nome amigável (`Name`), gerado automaticamente como `Hub_{id}` quando não informado. A identidade do vértice é definida pelo `id`: `Equals` e `GetHashCode` baseiam-se nele, o que permite que o vértice seja usado como chave em dicionários e conjuntos. **Por essa razão, ao longo de todo o sistema o identificador de um vértice é obtido via `GetHashCode()`.**

### `Edge` (rota)

Representa uma rota entre dois hubs. Carrega:

- `Source` / `Target` — vértices de origem e destino;
- `LoadValue` — o **custo** da rota (peso, usado por Bellman-Ford e Prim);
- `Capacity` — a **capacidade** máxima de escoamento (usada por Ford-Fulkerson); quando ausente no arquivo de entrada, assume `double.PositiveInfinity`;
- campos auxiliares de fluxo (`_flow`, capacidade residual) para suportar cenários de rede de fluxo.

A separação entre `LoadValue` (custo) e `Capacity` é o que permite ao mesmo grafo alimentar tanto algoritmos de menor custo quanto algoritmos de fluxo máximo.

### `Graph` (fachada)

`Graph` **não armazena diretamente** vértices e arestas — ele delega a uma implementação de `IGraphRepresentation`. Esse padrão de fachada é central para o projeto: o restante do código (algoritmos, parser) conversa apenas com `Graph`, sem saber qual estrutura de dados está por baixo. Isso permite trocar a representação interna de forma transparente (ver seção 4.2).

Métodos relevantes:

- `AddVertex` / `AddEdge` — montagem do grafo;
- `GetOutgoingEdges` / `GetIncomingEdges`, `GetOutDegree` / `GetInDegree` — navegação e grau;
- `OptimizeRepresentation()` — avalia a densidade e migra para a representação mais adequada;
- `GetRepresentationReport()` — relatório textual de densidade e custo de espaço;
- `Clone()` — cópia profunda do grafo (usada por algoritmos que precisam manipular uma cópia sem alterar o original).

### Interpretação do grafo: direcionado vs. não direcionado

O enunciado modela a malha como um **grafo direcionado e ponderado**, e o sistema preserva essa orientação no armazenamento: cada aresta tem `Source` e `Target` bem definidos, e a representação registra arestas de **saída** e de **entrada** separadamente.

Entretanto, **dois dos cinco problemas são, por natureza, definidos sobre grafos não direcionados** — a Árvore Geradora Mínima (Problema III), o percurso euleriano (Problema V – Cenário A) e o ciclo hamiltoniano (Problema V – Cenário B) são conceitos clássicos da teoria de grafos **não direcionados**. Aplicá-los diretamente sobre a orientação das arestas não faria sentido no contexto do problema.

**Interpretação adotada:** nesses casos, o sistema trata a direção da aresta como **irrelevante**, considerando a rota como uma ligação **bidirecional** entre os dois hubs. Essa interpretação é fisicamente justificável no domínio logístico:

- **Problema III (fibra óptica):** uma rota de fibra que interliga dois hubs serve ao tráfego nos dois sentidos; o que importa é a *conectividade* entre os centros, não o sentido de instalação. Por isso o Prim avalia uma aresta como candidata sempre que ela **cruza o corte** entre a árvore parcial e o restante, independentemente de qual ponta é origem ou destino.
- **Problema V (inspeção de estradas/hubs):** o inspetor percorre estradas físicas, que podem ser trafegadas nos dois sentidos. Assim, tanto o Fleury quanto o backtracking hamiltoniano constroem uma **estrutura de adjacência não direcionada** a partir das arestas: cada aresta `Source → Target` é tratada como conexão entre os dois vértices em ambos os sentidos.

Já os problemas em que a **direção é semanticamente essencial** — **Roteamento de Menor Custo (I)** e **Capacidade Máxima de Escoamento (II)** — respeitam integralmente a orientação das arestas, pois enviar carga de A para B é distinto de enviá-la de B para A.

---

## 4. Representação dos Grafos

Esta é uma das decisões de projeto mais importantes. Em vez de fixar uma única estrutura de dados, o sistema implementa **três representações** sob uma mesma interface e escolhe automaticamente a mais adequada com base nas características do grafo carregado.

### 4.1 As três representações implementadas

Todas implementam a interface `IGraphRepresentation`, que define o contrato comum (`AddVertex`, `AddEdge`, `GetOutgoingEdges`, `GetIncomingEdges`, `GetAllVertices`, `GetAllEdges`, etc.). Isso garante que possam ser usadas de forma intercambiável.

#### Lista de Adjacência — `AdjacencyListRepresentation`

- **Estrutura:** um dicionário `id → lista de arestas de saída`.
- **Custo de espaço:** `O(V + E)`.
- **Vantagem:** extremamente eficiente em memória para grafos **esparsos** (poucas arestas em relação ao número de vértices), que são o caso comum em redes logísticas reais. Iterar sobre os vizinhos de um vértice é proporcional ao seu grau.
- **Desvantagem:** verificar a existência de uma aresta específica `(u, v)` exige varrer a lista de `u`; obter as arestas de entrada (`GetIncomingEdges`) exige varrer todas as listas.

#### Matriz de Adjacência — `AdjacencyMatrixRepresentation`

- **Estrutura:** uma matriz `V × V` onde cada célula `[u, v]` guarda a(s) aresta(s) de `u` para `v`. A matriz é construída sob demanda (*lazy*) por `EnsureBuilt()` e reconstruída apenas quando o grafo é modificado (flag `_dirty`), evitando recomputação desnecessária.
- **Custo de espaço:** `O(V²)`.
- **Vantagem:** ideal para grafos **densos**. A consulta de adjacência e o acesso direto a uma posição são naturais, e tanto arestas de saída quanto de entrada são obtidas percorrendo uma linha ou uma coluna.
- **Desvantagem:** desperdiça memória em grafos esparsos, pois reserva espaço para todas as combinações de vértices, ocupadas ou não.

#### Matriz de Incidência — `IncidenceMatrixRepresentation`

- **Estrutura:** uma matriz `V × E` (linhas = vértices, colunas = arestas). Cada coluna codifica uma aresta: `-1` na origem, `+1` no destino e `2` para laços (origem = destino). Também construída de forma *lazy*.
- **Custo de espaço:** `O(V × E)`.
- **Vantagem:** representa explicitamente a relação vértice–aresta, sendo conveniente para análises orientadas a arestas e para distinguir laços e multiarestas.
- **Desvantagem:** geralmente é a mais custosa em espaço; está disponível na interface, mas não é selecionada automaticamente pelo seletor (que opta entre lista e matriz de adjacência).

### 4.2 A lógica de seleção automática

A escolha entre as representações é centralizada em `GraphRepresentationSelector` e baseada na **densidade** do grafo.

A densidade de um grafo direcionado é calculada como:

```
D = E / (V × (V − 1))
```

onde `E` é o número de arestas e `V` o de vértices (para `V < 2`, define-se `D = 0`).

A regra de decisão é:

```csharp
public const double DenseThreshold = 0.5;

public static RepresentationKind Select(int vertexCount, int edgeCount)
{
    if (vertexCount >= 2 && Density(vertexCount, edgeCount) >= DenseThreshold)
        return RepresentationKind.AdjacencyMatrix;   // grafo denso
    return RepresentationKind.AdjacencyList;          // grafo esparso
}
```

- Se a densidade for **maior ou igual a 50%**, o grafo é considerado **denso** e migra-se para a **matriz de adjacência** (`O(V²)` compensa, pois quase todas as células estarão ocupadas).
- Caso contrário, o grafo é **esparso** e usa-se a **lista de adjacência** (`O(V + E)`, mais econômica).

O método `Graph.OptimizeRepresentation()` aplica essa decisão: ele cria a nova representação, copia vértices e arestas e substitui a estrutura interna — tudo de forma transparente para os algoritmos. Esse método é chamado automaticamente pelo `DimacsParser` logo após a carga do grafo.

O método `Report(...)` produz um diagnóstico legível ao usuário, exibindo o número de vértices e arestas, a densidade percentual, a classificação (denso/esparso), o custo de espaço de cada representação e a estrutura efetivamente selecionada.

> **Resumo da lógica de escolha:** o sistema parte de uma lista de adjacência (padrão econômico), mede a densidade após a carga, e migra automaticamente para a matriz de adjacência somente quando isso é vantajoso. A matriz de incidência fica disponível como alternativa para análises orientadas a arestas, mas não entra na seleção automática.

### 4.3 Conversão de arquivos DIMACS

A entrada de dados é feita por arquivos no formato DIMACS, convertidos para a estrutura interna pelo `DimacsParser` (`Utils/DimacsParser.cs`).

#### Formato esperado

```
V E
<origem> <destino> <custo> [capacidade]
<origem> <destino> <custo> [capacidade]
...
```

- A **primeira linha** contém dois inteiros: o número de vértices `V` e o de arestas `E`.
- Cada **linha seguinte** descreve uma aresta com, no mínimo, três campos: origem, destino e custo. Um quarto campo opcional define a capacidade.

Exemplo (`Data/grafo01.dimacs`):

```
6 12
1 2 2 10      → rota do hub 1 ao hub 2, custo 2, capacidade 10
1 3 3 7
1 4 1 8
...
```

#### Etapas da conversão (`LoadFromFile`)

1. **Validação inicial:** verifica a existência do arquivo, se não está vazio e se a primeira linha tem exatamente dois inteiros (`V E`). Falhas geram `FileNotFoundException` ou `FormatException` com mensagens descritivas.
2. **Criação dos vértices:** cria `V` vértices com identificadores de `1` a `V` (`graph.AddVertex(new Vertex(i))`).
3. **Leitura das arestas:** para cada linha não vazia:
   - separa os campos por espaços;
   - exige ao menos 3 campos (`source target cost`);
   - converte origem/destino para `int` e custo para `double`;
   - se houver um 4º campo, interpreta-o como capacidade (`double`); caso contrário, a capacidade fica como `double.PositiveInfinity`;
   - resolve os vértices de origem e destino via `graph.GetVertex(...)` e valida as referências;
   - cria a aresta `new Edge(sourceNode, targetNode, cost, capacity)` e a adiciona ao grafo.
   - Qualquer inconsistência (campo inválido, vértice inexistente) interrompe a leitura com `FormatException` indicando o número da linha.
4. **Otimização:** ao final, chama `graph.OptimizeRepresentation()`, fechando o ciclo descrito na seção 4.2 — o grafo recém-montado é imediatamente migrado para a representação ideal conforme sua densidade.

#### Escrita (`SaveToFile`)

O parser também permite **exportar** um grafo de volta ao formato DIMACS: escreve a linha de cabeçalho `V E` e, em seguida, uma linha por aresta. Arestas com capacidade infinita são gravadas apenas com `origem destino custo`; arestas com capacidade finita incluem o quarto campo. Isso garante simetria de leitura/escrita (*round-trip*).

---

## 5. Algoritmos Implementados

Cada problema do enunciado é resolvido por um algoritmo dedicado. As subseções abaixo descrevem **a funcionalidade, a implementação e a justificativa da escolha** de cada um.

---

### Problema I — Roteamento de Menor Custo (Bellman-Ford)

**Arquivo:** `Algorithms/BellmanFordAlgorithm.cs` · **Opção de menu:** 2

**Funcionalidade.** Dado um hub de origem, calcula o menor custo de roteamento da origem para **todos** os demais hubs da malha. O resultado (`BellmanFordResult`) contém, para cada vértice, a distância mínima (`Distances`) e o predecessor no caminho ótimo (`Predecessors`), permitindo reconstruir as rotas.

**Implementação.** Segue a formulação clássica:

1. Inicializa todas as distâncias como infinito e a da origem como `0`.
2. Executa `V − 1` iterações de **relaxamento** sobre todas as arestas: para cada aresta `v → w`, se `dist[v] + custo < dist[w]`, atualiza `dist[w]` e registra `v` como predecessor.
3. Inclui uma **otimização de parada antecipada**: se uma iteração completa não produz nenhuma alteração, o algoritmo encerra antes de completar as `V − 1` passagens, pois já convergiu.

**Justificativa da escolha.** Foi escolhido o algoritmo de **Bellman-Ford** para resolver o roteamento de menor custo. Mesmo que o algoritmo de **Dijkstra** seja mais eficiente em termos de tempo, o Bellman-Ford foi escolhido pela **flexibilidade que oferece em lidar com grafos que possuem arestas com pesos negativos**, situação que o Dijkstra não trata corretamente.

**Sobre o vértice de destino.** O enunciado pergunta pelo trajeto mais econômico entre um hub de origem e um de destino. Como o Bellman-Ford é um algoritmo de **origem única para todos os destinos** (*single-source shortest path*), uma única execução já calcula o menor custo da origem informada para **todos os demais hubs** simultaneamente. Por isso, **não é necessário solicitar explicitamente um vértice de destino**: o resultado contém a resposta para qualquer destino que o gerente queira consultar. A partir da origem, basta ler na tabela a distância mínima até o hub desejado, e os predecessores (`Predecessors`) permitem reconstruir a sequência de hubs de qualquer um desses trajetos.

---

### Problema II — Capacidade Máxima de Escoamento (Ford-Fulkerson)

**Arquivo:** `Algorithms/FordFulkersonAlgorithm.cs` · **Opção de menu:** 3

**Funcionalidade.** Dados um hub de **origem (fonte)** e um de **destino (sumidouro)**, determina a **capacidade máxima de escoamento** entre eles — o fluxo máximo que a rede consegue transportar. O resultado (`MaxFlowResult`) traz o valor do fluxo máximo, os caminhos aumentantes encontrados e o fluxo final em cada aresta.

**Implementação.** O algoritmo:

1. Mapeia os vértices para índices e monta uma **matriz de capacidades** `V × V` (capacidades de arestas paralelas são somadas). Exige que **todas as capacidades sejam finitas**, lançando exceção caso contrário.
2. Repetidamente busca um **caminho aumentante** da fonte ao sumidouro por meio de uma **Busca em Largura (BFS)** sobre o grafo residual — a variante conhecida como **Edmonds-Karp**. A BFS considera tanto arestas diretas (capacidade residual `> 0`) quanto **arestas reversas** (para permitir cancelamento de fluxo).
3. Para cada caminho, calcula o **gargalo** (menor capacidade residual ao longo do caminho), atualiza os fluxos diretos e reversos e acumula o fluxo total.
4. Termina quando não há mais caminhos aumentantes; o fluxo acumulado é o máximo.

**Justificativa da escolha.** Foi implementado o algoritmo de **Ford-Fulkerson** por ser adequado para encontrar o fluxo máximo em uma rede de fluxo, permitindo determinar a capacidade máxima de escoamento entre um nó de origem e um nó de destino. A escolha se deve à sua **simplicidade e eficácia em lidar com redes de fluxo**, além de ser capaz de **lidar com grafos direcionados e não direcionados**.

**Escopo atual.** O enunciado menciona também a identificação do **corte mínimo** (o conjunto mínimo de arestas cujo bloqueio estrangularia a rede). Pelo teorema *max-flow min-cut*, o valor do corte mínimo é igual ao do fluxo máximo já calculado, e o corte poderia ser extraído ao final a partir dos vértices alcançáveis pela fonte no grafo residual. **No momento, a implementação entrega o valor do fluxo máximo, os caminhos aumentantes e o fluxo resultante por aresta, mas não enumera explicitamente o conjunto de arestas do corte mínimo.** Optou-se por manter o algoritmo nessa forma nesta etapa do projeto.

---

### Problema III — Expansão da Rede de Comunicação (Prim)

**Arquivo:** `Algorithms/PrimAlgorithm.cs` · **Opção de menu:** 4

**Funcionalidade.** Encontra a **Árvore Geradora Mínima (AGM/MST)** da malha — o subconjunto de rotas que conecta todos os hubs com o **menor custo total de instalação** possível, sem formar ciclos. O resultado (`MinimumSpanningTree`) traz as arestas selecionadas, o custo total e um indicador de conectividade (`IsConnected`).

**Implementação.** A partir de uma raiz (informada pelo usuário), o algoritmo mantém o conjunto `inTree` de vértices já incluídos e, a cada passo:

1. Procura, entre todas as arestas que **cruzam o corte** (uma extremidade dentro e outra fora da árvore), a de **menor custo**.
2. Adiciona essa aresta e o novo vértice à árvore, acumulando o custo.
3. Se em algum momento não existir aresta cruzando o corte, conclui que o **grafo é desconexo** (`IsConnected = false`) e retorna a árvore parcial.

A impressão apresenta o resultado no contexto do problema: rotas selecionadas e custo total de instalação em reais.

**Justificativa da escolha.** Foi utilizado o algoritmo de **Prim** para encontrar a árvore geradora mínima, garantindo a expansão da rede com o menor custo possível. **Não houve necessidade de utilizar o algoritmo de Kruskal**, pois o Prim é **mais eficiente em grafos densos**, que é o caso da rede de comunicação em questão.

---

### Problema IV — Agendamento de Manutenções sem Conflito (Welsh-Powell)

**Arquivo:** `Algorithms/WelshPowellAlgorithm.cs` · **Opção de menu:** 5

**Funcionalidade.** Organiza as manutenções das rotas em **turnos** de modo que rotas que compartilham um mesmo hub **não sejam agendadas no mesmo turno** (evitando conflito de recursos no hub). O resultado (`MaintenanceSchedule`) agrupa as rotas por turno e informa o número mínimo de turnos necessários.

**Implementação.** O problema é modelado como **coloração de grafos**, mas sobre um **grafo de conflitos das arestas**:

1. Cada **rota (aresta)** vira um "nó" do problema de coloração.
2. Dois nós estão em **conflito** se as rotas correspondentes compartilham algum hub (`RoutesShareHub`). Monta-se uma matriz de conflitos e calcula-se o **grau** de cada nó.
3. Aplica-se a heurística de **Welsh-Powell**: ordena os nós por **grau decrescente** e, para cada cor/turno, percorre os nós ainda não coloridos atribuindo a cor atual sempre que não houver conflito com outro nó já marcado com ela.
4. Cada cor corresponde a um turno; o número de cores é o número mínimo de turnos.

**Justificativa da escolha.** Foi implementado o algoritmo de **Welsh-Powell**, um algoritmo de coloração de grafos, que permite organizar as manutenções de forma que **não haja sobreposição de horários**, garantindo que cada manutenção seja realizada sem conflitos. A escolha se deve à sua **acurácia e eficiência em encontrar uma coloração adequada para grafos**, especialmente em problemas de agendamento de tarefas.

---

### Problema V — Rota Única de Inspeção (Fleury e Backtracking)

Este problema possui **dois cenários**, cada um com seu algoritmo.

#### Cenário A — Percurso de Rotas: Caminho/Circuito Euleriano (Fleury)

**Arquivo:** `Algorithms/FleuryAlgorithm.cs` · **Opção de menu:** 6

**Funcionalidade.** Verifica se é possível percorrer **todas as rotas exatamente uma vez** em um único trajeto contínuo (percurso euleriano) e, se for, exibe a sequência de hubs. Distingue:

- **Circuito euleriano** — o trajeto retorna ao hub inicial (todos os vértices têm grau par);
- **Caminho euleriano** — o trajeto não retorna ao início (exatamente 2 vértices de grau ímpar).

O resultado (`EulerianResult`) informa o tipo (`None`/`Path`/`Circuit`), a sequência de vértices e arestas e, em caso de impossibilidade, o motivo.

**Implementação.** Tratando as rotas como **não direcionadas**:

1. Monta uma estrutura de adjacência e conta os **vértices de grau ímpar**. Se houver **3 ou mais**, não existe percurso euleriano (limite é no máximo 2).
2. Verifica se todas as arestas estão em uma **única componente conexa** (busca em profundidade); um grafo desconexo inviabiliza o trajeto único.
3. Aplica o **algoritmo de Fleury**: partindo de um vértice de grau ímpar (se houver) ou de qualquer vértice, escolhe a cada passo uma aresta ainda não usada, **evitando atravessar pontes** (`IsBridge`) sempre que houver alternativa — exatamente o critério que define o método de Fleury.

**Justificativa da escolha.** O algoritmo de **Fleury** é **eficiente para encontrar caminhos eulerianos** em grafos.

**Restrição de escopo — caminho, não circuito.** O Cenário A do enunciado, em sua redação, pede o percurso de todas as rotas exatamente uma vez **com retorno ao ponto de partida**, o que corresponde estritamente a um **circuito euleriano**. Contudo, **não é permitido, no escopo deste trabalho, incluir um algoritmo dedicado à busca de um ciclo/circuito euleriano** — a implementação restringe-se à determinação do **caminho euleriano**. Por essa razão, o `FleuryAlgorithm` classifica o resultado segundo o critério dos vértices de grau ímpar e relata:

- **Caminho euleriano** quando existem exatamente 2 hubs de grau ímpar (todas as rotas são percorridas uma única vez, mas **sem** retornar ao hub inicial);
- a condição de **circuito** (0 vértices de grau ímpar) é apenas **diagnosticada e informada** ao usuário, sem que se utilize um algoritmo específico de busca de circuito euleriano.

Em outras palavras, o sistema verifica a **existência e a viabilidade** do percurso (que é o que o enunciado pede em "verificar a existência e viabilidade dos percursos propostos") e exibe a sequência de hubs do caminho, mas não há um procedimento separado voltado à construção do circuito fechado.

#### Cenário B — Percurso de Hubs: Ciclo Hamiltoniano (Backtracking)

**Arquivo:** `Algorithms/HamiltonAlgorithm.cs` · **Opção de menu:** 7

**Funcionalidade.** Verifica se existe um **ciclo hamiltoniano** — um trajeto que visita **todos os hubs exatamente uma vez** e retorna ao ponto de partida — e, em caso afirmativo, exibe a sequência. O resultado (`HamiltonianResult`) indica a viabilidade, o ciclo e o motivo em caso negativo.

**Implementação.** Usa **backtracking** sobre uma matriz de adjacência booleana:

1. Trata casos triviais (grafo vazio, único vértice).
2. Constrói o caminho recursivamente (`Solve`): a partir do último vértice fixado, testa cada candidato ainda não usado que seja adjacente; ao preencher todas as posições, valida que o último vértice é adjacente ao primeiro (fechando o ciclo).
3. Quando um candidato não leva a solução, **desfaz a escolha** (backtrack) e tenta o próximo.

**Justificativa da escolha.** O **backtracking** é a abordagem adequada para **explorar todas as possibilidades** de caminhos hamiltonianos, tendo em vista que **encontrar um caminho hamiltoniano é um problema NP-completo** — não há, no estado atual do conhecimento, algoritmo eficiente (polinomial) garantido, sendo a busca exaustiva com poda a estratégia natural.

---

## 6. Componentes de Suporte (Utils)

Além do `DimacsParser` (seção 4.3), a pasta `Utils` reúne os componentes que atendem a requisitos não-algorítmicos do projeto: **auditoria** e **saída padronizada**.

### `QueryLogger` — estrutura de logging

Responsável pelo **registro completo de cada sessão** em arquivo, atendendo ao requisito de rastreabilidade. Características:

- **Sessão por arquivo:** `StartSession` cria um arquivo em `logs/` com nome `{grafo}_{timestamp}.log` (ex.: `grafo01_20260623_143015_120.log`), registrando início e, ao encerrar (`Dispose`/`CloseCurrentLog`), o horário de fim — formando blocos `=== Sessao iniciada ===` / `=== Sessao encerrada ===`.
- **Categorias de evento:** o log distingue tipos de entrada por categoria — `USUARIO` (ações do operador), `SISTEMA` (decisões de fluxo), `ALGORITMO` (passos internos da execução), `ERRO` e `GRAFO` (metadados do grafo carregado). Cada linha recebe timestamp.
- **Rastreamento granular dos algoritmos:** cada algoritmo recebe o logger (parâmetro `QueryLogger? logger`) e registra seus passos internos — inicializações, relaxamentos, caminhos aumentantes, seleção de arestas, conflitos, backtracking, etc. Isso permite **auditar e estudar** a execução passo a passo a partir do arquivo de log.
- **Thread-safety:** todas as escritas são protegidas por um `lock`, e o `StreamWriter` usa `AutoFlush` para garantir persistência imediata.
- **Robustez:** implementa `IDisposable`, fechando corretamente o arquivo mesmo em caso de exceção (uso de `try/finally` no `Program.cs`).

### `OutputFormatter` — estrutura de saída

Centraliza a **apresentação no console**, oferecendo um ponto único para a saída padronizada do sistema:

- `PrintSuccess`, `PrintError`, `PrintWarning`, `PrintInfo` — mensagens categorizadas (atualmente uniformizadas via `Console.WriteLine`, mas centralizadas para fácil evolução, p. ex. com cores);
- `PrintGraphInfo(Graph)` — resumo do grafo (contagem de vértices e arestas).

A separação entre `OutputFormatter` (saída ao usuário) e `QueryLogger` (auditoria persistida) garante que a interface com o operador e o registro técnico evoluam de forma independente.

---

## 7. Como Utilizar o Sistema

### Pré-requisitos

- **.NET SDK 9.0** instalado.

### Execução

A partir da raiz do projeto:

```bash
dotnet run
```

O programa exibe um banner e o **menu principal**. As opções são:

| Opção | Ação |
|-------|------|
| **1** | Carregar grafo (lista os arquivos `.dimacs` da pasta `Data` para seleção) |
| **2** | Bellman-Ford — roteamento de menor custo (pede o hub de origem) |
| **3** | Ford-Fulkerson — capacidade máxima de escoamento (pede fonte e sumidouro) |
| **4** | Prim — expansão da rede / AGM (pede o hub raiz) |
| **5** | Welsh-Powell — agendamento de manutenções |
| **6** | Euleriano (Fleury) — percurso de todas as rotas |
| **7** | Hamiltoniano — percurso de todos os hubs |
| **8** | Sair |

### Fluxo típico de uso

1. Escolha a **opção 1** e selecione um arquivo DIMACS da pasta `Data`. O sistema carrega o grafo, exibe vértices/arestas e o **relatório de representação** (densidade e estrutura selecionada).
2. Escolha um dos algoritmos (opções 2 a 7). Quando necessário, o programa solicita os vértices de **origem** e/ou **destino**, validando se existem no grafo.
3. O resultado é impresso no console e a execução completa é registrada no arquivo de log correspondente em `logs/`.
4. Repita à vontade; ao terminar, use a **opção 8** para encerrar (o log da sessão é fechado corretamente).

> **Observação:** tentar executar qualquer algoritmo (opções 2–7) sem antes carregar um grafo (opção 1) resulta em uma mensagem de erro orientando a carga prévia.

### Preparando seus próprios dados

Para testar com grafos próprios, basta criar um arquivo `.dimacs` na pasta `Data` seguindo o formato descrito na [seção 4.3](#43-conversão-de-arquivos-dimacs): primeira linha com `V E`, demais linhas com `origem destino custo [capacidade]`.
