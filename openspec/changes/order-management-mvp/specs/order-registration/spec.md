# Spec Delta

## Purpose

Define a composição mínima de um pedido e a aplicação das regras de cliente, catálogo, desconto e entrega no ato do registro, garantindo que nenhum pedido inválido seja aceito.

## ADDED Requirements

### Requirement: Pedido deve conter cliente, itens, forma de pagamento e distância

O sistema MUST exigir cliente, um ou mais itens, forma de pagamento e distância no registro do pedido e MUST recusar pedido sem itens.

#### Scenario: pedido completo é registrado

- **WHEN** um pedido é registrado com cliente existente, ao menos um item, forma de pagamento válida e distância válida
- **THEN** o pedido é registrado com subtotal, desconto, taxa de entrega e total calculados

#### Scenario: pedido sem itens é recusado

- **WHEN** um pedido é registrado sem nenhum item
- **THEN** o pedido é recusado e a resposta indica que ao menos um produto é obrigatório

### Requirement: Itens do pedido referenciam produtos do catálogo com quantidade válida

O sistema MUST aceitar nos itens somente SKUs existentes no catálogo e MUST exigir quantidade inteira maior ou igual a um.

#### Scenario: item com SKU inexistente

- **WHEN** um pedido é registrado com um item cujo SKU não existe no catálogo
- **THEN** o pedido é recusado e a resposta indica o SKU não encontrado

#### Scenario: item com quantidade zero

- **WHEN** um pedido é registrado com um item de quantidade zero
- **THEN** o pedido é recusado e a resposta indica que a quantidade deve ser ao menos um

#### Scenario: item com quantidade maior que um

- **WHEN** um pedido é registrado com um item de SKU existente e quantidade dois
- **THEN** o pedido é registrado e o subtotal considera o preço final multiplicado por dois

### Requirement: Forma de pagamento deve ser uma das opções aceitas

O sistema MUST aceitar somente as formas de pagamento PIX, Crédito, Débito e Dinheiro, sem cobrar taxas próprias neste MVP.

#### Scenario: pagamento com PIX

- **WHEN** um pedido é registrado com a forma de pagamento PIX
- **THEN** o pedido é aceito sem acréscimo de valor

#### Scenario: forma de pagamento desconhecida

- **WHEN** um pedido é registrado com uma forma de pagamento fora da lista aceita
- **THEN** o pedido é recusado e a resposta indica a forma de pagamento inválida

### Requirement: Registro aplica e valida as regras de desconto, entrega e preço

O sistema MUST aplicar as regras das capacidades `discount-policy`, `delivery-fee` e `pricing` no registro do pedido e MUST recusar o pedido quando qualquer uma de suas invariantes for violada.

#### Scenario: pedido recusado por distância acima do limite

- **WHEN** um pedido é registrado com distância de 120 km e desconto de 10%
- **THEN** o pedido é recusado pela regra de distância máxima e nenhum total é persistido

#### Scenario: pedido recusado por conflito de descontos

- **WHEN** um pedido contém um produto com desconto unitário e é registrado com desconto de pedido
- **THEN** o pedido é recusado pela regra de exclusividade e nenhum total é persistido
