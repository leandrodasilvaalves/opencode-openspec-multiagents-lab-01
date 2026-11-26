# Spec Delta

## Purpose

Centraliza as regras de cálculo de preço final do produto, subtotal e total do pedido para que catálogo e registro de pedido produzam sempre os mesmos valores, com arredondamento e limites consistentes.

## ADDED Requirements

### Requirement: Preço final do produto é o preço menos o desconto unitário

O sistema MUST calcular o preço final subtraindo o desconto unitário do preço: no desconto percentual, `preco * (1 - percentual/100)`; no desconto monetário, `preco - valor`.

#### Scenario: desconto percentual no produto

- **WHEN** um produto com preço de R$ 100,00 possui desconto unitário de 15%
- **THEN** o preço final calculado é R$ 85,00

#### Scenario: desconto monetário no produto

- **WHEN** um produto com preço de R$ 100,00 possui desconto unitário de R$ 7,50
- **THEN** o preço final calculado é R$ 92,50

#### Scenario: produto sem desconto

- **WHEN** um produto com preço de R$ 100,00 não possui desconto unitário
- **THEN** o preço final calculado é R$ 100,00

### Requirement: Subtotal é a soma dos preços finais multiplicados pela quantidade

O sistema MUST calcular o subtotal do pedido somando, para cada item, o preço final do produto multiplicado pela quantidade informada.

#### Scenario: subtotal com itens de quantidades diferentes

- **WHEN** o pedido contém 2 unidades de um produto com preço final de R$ 90,00 e 1 unidade de um produto com preço final de R$ 45,00
- **THEN** o subtotal calculado é R$ 225,00

### Requirement: Total do pedido é subtotal menos desconto do pedido mais taxa de entrega

O sistema MUST calcular o total como `subtotal - descontoPedido + taxaEntrega`. Quando o desconto do pedido é percentual, a base MUST ser o subtotal; a taxa de entrega MUST NOT ser incluída na base do desconto nem descontada.

#### Scenario: desconto monetário do pedido

- **WHEN** o subtotal é R$ 225,00, o desconto do pedido é R$ 25,00 e a taxa de entrega é R$ 12,00
- **THEN** o total calculado é R$ 212,00

#### Scenario: desconto percentual do pedido incide apenas sobre o subtotal

- **WHEN** o subtotal é R$ 225,00, o desconto do pedido é 10% e a taxa de entrega é R$ 12,00
- **THEN** o desconto é R$ 22,50 e o total calculado é R$ 214,50

### Requirement: Total calculado nunca pode ser negativo

O sistema MUST recusar a operação quando o desconto exceder sua base de cálculo, em vez de produzir um preço final ou total negativo.

#### Scenario: desconto monetário do pedido maior que o subtotal

- **WHEN** o subtotal é R$ 100,00 e é informado um desconto monetário de R$ 150,00 no pedido
- **THEN** o pedido é recusado e a resposta indica que o total não pode ser negativo

#### Scenario: desconto percentual integral mantém a taxa de entrega

- **WHEN** o subtotal é R$ 100,00, o desconto do pedido é 100% e a taxa de entrega é R$ 12,00
- **THEN** o total calculado é R$ 12,00
