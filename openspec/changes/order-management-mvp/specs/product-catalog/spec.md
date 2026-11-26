# Spec Delta

## Purpose

Mantém o catálogo de produtos disponíveis para venda, identificados por SKU, com preço, desconto unitário opcional e preço final calculado, de modo que os pedidos referenciem produtos já existentes.

## ADDED Requirements

### Requirement: Produto deve ter SKU, descrição e preço obrigatórios

O sistema MUST exigir SKU, descrição e preço no cadastro do produto e MUST garantir que o SKU seja único em todo o catálogo.

#### Scenario: produto com SKU inédito é cadastrado

- **WHEN** um produto é cadastrado com SKU, descrição e preço e o SKU ainda não existe no catálogo
- **THEN** o produto é registrado no catálogo

#### Scenario: SKU duplicado é recusado

- **WHEN** um produto é cadastrado com um SKU já existente no catálogo
- **THEN** o cadastro é recusado e a resposta indica que o SKU já está em uso

#### Scenario: produto sem preço é recusado

- **WHEN** um produto é cadastrado sem informar o preço
- **THEN** o cadastro é recusado e a resposta indica o campo obrigatório ausente

### Requirement: Desconto unitário é opcional e usa uma única modalidade

O sistema MUST permitir que o produto tenha desconto unitário percentual ou monetário, mas MUST NOT permitir ambas as modalidades ao mesmo tempo, e MUST aceitar produto sem desconto.

#### Scenario: produto sem desconto

- **WHEN** um produto é cadastrado sem desconto unitário
- **THEN** o produto é registrado com preço final igual ao preço

#### Scenario: produto com desconto percentual

- **WHEN** um produto com preço de R$ 100,00 é cadastrado com desconto unitário de 10%
- **THEN** o produto é registrado com preço final de R$ 90,00

#### Scenario: produto com desconto monetário

- **WHEN** um produto com preço de R$ 100,00 é cadastrado com desconto unitário de R$ 5,00
- **THEN** o produto é registrado com preço final de R$ 95,00

#### Scenario: produto com as duas modalidades de desconto

- **WHEN** um produto é cadastrado com desconto percentual e desconto monetário
- **THEN** o cadastro é recusado e a resposta indica que as modalidades são exclusivas

### Requirement: Desconto unitário não pode ser negativo nem exceder o preço

O sistema MUST recusar desconto unitário com valor negativo e MUST recusar desconto que resulte em preço final menor que zero.

#### Scenario: desconto unitário negativo

- **WHEN** um produto é cadastrado com desconto unitário de -5%
- **THEN** o cadastro é recusado e a resposta indica que o desconto não pode ser negativo

#### Scenario: desconto unitário maior que o preço

- **WHEN** um produto com preço de R$ 100,00 é cadastrado com desconto unitário de R$ 120,00
- **THEN** o cadastro é recusado e a resposta indica que o preço final não pode ser negativo
