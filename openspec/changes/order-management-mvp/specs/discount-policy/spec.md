# Spec Delta

## Purpose

Define as regras de elegibilidade de descontos em produtos e pedidos, evitando combinações inválidas e garantindo que a política aplicada seja sempre determinística e não negativa.

## ADDED Requirements

### Requirement: Um desconto usa somente uma modalidade

O sistema MUST aceitar um desconto como percentual ou como valor monetário, mas MUST recusar a operação quando as duas modalidades forem informadas ao mesmo tempo.

#### Scenario: desconto informado somente em percentual

- **WHEN** um desconto de pedido é informado com 10% e nenhum valor monetário
- **THEN** o desconto é aceito na modalidade percentual

#### Scenario: desconto informado somente em valor monetário

- **WHEN** um desconto de pedido é informado com R$ 20,00 e nenhum percentual
- **THEN** o desconto é aceito na modalidade monetária

#### Scenario: desconto informado nas duas modalidades

- **WHEN** um desconto de pedido é informado com 10% e com R$ 20,00
- **THEN** o pedido é recusado e a resposta indica que as modalidades são exclusivas

### Requirement: Desconto não pode ser negativo

O sistema MUST recusar qualquer desconto unitário ou de pedido cujo valor informado seja negativo, independentemente da modalidade.

#### Scenario: desconto de pedido negativo

- **WHEN** um pedido é registrado com desconto monetário de R$ -10,00
- **THEN** o pedido é recusado e a resposta indica que o desconto não pode ser negativo

#### Scenario: desconto unitário negativo em percentual

- **WHEN** um produto do pedido possui desconto unitário de -5%
- **THEN** o pedido é recusado e a resposta indica que o desconto não pode ser negativo

### Requirement: Desconto de pedido e desconto de produto são mutuamente exclusivos

O sistema MUST recusar um pedido que possua desconto de pedido e, ao mesmo tempo, um ou mais produtos com desconto unitário.

#### Scenario: pedido com desconto e produto com desconto

- **WHEN** o carrinho contém um produto com desconto unitário e o cliente tenta registrar o pedido com desconto
- **THEN** o pedido é recusado e a resposta indica que os descontos são exclusivos

#### Scenario: descontos individuais substituem o desconto do pedido

- **WHEN** um ou mais produtos do carrinho possuem desconto unitário e o pedido é registrado sem desconto de pedido
- **THEN** o pedido é aceito e o total é calculado usando apenas os descontos individuais

#### Scenario: desconto do pedido aplicado quando nenhum produto tem desconto

- **WHEN** nenhum produto do carrinho possui desconto unitário e o pedido é registrado com desconto
- **THEN** o pedido é aceito e o total é calculado usando o desconto do pedido
