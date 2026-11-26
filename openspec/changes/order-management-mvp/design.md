# Design

## Context

Veja `proposal.md` para a motivação e as specs em `specs/` para o comportamento exigido. A solução `Lab.Orders` existe apenas como esqueleto: quatro projetos (`Domain`, `Application`, `Infrastructure`, `Api`) e quatro projetos de teste, todos em .NET 10 / C# 14 com nullable habilitado, sem código de negócio. O único arquivo de domínio é um `Class1.cs` gerado. Não há specs aprovadas, banco de dados ou dependências de persistência configuradas — toda a estrutura de domínio será introduzida por esta change.

## Goals / Non-Goals

**Goals:**

- Implementar as invariantes de desconto e taxa de entrega em um único lugar, compartilhado por catálogo e pedido.
- Manter as camadas desacopladas na ordem `Domain -> Application -> Infrastructure -> Api`.
- Produzir um teste automatizado por cenário das specs, na camada adequada.

**Non-Goals:**

- Persistência em banco de dados externo, migrações ou schema SQL neste MVP.
- Integração com mapas, gateways de pagamento ou serviços de frete.
- Histórico, auditoria ou reprocessamento de pedidos antigos.

## Decisions

### 1. Desconto como value object discriminado, não dois campos anuláveis

Um `Desconto` será um value object com duas variantes mutuamente exclusivas: `Percentual` e `Monetario`. A variante é escolhida na construção, o que torna impossível representar "as duas modalidades ao mesmo tempo" — a regra deixa de depender de `if` espalhados.

- Alternativa considerada: dois campos `decimal?` (percentual e valor) com validação no caso de uso. Rejeitada porque a invariante poderia ser violada por qualquer novo código que construísse o objeto sem passar pelo validador.
- Consequência: a recusa de "modalidades duplas" passa a acontecer na fronteira de entrada (DTO -> value object), antes de qualquer cálculo.

### 2. Valores monetários como value object central com arredondamento único

`Money` encapsula `decimal` com duas casas decimais e arredondamento `MidpointRounding.AwayFromZero`. Percentual também é um value object com faixa válida de 0 a 100. Todo cálculo (`preco final`, `desconto`, `subtotal`, `taxa`, `total`) usa esses value objects, evitando divergência de arredondamento entre `product-catalog` e `pricing`.

- Alternativa considerada: `decimal` solto nas entidades. Rejeitada porque cada camada poderia arredondar em um momento diferente do cálculo.

### 3. Pedido guarda snapshot do preço final no item

O item do pedido registra `SKU`, `quantidade`, `precoFinalUnitario` e o desconto unitário vigente no momento do registro. O pedido referencia o catálogo apenas para resolver o SKU; mudanças posteriores no catálogo não alteram pedidos já registrados.

- Alternativa considerada: referência viva ao produto do catálogo. Rejeitada porque recalcularia pedidos antigos e quebraria a estabilidade do total persistido.
- Trade-off: o item duplica dados do catálogo; a consistência é garantida no ato do registro, não por sincronização posterior.

### 4. Taxa de entrega como política pura no domínio

`delivery-fee` é implementado como uma política determinística do domínio, com constantes para R$ 12,00, 15 km, R$ 1,00/km e 100 km. Não há `HttpClient`, configuração externa nem serviço de mapas. A política é chamada pelo caso de uso de registro de pedido e sua rejeição de distância é traduzida em erro de validação.

- Alternativa considerada: taxa configurável por arquivo. Rejeitada no MVP porque não há requisito de variação por região ou horário.

### 5. Repositórios em memória na Infrastructure

As interfaces de repositório ficam em `Application` e a implementação em `Infrastructure` usa coleções em memória com chave por `Id`/`SKU`. Isso permite testar a camada de infraestrutura sem banco externo e mantém o foco nas regras de negócio.

- Alternativa considerada: EF Core com SQLite. Adiada porque exigiria pacotes, schema e migrações que não fazem parte do escopo; a troca futura é local ao projeto de infraestrutura, já que as interfaces estão em `Application`.

### 6. Validação nas fronteiras e mapeamento de erro na API

Invariantes de domínio são verificadas em construtores/fábricas (fail fast). Casos de uso verificam existência de cliente e SKU via repositórios. A API traduz falhas de domínio e validação em respostas HTTP 400 com `ProblemDetails`. O projeto `Lab.Orders.Api` hoje usa `Microsoft.NET.Sdk`; será necessário migrar para `Microsoft.NET.Sdk.Web` (ou adicionar `FrameworkReference` ao ASP.NET Core) para expor endpoints.

### 7. Ordem de implementação por camada

Domain -> Application -> Infrastructure/API -> validação final, espelhando o pipeline de agentes do lab. Cada cenário das specs vira um teste: regras puras em `Lab.Orders.Domain.UnitTests`, orquestração em `Lab.Orders.Application.UnitTests`, repositórios em `Lab.Orders.Infrastructure.IntegrationTests` e endpoints em `Lab.Orders.Api.FunctionalTests`.

### 8. Validação de documento com dígitos verificadores

`Documento` valida formato e dígitos verificadores de CPF e CNPJ (assunção registrada em `proposal.md`), e não apenas o padrão de caracteres.

- Alternativa considerada: validar somente o formato e o tipo. Rejeitada porque a spec exige documento válido, não apenas bem formatado.
- Trade-off: mais regras no domínio e mais casos de teste; não há dependência externa.

## Risks / Trade-offs

- [Risco] Suposições ainda não confirmadas mudarem o comportamento esperado (desconto acima da base, validação de documento, estrutura do endereço, enum de pagamento, modalidade dupla no cadastro) → Mitigação: todas estão marcadas em `proposal.md`; confirmar com o humano antes do apply e ajustar specs se necessário.
- [Risco] Regras de desconto duplicadas entre catálogo e pedido → Mitigação: value objects `Desconto` e `Money` são a única implementação das regras; specs diferentes apenas descrevem os pontos de aplicação.
- [Risco] Diferença de arredondamento entre cálculo esperado e obtido nos testes → Mitigação: política única de arredondamento no value object `Money`, com testes parametrizados nos cenários de `pricing`.
- [Risco] Snapshot do item divergir do catálogo e gerar dúvida sobre qual preço é oficial → Mitigação: documentar no modelo que o preço do pedido é o preço da compra; o catálogo permanece como fonte apenas no registro.
- [Risco] Projeto de API sem o SDK web configurado → Mitigação: primeira tarefa de API ajusta o `csproj` antes de criar endpoints.

## Migration Plan

Greenfield: não há dados nem código de produção a migrar. A entrega acontece por camada, cada uma compilando e com testes verdes antes da seguinte. Rollback consiste em remover os arquivos introduzidos pela change; não há estado persistido para reverter.

