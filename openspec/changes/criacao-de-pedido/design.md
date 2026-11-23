# Design

## Context

O repositório Lab.Orders possui apenas `src/Lab.Orders.Domain` e `tests/Lab.Orders.Domain.Tests` (esqueleto). Não há projetos Application/Infrastructure/API nem entidades de Pedido ainda. A mudança implementa o fluxo de criação de Pedido descrito no proposal e nas specs: validação de limite de crédito do cliente e cálculo de desconto por item.

## Goals / Non-Goals

**Goals:**
- Agregados de Domínio: `Customer` (com `CreditLimit`) e `Order` (CustomerId, Items, Total, Status, CreatedAt).
- `Order`: validação de criação (customer obrigatório, items não vazios, quantidade > 0, unitPrice > 0, DiscountPercent entre 0 e 100), cálculo do total com desconto por item, verificação de limite de crédito.
- Caso de uso `CreateOrderUseCase` + interfaces de repositório (`IOrdersRepository`, `ICustomersRepository`).
- Implementação concreta dos repositórios (Infrastructure) e endpoint de criação de pedidos (API).
- Suíte unitária cobrindo validação, cálculo de desconto e isolamento de camadas (dependência Domain → não depender de Outras Camadas).

**Non-Goals:**
- Listagem, edição ou cancelamento de pedidos nesta mudança.
- Alteração do esquema de banco existente — não há banco/persistência configurada no repositório.
- Regras de cobrança, política de taxas ou dívida do cliente além do limite de crédito.

## Decisions

1. **Validações de criação no agregado `Order`** (Domínio): construtor e `AddItem` lançam exceções (`ArgumentException`/`DomainException`) para customer ausente, items vazios, quantidade 0 ou negative, unitPrice ≤ 0 e DiscountPercent fora de 0–100. Alternativa considerada: validar no caso de uso; descartada porque regras de invariantes de agregado devem viver no agregado.
2. **Cálculo do desconto por item no agregado**: `OrderItem.DiscountPercent` (0–100, default 0); `OrderItem.LineTotal` = `UnitPrice * Quantity * (1 - DiscountPercent / 100)`; `Order.Total` = soma das `LineTotal` das items. Alternativa: desconto monetário fixo (`Discount`) — descartada por ser menos expressivo para percentuais e exigir conversão de moeda.
3. **Limite de crédito validado pelo use case via `Customer`**: `Customer` agregado detém `CreditLimit`; `CreateOrderUseCase.ExecuteAsync` verifica `order.Total > customer.CreditLimit` e lança `OrderCreditLimitExceededException`. Alternativa: checar dentro do `Order.Validate()` — descartada porque `Order` não carrega o `Customer` atual, apenas o ID.
4. **Application**: `CreateOrderUseCase` orquestra: carrega `Customer` pelo repositório, cria o `Order`, chama `order.Validate()`, verifica o limite de crédito, persiste pelo repositório de orders. Status inicial do pedido: `Created`.
5. **Infrastructure**: projeto com implementação concreta dos repositórios (ex.: persistência baseada em provedor configurável). O projeto será estruturado conforme a convenção Clean Architecture do projeto.
6. **API**: `OrdersController` com `POST /orders`: mapeia `CreateOrderRequest` (customerId, items[]) para `CreateOrderCommand`, retorna 201 com `OrderDto`; mapeia `OrderCreditLimitExceededException` para 409 Conflict; exceções de validação de domínio para 400.

## Risks / Trade-offs

- [Risco] `DiscountPercent` interpretado de forma diferente (ex.: resgate) → Mitigação: regra única em `OrderItem.LineTotal`, coberta por cenários das specs.
- [Risco] Validações duplicadas entre Domain e Application → Mitigação: somente o agregado valida; o caso de uso confia no agregado.
- [Risco] Repositórios não têm provedor de persistência definido no momento (Infrastructure/API não existem) → Mitigação: projetos serão introduzidos no início da implementação; a lógica de domínio não depende deles.

## Migration Plan

Não se aplica: sem banco/persistência configurada no repositório ainda; as entidades novas (`Customer`, `Order`) iniciam como agregados vazios no pacote de testes e no repositório concreto.
