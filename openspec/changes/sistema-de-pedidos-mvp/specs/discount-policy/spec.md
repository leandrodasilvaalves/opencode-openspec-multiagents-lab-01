# Spec Delta

## Purpose

Define as regras comuns de desconto aplicaveis a itens e ao pedido: modalidade unica, nao negatividade, exclusividade entre desconto de item e desconto de pedido e limite do valor total.

## ADDED Requirements

### Requirement: Cada desconto deve usar uma unica modalidade
O sistema DEVE (MUST) aceitar valor ou percentual para cada desconto, mas nunca ambos no mesmo desconto, tanto no item quanto no pedido.

#### Scenario: desconto do pedido em valor e aceito
- **GIVEN** que apenas o valor do desconto do pedido foi informado
- **WHEN** o pedido e registrado
- **THEN** o desconto e aplicado como valor monetario

#### Scenario: desconto do pedido em percentual e aceito
- **GIVEN** que apenas o percentual de desconto do pedido foi informado
- **WHEN** o pedido e registrado
- **THEN** o desconto e aplicado como percentual

#### Scenario: valor e percentual no desconto do pedido e recusado
- **GIVEN** que valor e percentual de desconto do pedido foram informados ao mesmo tempo
- **WHEN** o pedido e registrado
- **THEN** o pedido e recusado e uma mensagem indica que apenas uma modalidade de desconto e permitida

#### Scenario: valor e percentual no desconto de item e recusado
- **GIVEN** que valor e percentual de desconto foram informados ao mesmo tempo para um item
- **WHEN** o pedido e registrado
- **THEN** o pedido e recusado e uma mensagem indica que apenas uma modalidade de desconto e permitida

### Requirement: Descontos nao podem ser negativos
O sistema DEVE (MUST) recusar qualquer desconto negativo, em valor ou percentual, aplicado a um item ou ao pedido.

#### Scenario: desconto de item em valor negativo e recusado
- **GIVEN** que o valor de desconto de um item e menor que zero
- **WHEN** o pedido e registrado
- **THEN** o pedido e recusado e uma mensagem indica que descontos negativos nao sao permitidos

#### Scenario: desconto do pedido em percentual negativo e recusado
- **GIVEN** que o percentual de desconto do pedido e menor que zero
- **WHEN** o pedido e registrado
- **THEN** o pedido e recusado e uma mensagem indica que descontos negativos nao sao permitidos

### Requirement: Desconto do pedido so existe quando nenhum item tem desconto
O sistema DEVE (MUST) recusar um pedido que combine desconto no pedido com desconto em qualquer um de seus itens, aplicando somente os descontos individuais nesse caso.

#### Scenario: nenhum item com desconto permite desconto do pedido
- **GIVEN** que nenhum item do pedido possui desconto unitario
- **WHEN** um desconto do pedido e informado
- **THEN** o desconto do pedido e aceito

#### Scenario: um item com desconto impede desconto do pedido
- **GIVEN** que ao menos um item do pedido possui desconto unitario
- **WHEN** um desconto do pedido tambem e informado
- **THEN** o pedido e recusado e uma mensagem indica que devem ser aplicados apenas os descontos individuais

#### Scenario: itens com desconto sao mantidos sem desconto do pedido
- **GIVEN** que um item do pedido possui desconto unitario e nenhum desconto do pedido foi informado
- **WHEN** o pedido e registrado
- **THEN** o pedido e aceito e somente o desconto unitario do item e aplicado

### Requirement: Desconto do pedido nao pode exceder o subtotal dos itens
O sistema DEVE (MUST) recusar o pedido quando o desconto do pedido for maior que o subtotal, garantindo que a parcela antes da taxa nunca seja negativa.

#### Scenario: desconto menor que o subtotal e aceito
- **GIVEN** que o subtotal dos itens e R$ 100,00 e o desconto do pedido e R$ 20,00
- **WHEN** o pedido e registrado
- **THEN** a parcela antes da taxa e R$ 80,00

#### Scenario: desconto igual ao subtotal resulta em parcela zero
- **GIVEN** que o subtotal dos itens e R$ 100,00 e o desconto do pedido e R$ 100,00
- **WHEN** o pedido e registrado
- **THEN** a parcela antes da taxa e R$ 0,00 e o pedido e aceito

#### Scenario: desconto maior que o subtotal recusa o pedido
- **GIVEN** que o subtotal dos itens e R$ 100,00 e o desconto do pedido e R$ 120,00
- **WHEN** o pedido e registrado
- **THEN** o pedido e recusado e uma mensagem indica que o desconto nao pode tornar o total negativo

#### Scenario: desconto percentual maior que o subtotal recusa o pedido
- **GIVEN** que o subtotal dos itens e R$ 100,00 e o desconto do pedido e cento e vinte por cento
- **WHEN** o pedido e registrado
- **THEN** o pedido e recusado e uma mensagem indica que o desconto nao pode tornar o total negativo

### Requirement: Desconto do pedido nunca incide sobre a taxa de entrega
O sistema DEVE (MUST) aplicar o desconto do pedido somente ao subtotal dos itens, preservando integralmente a taxa de entrega no total final.

#### Scenario: desconto nao reduz a taxa de entrega
- **GIVEN** que o subtotal dos itens e R$ 100,00, o desconto do pedido e R$ 20,00 e a taxa de entrega e R$ 12,00
- **WHEN** o total do pedido e calculado
- **THEN** o desconto e descontado apenas do subtotal e a taxa de R$ 12,00 permanece integralmente no total

### Requirement: Desconto e calculado com precisao monetaria deterministica
O sistema DEVE (MUST) calcular o valor do desconto de forma deterministica, com valores monetarios em duas casas decimais, arredondamento half-up aplicado uma unica vez, independente de cultura do ambiente.

#### Scenario: desconto percentual arredonda ao centavo uma unica vez
- **GIVEN** um subtotal ou preco base e um percentual de desconto
- **WHEN** o valor do desconto e calculado
- **THEN** o resultado em moeda tem duas casas decimais, arredondado half-up uma unica vez, independente de cultura
