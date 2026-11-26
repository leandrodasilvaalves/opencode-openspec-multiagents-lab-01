# Spec Delta

## Purpose

Define como cada item de um pedido e composto a partir do catalogo, combinando SKU, quantidade, desconto unitario opcional e preco final calculado.

## ADDED Requirements

### Requirement: Item de pedido referencia um produto existente no catalogo
O sistema DEVE (MUST) aceitar somente itens cujo SKU corresponda a um produto cadastrado no catalogo.

#### Scenario: SKU existente e aceito
- **GIVEN** que existe no catalogo um produto com o SKU informado
- **WHEN** o item e adicionado ao pedido
- **THEN** o item e aceito e o preco base e obtido do produto do catalogo

#### Scenario: SKU inexistente e recusado
- **GIVEN** que nao existe no catalogo um produto com o SKU informado
- **WHEN** o item e adicionado ao pedido
- **THEN** o item e recusado e uma mensagem indica que o produto nao foi encontrado

### Requirement: Item de pedido deve ter quantidade maior que zero
O sistema DEVE (MUST) recusar itens com quantidade igual ou menor que zero.

#### Scenario: quantidade um e aceita
- **GIVEN** que a quantidade informada para o item e um
- **WHEN** o item e adicionado ao pedido
- **THEN** o item e aceito

#### Scenario: quantidade maior que um e aceita
- **GIVEN** que a quantidade informada para o item e tres
- **WHEN** o item e adicionado ao pedido
- **THEN** o item e aceito e o preco final considera as tres unidades

#### Scenario: quantidade zero e recusada
- **GIVEN** que a quantidade informada para o item e zero
- **WHEN** o item e adicionado ao pedido
- **THEN** o item e recusado e uma mensagem indica que a quantidade deve ser maior que zero

#### Scenario: quantidade negativa e recusada
- **GIVEN** que a quantidade informada para o item e menor que zero
- **WHEN** o item e adicionado ao pedido
- **THEN** o item e recusado e uma mensagem indica que a quantidade deve ser maior que zero

### Requirement: Desconto unitario e opcional e respeita a politica de descontos
O sistema DEVE (MUST) permitir que o item tenha um unico desconto unitario, em valor ou em percentual, ou nenhum desconto, conforme a politica definida em `discount-policy`.

#### Scenario: item sem desconto e aceito
- **GIVEN** que nenhum desconto unitario foi informado
- **WHEN** o item e adicionado ao pedido
- **THEN** o item e aceito e o preco final e igual ao preco base multiplicado pela quantidade

#### Scenario: desconto unitario em valor e aceito
- **GIVEN** que o desconto unitario informado e um valor monetario
- **WHEN** o item e adicionado ao pedido
- **THEN** o item e aceito e o desconto e aplicado por unidade

#### Scenario: desconto unitario em percentual e aceito
- **GIVEN** que o desconto unitario informado e um percentual
- **WHEN** o item e adicionado ao pedido
- **THEN** o item e aceito e o desconto e calculado sobre o preco base de cada unidade

#### Scenario: desconto unitario em valor e percentual ao mesmo tempo e recusado
- **GIVEN** que foram informados valor e percentual de desconto para o mesmo item
- **WHEN** o pedido e registrado
- **THEN** o pedido e recusado e uma mensagem indica que apenas uma modalidade de desconto e permitida

### Requirement: Desconto unitario nao pode ser negativo
O sistema DEVE (MUST) recusar desconto unitario negativo em qualquer modalidade.

#### Scenario: desconto unitario em valor negativo e recusado
- **GIVEN** que o desconto unitario informado em valor e menor que zero
- **WHEN** o pedido e registrado
- **THEN** o pedido e recusado e uma mensagem indica que descontos negativos nao sao permitidos

#### Scenario: desconto unitario em percentual negativo e recusado
- **GIVEN** que o desconto unitario informado em percentual e menor que zero
- **WHEN** o pedido e registrado
- **THEN** o pedido e recusado e uma mensagem indica que descontos negativos nao sao permitidos

### Requirement: Preco final do item e calculado por unidade antes da quantidade
O sistema DEVE (MUST) calcular o preco final do item como (preco base menos desconto unitario) multiplicado pela quantidade, com valor nunca inferior a zero.

#### Scenario: preco final sem desconto
- **GIVEN** que o preco base e R$ 10,00, a quantidade e dois e nao ha desconto
- **WHEN** o preco final e calculado
- **THEN** o preco final e R$ 20,00

#### Scenario: preco final com desconto em valor
- **GIVEN** que o preco base e R$ 10,00, a quantidade e tres e o desconto unitario e R$ 2,00
- **WHEN** o preco final e calculado
- **THEN** o preco final e R$ 24,00

#### Scenario: preco final com desconto em percentual
- **GIVEN** que o preco base e R$ 10,00, a quantidade e dois e o desconto unitario e dez por cento
- **WHEN** o preco final e calculado
- **THEN** o preco final e R$ 18,00

#### Scenario: desconto unitario igual ao preco base gera preco final zero
- **GIVEN** que o preco base e R$ 10,00, a quantidade e um e o desconto unitario e R$ 10,00
- **WHEN** o preco final e calculado
- **THEN** o preco final e R$ 0,00 e o item e aceito

#### Scenario: desconto unitario maior que o preco base e recusado
- **GIVEN** que o preco base e R$ 10,00 e o desconto unitario e R$ 12,00
- **WHEN** o pedido e registrado
- **THEN** o pedido e recusado e uma mensagem indica que o desconto nao pode exceder o preco do produto

#### Scenario: desconto percentual acima de cem por cento e recusado
- **GIVEN** que o desconto unitario informado e cento e dez por cento
- **WHEN** o pedido e registrado
- **THEN** o pedido e recusado e uma mensagem indica que o desconto nao pode exceder o preco do produto
