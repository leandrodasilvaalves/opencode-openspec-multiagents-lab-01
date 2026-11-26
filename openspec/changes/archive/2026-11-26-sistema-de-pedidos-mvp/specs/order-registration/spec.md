# Spec Delta

## Purpose

Define o registro do pedido como agregacao de cliente, itens, forma de pagamento, desconto e taxa de entrega, com as validacoes de entrada e o calculo do total final.

## ADDED Requirements

### Requirement: Pedido deve referenciar um cliente cadastrado
O sistema DEVE (MUST) recusar o registro de um pedido sem cliente ou com cliente inexistente.

#### Scenario: cliente cadastrado e aceito
- **GIVEN** que existe um cliente cadastrado com documento valido
- **WHEN** o pedido e registrado para esse cliente
- **THEN** o pedido e associado ao cliente

#### Scenario: pedido sem cliente e recusado
- **GIVEN** que nenhum cliente foi informado
- **WHEN** o pedido e registrado
- **THEN** o pedido e recusado e uma mensagem indica que o cliente e obrigatorio

#### Scenario: cliente inexistente e recusado
- **GIVEN** que o cliente informado nao existe no cadastro
- **WHEN** o pedido e registrado
- **THEN** o pedido e recusado e uma mensagem indica que o cliente nao foi encontrado

### Requirement: Pedido deve conter ao menos um produto
O sistema DEVE (MUST) recusar o registro de um pedido que nao possua nenhum item.

#### Scenario: pedido com um item e aceito
- **GIVEN** que o pedido possui um item com SKU existente e quantidade maior que zero
- **WHEN** o pedido e registrado
- **THEN** o pedido e criado

#### Scenario: pedido com varios itens e aceito
- **GIVEN** que o pedido possui dois itens validos
- **WHEN** o pedido e registrado
- **THEN** o pedido e criado com os dois itens

#### Scenario: pedido sem itens e recusado
- **GIVEN** que o pedido nao possui nenhum item
- **WHEN** o registrador de pedido e acionado
- **THEN** o pedido nao e criado e uma mensagem explica que ao menos um produto e obrigatorio

### Requirement: Forma de pagamento deve ser uma das opcoes aceitas
O sistema DEVE (MUST) aceitar somente dinheiro, pix, cartao de credito ou cartao de debito como forma de pagamento, sem validar dados de cartao neste MVP.

#### Scenario: pagamento em dinheiro e aceito
- **GIVEN** que a forma de pagamento informada e dinheiro
- **WHEN** o pedido e registrado
- **THEN** o pedido e criado com pagamento em dinheiro

#### Scenario: pagamento com pix e aceito
- **GIVEN** que a forma de pagamento informada e pix
- **WHEN** o pedido e registrado
- **THEN** o pedido e criado com pagamento via pix

#### Scenario: pagamento com cartao de credito e aceito
- **GIVEN** que a forma de pagamento informada e cartao de credito
- **WHEN** o pedido e registrado
- **THEN** o pedido e criado sem exigir validacao de dados do cartao

#### Scenario: pagamento com cartao de debito e aceito
- **GIVEN** que a forma de pagamento informada e cartao de debito
- **WHEN** o pedido e registrado
- **THEN** o pedido e criado sem exigir validacao de dados do cartao

#### Scenario: pedido sem forma de pagamento e recusado
- **GIVEN** que nenhuma forma de pagamento foi informada
- **WHEN** o pedido e registrado
- **THEN** o pedido e recusado e uma mensagem indica que a forma de pagamento e obrigatoria

#### Scenario: forma de pagamento nao listada e recusada
- **GIVEN** que a forma de pagamento informada nao esta entre dinheiro, pix, credito e debito
- **WHEN** o pedido e registrado
- **THEN** o pedido e recusado e uma mensagem lista as formas de pagamento aceitas

### Requirement: Total do pedido e calculado com piso zero antes da taxa
O sistema DEVE (MUST) calcular o total como o maior valor entre zero e (subtotal dos itens menos desconto do pedido), mais a taxa de entrega, aplicando o desconto somente ao subtotal.

#### Scenario: pedido sem desconto
- **GIVEN** que o subtotal dos itens e R$ 100,00, nao ha desconto do pedido e a taxa e R$ 12,00
- **WHEN** o pedido e registrado
- **THEN** o total e R$ 112,00

#### Scenario: pedido com desconto em valor
- **GIVEN** que o subtotal dos itens e R$ 100,00, o desconto do pedido e R$ 20,00 e a taxa e R$ 12,00
- **WHEN** o pedido e registrado
- **THEN** o total e R$ 92,00

#### Scenario: pedido com desconto em percentual
- **GIVEN** que o subtotal dos itens e R$ 200,00, o desconto do pedido e dez por cento e a taxa e R$ 12,00
- **WHEN** o pedido e registrado
- **THEN** o total e R$ 192,00

#### Scenario: desconto igual ao subtotal deixa apenas a taxa
- **GIVEN** que o subtotal dos itens e R$ 100,00, o desconto do pedido e R$ 100,00 e a taxa e R$ 12,00
- **WHEN** o pedido e registrado
- **THEN** o total e R$ 12,00

#### Scenario: pedido com itens descontados nao admite desconto do pedido
- **GIVEN** que um item possui desconto unitario e um desconto do pedido foi informado
- **WHEN** o pedido e registrado
- **THEN** o pedido e recusado e somente os descontos individuais devem ser aplicados

#### Scenario: desconto maior que o subtotal recusa o pedido
- **GIVEN** que o subtotal dos itens e R$ 100,00 e o desconto do pedido e R$ 120,00
- **WHEN** o pedido e registrado
- **THEN** o pedido e recusado e uma mensagem indica que o desconto nao pode tornar o total negativo

### Requirement: Criacao de pedido e idempotente
O sistema DEVE (MUST) garantir que registrar um pedido com a mesma chave de idempotencia e o mesmo conteudo nao crie pedido duplicado, retornando o mesmo resultado.

#### Scenario: mesma chave de idempotencia nao duplica pedido
- **GIVEN** que um pedido foi registrado com uma `IdempotencyKey`
- **WHEN** o mesmo pedido e reenviado com a mesma `IdempotencyKey`
- **THEN** nao e criado um novo pedido e o mesmo resultado e retornado

#### Scenario: chave de idempotencia diferente cria pedido novo
- **GIVEN** que um pedido foi registrado com uma `IdempotencyKey`
- **WHEN** o mesmo conteudo e enviado com outra `IdempotencyKey`
- **THEN** um novo pedido e criado
