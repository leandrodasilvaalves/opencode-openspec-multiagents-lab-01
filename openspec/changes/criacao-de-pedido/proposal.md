# Proposal

## Why

O repositório possui o projeto `Lab.Orders.Domain` e seus testes, mas não existe fluxo para criação de Pedido (Order). É necessário criar Pedidos com validação de limite de crédito do cliente e cálculo de desconto por item, cobrindo as camadas do sistema (Domain, Application, Infrastructure e API) e testes unitários.

## What Changes

- **Domain**: novo agregado `Order` com `OrderItem`; a criação do pedido valida a consistência dos dados (cliente válido, itens não vazios, cálculo do total).
- **Validação de limite de crédito**: ao criar um `Order`, o total do pedido (soma das linhas com desconto já aplicado) não pode exceder o limite de crédito do cliente; em caso de violação, a criação é rejeitada com a razão do negócio.
- **Cálculo de desconto por item**: cada `OrderItem` possui uma regra de desconto aplicada à sua linha do pedido (`unitPrice * quantity`), calculando o desconto por item; o total do pedido é a soma das linhas já com desconto aplicado.
- **Fluxo de criação de Pedido**: caso de uso que valida e persiste o pedido (Application), repositório e persistência (Infrastructure), e endpoints de criação (API).

## Capabilities

### New Capabilities

- `order-creation`: criação de pedidos com validação de limite de crédito do cliente e cálculo de desconto por item, cobrindo as camadas Domain, Application, Infrastructure e API.

### Modified Capabilities

(nenhuma)

## Impact

- **Domain**: novo agregado `Order`/`OrderItem` e regras de negócio (cálculo do total, validação de limite de crédito).
- **Application**: novos casos de uso e interface de repositório (`IOrdersRepository`) para criação de pedidos.
- **Infrastructure**: implementações concretas dos repositórios (persistência dos pedidos).
- **API**: endpoints de criação de pedidos (ex.: `POST /orders`).
- **Testes**: suíte unitária cobrindo validação de limite de crédito, cálculo de desconto por item e isolamento de camadas (dependência Domain → não depender de Outras Camadas, já presente no projeto).
- Repositório atual: a solução `Lab.Orders.slnx` possui apenas `src/Lab.Orders.Domain` e `tests/Lab.Orders.Domain.Tests`; os projetos Application, Infrastructure e API serão criados/estendidos conforme avanço da implementação.
