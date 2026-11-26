# Spec Delta

## Purpose

Define o catalogo de produtos que serve de origem para os itens de pedido, identificando cada produto por SKU, descricao e preco base.

## ADDED Requirements

### Requirement: Produto do catalogo exige SKU, descricao e preco base
O sistema DEVE (MUST) recusar o cadastro de um produto que nao informe SKU, descricao ou preco base.

#### Scenario: produto completo e aceito
- **GIVEN** que o SKU, a descricao e o preco base foram informados
- **WHEN** o produto e cadastrado no catalogo
- **THEN** o cadastro e aceito e o produto fica disponivel para compor pedidos

#### Scenario: produto sem SKU e recusado
- **GIVEN** que a descricao e o preco base foram informados, mas o SKU nao
- **WHEN** o produto e cadastrado no catalogo
- **THEN** o cadastro e recusado e uma mensagem indica que o SKU e obrigatorio

#### Scenario: produto sem descricao e recusado
- **GIVEN** que o SKU e o preco base foram informados, mas a descricao nao
- **WHEN** o produto e cadastrado no catalogo
- **THEN** o cadastro e recusado e uma mensagem indica que a descricao e obrigatoria

#### Scenario: produto sem preco base e recusado
- **GIVEN** que o SKU e a descricao foram informados, mas o preco base nao
- **WHEN** o produto e cadastrado no catalogo
- **THEN** o cadastro e recusado e uma mensagem indica que o preco base e obrigatorio

### Requirement: Preco base do produto nao pode ser negativo
O sistema DEVE (MUST) recusar um preco base negativo, pois um valor negativo tornaria negativo o subtotal do pedido.

#### Scenario: preco base zero e aceito
- **GIVEN** que o preco base informado e zero
- **WHEN** o produto e cadastrado no catalogo
- **THEN** o cadastro e aceito

#### Scenario: preco base negativo e recusado
- **GIVEN** que o preco base informado e menor que zero
- **WHEN** o produto e cadastrado no catalogo
- **THEN** o cadastro e recusado e uma mensagem indica que o preco base nao pode ser negativo
