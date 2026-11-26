# Design

## Context

A solucao `Lab.Orders` e atualmente um scaffold vazio: apenas `Class1.cs` no projeto de dominio e nenhum teste escrito. Nao existe spec anterior e nao ha persistencia, API ou integracoes a preservar. Veja `proposal.md` para a motivacao e os seis specs em `specs/` para o comportamento esperado. As decisoes abaixo fecham as ambiguidades do prompt original antes da implementacao e acrescentam o **refinamento tecnico** aprovado (NFRs, contratos, modelo de dados, infraestrutura, resiliencia, seguranca, observabilidade, idempotencia, convencoes de codigo, ADRs).

Stack: **.NET 10, C# 14, Nullable Reference Types habilitado**; testes **xUnit + FluentAssertions + NSubstitute + AutoFixture**; build `dotnet build --no-restore`; teste `dotnet test --no-build --verbosity minimal`; **Regra de Ouro: nada concluido com build/teste quebrado** (verificado por `@dotnet-reviewer`, nao por este papel).

## Goals / Non-Goals

**Goals:**

- Tornar as regras de desconto e de taxa de entrega deterministicas e verificaveis por teste.
- Manter o dominio independente de framework, persistencia e transporte HTTP.
- Dar a cada regra de negocio um unico dono entre as capabilities, evitando duplicacao de validacao.
- **Precisao monetaria deterministica** (`decimal`, 2 casas, half-up, cultura invariant); nunca `double/float`.
- **Clean Architecture inegociavel**; contratos em Application, implementacoes em Infrastructure.
- **Criacao de pedido idempotente** (`IdempotencyKey`).
- **Codigo sempre em ingles; artefatos OpenSpec em portugues; sem comentarios por padrao.**

**Non-Goals:**

- Resolver calculo de distancia real, rotas ou geocodificacao.
- Validar cartao de credito/debito ou integrar provedor de pagamento.
- Definir estoque, impostos, multas por atraso, historico de pedidos ou acompanhamento de entrega.
- Autenticacao/autorizacao, notificacoes, mensageria/assincrono no MVP.
- Migrar dados: nao existe base anterior.
- **CNPJ alfanumerico**: somente documento **numerico** (CPF/CNPJ digitos + digitos verificadores).

## Convencoes de codigo (OBRIGATORIO)

- **Codigo SEMPRE em ingles — sem excecao.** Tipos, membros, parametros, locais, constantes, namespaces, arquivos, projetos, migrations, **tabelas/colunas de banco**, eventos, **mensagens de excecao**. Interfaces `I...`. **Dominio/Ubiquitous Language em ingles** (`Customer`, nao `Cliente`; `Product`, nao `Produto`; `Order`, nao `Pedido`).
- **Idioma dos artefatos != idioma do codigo.** Artefatos OpenSpec (`proposal.md`, `specs`, `design.md`, `tasks.md`, ADRs) e comentarios (raros) em **portugues**; **codigo em ingles**. Qualquer referencia a identificador usa nome em **ingles**.
- **Sem comentarios por padrao — otica Clean Code.** **Proibido XML doc (`///`).** Comentario so quando indispensavel (regra de negocio realmente complexa; decisao contraintuitiva = *por que*; alerta de perigo; `TODO`/`HACK` com contexto) ou registrar em **ADR**.

**Naming (English):**
- Domain: `Customer`, `Product`, `Order`, `OrderItem`, `Address`, `Document`, `Money`, `Percentage`, `Discount`, `DiscountType`, `Sku`, `Quantity`, `Distance`, `DeliveryFee`, `PaymentMethod`, `CustomerId`, `ProductId`, `OrderId`, `IdempotencyKey`.
- `DiscountType { Amount, Percentage }`. `PaymentMethod { Cash, Pix, CreditCard, DebitCard }`.
- Application: `RegisterCustomer`, `RegisterProduct`, `RegisterOrder`; `ICustomerRepository`, `IProductRepository`, `IOrderRepository`; `IIdempotencyStore`; `Result`/`Result<T>`; `Command`/`Response`. Contracts in Application; impls in Infrastructure; **no MediatR**.

## Requisitos nao-funcionais verificaveis (NFRs)

Todo requisito tecnico verificavel entra como requisito com cenario OpenSpec, **sem tecnologia na spec** (tecnologia em `design.md`/ADR). Subconjunto proposto e aprovado:

- **Precisao monetaria**: `Money` `decimal`, 2 casas, `MidpointRounding.AwayFromZero`, cultura `InvariantCulture`; **proibido `double/float`**. Arredondamento **uma unica vez** ao centavo no preco final do item e no desconto do pedido. *(emenda em `order-item-pricing`/`discount-policy`.)*
- **Distancia**: `Distance` `decimal`, km, nao negativa; arredondamento **half-up no 1o decimal para km inteiro antes** de taxa/limite; limite **100 km** (apos arredondar); 100 aceito (R$97,00); 101 recusado. Sem mapas.
- **Taxa**: R$12,00 ate 15 km; +R$1,00/km inteiro acima de 15; nunca descontada.
- **Desconto**: `Discount` 1 modalidade (`Amount`/`Percentage`); nunca ambos; nunca negativo; `null` = sem; exclusividade com desconto de item; `OrderDiscount <= Subtotal`; `==Subtotal` parcela 0; `>Subtotal` recusa; percentual sobre base unitaria (item) ou subtotal (pedido), arredondado 1x.
- **Total**: `max(0, Subtotal - OrderDiscount) + DeliveryFee`; piso zero antes da taxa; nunca negativo antes da taxa; taxa integral.
- **Determinismo**: dominio puro; mesma entrada -> mesma saida; sem I/O no Domain.
- **Cobertura**: **todo Scenario** dos seis specs tem teste; build/teste verde; **nenhum teste ignorado**; `dotnet test --no-build --verbosity minimal`.
- **Persistencia round-trip**: gravar + recuperar cada agregado sem perda; totals coerentes; `numeric(18,2)` MySQL.
- **Contrato API**: `ProblemDetails`; mensagens **em ingles**; **camelCase**; `CultureInfo.InvariantCulture`; `400/404/422/409`. Spec nao fixa status HTTP.
- **Idempotencia**: criacao de `Order` com `IdempotencyKey`; mesma chave + mesmo payload -> mesmo resultado; nao duplica; chave nova -> novo pedido. *(emenda em `order-registration`.)*
- **Seguranca**: sem segredos em codigo; **documento (CPF/CNPJ) nao logado**; sem PII em logs; HTTPS; validacao de entrada; parameterized queries; segredos via env/secret.
- **Observabilidade**: logs estruturados sem PII; correlation id; health check (`/health`); metricas/latencia.
- **Performance/disponibilidade** (proposto, nao verificavel neste lab): `Order` sincrono **p99 < 300 ms** (excl. provisioning); disponibilidade **99,5%**; **RPO/RTO 24h/4h**; volume **nao verificavel** sem dado de producao.
- **Resiliencia**: `AddProblemDetails` + global exception handler; sem Polly no MVP (sincrono, sem externo); opcional `Microsoft.Extensions.Resilience` so com ADR.

## Decisions

### 1. Fronteiras entre capabilities

Seis capabilities com responsabilidades exclusivas:

```
  +--------------------------------------------------------+
  |                    order-registration                   |
  |  agregacao, pagamento, validacoes de entrada, total     |
  +--+-----------+-----------+-----------------------------+
     |           |           |
     v           v           v
+---------+  +---------+  +---------+
| customer|  | order-  |  |delivery-|
| -regist |  | item-   |  | fee     |
| (cli.)  |  | pricing |  |         |
+---------+  +----+----+  +---------+
                  |
       +----------+----------+
       v                     v
+--------------+    +------------------+
| product-     |    | discount-policy  |
| catalog      |    | (transversal)    |
+--------------+    +------------------+
```

`discount-policy` e transversal: consultada por `order-item-pricing` (desconto unitario) e por `order-registration` (desconto do pedido). `delivery-fee` nao consulta `discount-policy`: a taxa nunca recebe desconto.

**Alternativa considerada:** um unico modulo `order-management`. Descartada porque misturaria regras de negocio com vidas diferentes (catalogo, documento, tarifa) e tornaria os testes de fronteira impossiveis de isolar.

### 2. Composicao do total

Adotada a opcao (c):

```
Subtotal = SUM(OrderItem.FinalPrice)
Parcela  = max(0, Subtotal - OrderDiscount)
Total    = Parcela + DeliveryFee
```

O desconto incide somente sobre produtos, nunca sobre a taxa. A recusa por total negativo dispara quando `OrderDiscount > Subtotal`, antes de somar a taxa. **Alternativas:** (a) sem piso; (b) `(Subtotal + Taxa) - Desconto` permitindo desconto sobre entrega — ambas descartadas.

### 3. Desconto como conceito unico

Um `Discount` e um valor com uma unica modalidade: `Amount` ou `Percentage`. Ambas preenchidas recusa, tanto no item quanto no pedido. Valores negativos recusados em ambos. `null` = sem desconto. **Exclusividade:** se qualquer `OrderItem` tem `Discount`, `Order` nao pode ter `Discount` proprio. **Dono unico:** `discount-policy`. **Alternativa considerada:** modalidade unica apenas no pedido — descartada.

### 4. Arredondamento e precisao

- Valores monetarios: 2 casas (centavos), `half-up`, `decimal`, `InvariantCulture`. **Sem `double/float`.**
- Distancia: `half-up` no 1o decimal para km inteiro, antes de qualquer regra. `15,2 -> 15`, `15,5 -> 16`.
- Teto 100 km avaliado apos arredondar: `100,4 -> 100` aceito; `100,5 -> 101` recusado.
- `Percentage` aplicada sobre preco base unitario (item) ou subtotal (pedido); resultado arredondado **uma unica vez** ao centavo.
- **Arredondar uma vez** no preco final do item e no total do pedido. **Alternativa:** distancia fracionaria — descartada.

### 5. Produto no catalogo x item do pedido

Catalogo `Product(Sku, Description, BasePrice)`. `OrderItem` referencia `Sku` e guarda `Quantity` + `Discount` unitario; preco final **calculado**, nao persistido como entrada (mas **snapshot** persistido). Preco do pedido nao altera catalogo; preco base atualizado afeta pedidos **futuros**. **SKU** nao-vazio, trim, unique index (proposto); produto inexistente recusado. **Alternativa:** produto como parte do pedido — descartada.

### 6. Documento e forma de pagamento

CPF (11) e CNPJ (14) validados formato + DV; **somente digitos numericos**; entrada normalizada p/ digitos antes de validar; mascara nao altera resultado. `Document`. Forma de pagamento conjunto fechado `PaymentMethod { Cash, Pix, CreditCard, DebitCard }`. Cartao **nao** exige dados/validacao no MVP. **CNPJ alfanumerico fora.** **Alternativa:** documento texto livre — descartada.

### 7. Organizacao por camadas

Dominio puro com agregados e value objects; Application com casos de uso + validacoes + **contratos**; Infrastructure com persistencia/EF Core/MySQL + DI; API minimal. **Contratos em Application, impls em Infrastructure.** Sem tecnologia no dominio. **Testes acompanham:** unitarios de dominio e aplicacao, integracao de persistencia (in-memory), funcionais HTTP. **Alternativa:** regras na API — descartada.

### 8. `Address` estruturado

`record Address(string Street, string Number, string? Complement, string Neighborhood, string City, string State, string PostalCode)`. **Required** nao-vazio: `Street`, `Number`, `Neighborhood`, `City`, `State`, `PostalCode`. **Opcional:** `Complement`. `State`=UF (2 letras); `PostalCode`=CEP (8 digitos, hifen opcional, normalizado). `Address` presente obrigatorio (scenario preservado). `Distance` km informada a parte (nao derivada do endereco). **Owned type** no EF Core. *(emenda minima em `client-registration`.)*

### 9. `Guid`

`CustomerId`, `ProductId`, `OrderId` = `Guid`; `Order` referencia `CustomerId`; `OrderItem` referencia `Sku`. `Sku` unique. `Guid` como PK surrogate. **Alternativa:** chave natural — descartada.

### 10. Idempotencia

`RegisterOrder` aceita `IdempotencyKey` (header `Idempotency-Key`; no command). Persistencia `IdempotencyRecord(Key, PayloadHash, OrderId, CreatedAt)`; **unique index em `Key`**; checagem atomica (transacoo/lock); retry seguro. `IIdempotencyStore`. Nao e regra de dominio pura. **Mesma chave + mesmo payload -> mesmo resultado; nao duplica.** **TTL** proposto (nao fixado).

## Contratos Application (English)

- `ICustomerRepository`, `IProductRepository`, `IOrderRepository`, `IIdempotencyStore`. 
- `RegisterCustomer`, `RegisterProduct`, `RegisterOrder`. 
- `Result`/`Result<T>` + `Failure`; dominio valida invariantes lancando `DomainException`; Application mapeia; API -> `ProblemDetails`. 
- Sem MediatR; sem tecnologia de infra na Application.

## Modelo de dados (Infrastructure, EF Core MySQL)

- `OrdersDbContext`. **Value objects** como **owned types** ou conversoes; `Money` -> `numeric(18,2)`; `Percentage` -> `numeric`; `Document` -> string digitos; `PaymentMethod` -> int/string; `Sku` -> string; `Quantity` -> int; `Discount` -> colunas nullable (`DiscountType` + `numeric(18,2)`); `Distance` -> `numeric`; **tabelas/colunas English**. 
- `Customer`, `Product`, `Order`, `OrderItem`, `IdempotencyRecord`. PK `Guid`. `Order` snapshot totals (`Subtotal`, `OrderDiscount`, `DeliveryFee`, `Total`, `DistanceKm`). `OrderItem` (`Sku`, `Quantity`, `Discount`, `UnitPrice`, `FinalPrice`). FK `Order.CustomerId`. Unique `Product.Sku`. `Address` owned. **`numeric(18,2)`**; **migrations English**. 

## Infraestrutura & empacotamento

- `Microsoft.NET.Sdk.Web`; `Program.cs`; `appsettings.json`/`.Development`; `launchSettings`; `AddProblemDetails` + global handler; `AddOpenApi`; `AddHealthChecks`; `CultureInfo.InvariantCulture`; `AddDbContext`; `AddApplication`/`AddInfrastructure`. 
- `Microsoft.NET.Sdk` (Domain/Application/Infrastructure); `Microsoft.AspNetCore.Mvc.Testing` (testes). 
- **`Directory.Packages.props`** (CPM); **`Directory.Build.props`**; pacotes English; versoes fixas; `dotnet package search`. 
- Pacotes: EF Core 10 + provider **MySQL** + `.Design`/`.Tools`; `Microsoft.EntityFrameworkCore.InMemory` (testes); **AutoFixture**. 
- **Provider MySQL recomendado:** Oracle **`MySql.EntityFrameworkCore`** (estavel EF Core 10, `UseMySQL`, MySQL 8+; **licenca `GPL-2.0-only WITH Universal-FOSS-exception-1.0` + `requireLicenseAcceptance` -> revisao legal**). **Alternativa MIT `Microting.EntityFrameworkCore.MySql`** (renomeado de `Pomelo.EntityFrameworkCore.MySql`, `UseMySql`, `MySqlConnector`; `Pomelo...MySql` parado em 9.0). **Versao exata a fixar na aplicacao.** Reversibilidade: troca provider + reimplementa Infra. *(ADR-010.)* 
- **`Dockerfile`** (multi-stage; non-root; sem segredos); **`docker-compose.yml`** (app + `mysql:8`; volumes; healthcheck; connection string env/secret). **Sem CI por enquanto.** 
- `dotnet ef migrations add <EnglishName>`; `dotnet ef database update`. 
- **Nada implementado aqui;** `@ca-architect` orquestra; `@dotnet-*` implementa; `@dotnet-reviewer` valida. 

## Resiliencia

Sincrono; sem integracoes externas; sem mensageria/outbox no MVP. Validacao de entrada; `ProblemDetails`; exception handler global; **idempotencia**; (opcional `Microsoft.Extensions.Resilience` so com ADR). Sem K8s/Kafka/NoSQL sem requisito.

## Seguranca & LGPD

Sem segredos; **documento nao logado**; sem PII em logs; HTTPS; parameterized queries; validacao; connection string segura; **minimizacao**; retention a definir; sem auth no MVP. 

## Observabilidade

Logs estruturados sem PII; correlation id; `/health`; metricas/latencia; alertas. 

## Risks / Trade-offs

- [Arredondamento em cascata] -> arredondar uma unica vez ao centavo; testes de fronteira.
- [Mesma regra de desconto duplicada] -> `discount-policy` unico dono.
- [Distancia manual nao reflete entrega real] -> aceito MVP; sem mapas declarado.
- [Mascara CPF/CNPJ] -> normalizar digitos.
- [CNPJ alfanumerico real-world 2026] -> **fora**; somente numerico; revisitar com aprovacao.
- [Piso zero vs recusa no item] -> `OrderItem` rejeita desconto > base; piso zero so `==`; regra no `discount-policy`.
- [Cenarios sobrepostos `discount-policy` x `order-registration`] -> regra em `discount-policy`; integracao em `order-registration`.
- [EF Core InMemory nao valida constraints reais] -> in-memory so teste; MySQL em prod/Docker.
- [Concorrencia na criacao de pedido] -> unique `IdempotencyKey` + transacao.
- [Provider MySQL/licenca/versao] -> verificar; reversivel.
- [Totals persistidos vs recalcular] -> snapshot + recalculo no dominio; round-trip.

## Migration Plan / Reversibilidade

Nao ha dados/codigo anterior. Implementacao a partir do scaffold. Rollback = nao aplicar a change; **sem migracao de base nem contrato externo**. Reversibilidade por `IRepository`, provider trocavel, ADRs registradas. 

## ADRs

> Artefatos em portugues; identificadores em ingles. 2-4 linhas essenciais; detalhe no `design.md`. `Class1.cs` removido.

- **ADR-001 — Clean Architecture + regra de dependencia.** `Domain←Application←Infrastructure←Api`; dominio puro; contratos em Application. **Reverter:** nao recomendado.
- **ADR-002 — `Money` `decimal` + `Percentage`.** `decimal` 2dp half-up invariant; sem `double/float`. **Reverter:** n/a (correcao de bug se houver).
- **ADR-003 — Arredondamento e precisao.** money 1x ao centavo; km inteiro half-up 1o decimal ≤100; percentual 1x. **Reverter:** ajustar regra com teste.
- **ADR-004 — `Discount`.** 1 modalidade `Amount`/`Percentage`; nao negativo; `null`; exclusividade; `OrderDiscount<=Subtotal`; nao incide em taxa; dono `discount-policy`. **Reverter:** refatorar dono.
- **ADR-005 — `DeliveryFee`.** R$12 ate 15km; +R$1/km inteiro; >100 recusa; km half-up; sem mapas. **Reverter:** injetar `IDeliveryFeeCalculator`.
- **ADR-006 — Total.** `max(0,Subtotal-OrderDiscount)+DeliveryFee`. **Reverter:** n/a.
- **ADR-007 — `Document`.** CPF/CNPJ **numerico** + DV; normaliza digitos. **Reverter:** suportar alfanumerico com aprovacao.
- **ADR-008 — `PaymentMethod`.** `{Cash,Pix,CreditCard,DebitCard}`; sem validacao de cartao. **Reverter:** estender enum.
- **ADR-009 — `Product`/`OrderItem`.** `Sku`; preco **snapshot**; final calculado. **Reverter:** n/a.
- **ADR-010 — Persistencia EF Core + MySQL.** `OrdersDbContext`; owned/converters; `numeric(18,2)`; `Guid`; `Sku` unique; FK; migrations; `IRepository`; **in-memory teste**; provider Oracle `MySql.EntityFrameworkCore` rec. / `Microting...` alt; licenca + versao verificar; MySQL 8+. **Reverter:** trocar provider + reimplementar Infra.
- **ADR-011 — `Address`.** `record` estruturado; required `Street/Number/Neighborhood/City/State/PostalCode`; opt `Complement`; owned. **Reverter:** string.
- **ADR-012 — `Guid`.** ids surrogate; `Sku` unique; pedido ref. `CustomerId`. **Reverter:** chave natural.
- **ADR-013 — Idempotencia.** `IdempotencyKey` + `IdempotencyRecord` + unique; atomica; `IIdempotencyStore`. **Reverter:** remover.
- **ADR-014 — API minimal + `ProblemDetails`.** camelCase; mensagens ingles; invariant; `400/404/422/409`; `Idempotency-Key`. **Reverter:** controllers.
- **ADR-015 — Sem mensageria/auth + sincrono.** MVP. **Reverter:** outbox+broker / auth.
- **ADR-016 — Convencoes codigo.** ingles; artefato pt; sem comentarios/XML doc; regra pt rara. **Reverter:** n/a.
- **ADR-017 — Docker + compose; sem CI.** `Dockerfile`+`mysql:8`. **Reverter:** CI/K8s.
- **ADR-018 — CPM + `Directory.Build.props`.** `Directory.Packages.props`. **Reverter:** inline.
