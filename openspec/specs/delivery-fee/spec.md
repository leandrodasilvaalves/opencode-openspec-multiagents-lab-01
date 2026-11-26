# delivery-fee Specification

## Purpose

Define o calculo da taxa de entrega a partir da distancia informada em quilometros, com arredondamento, faixa minima, acrescimo por km adicional e limite maximo, sem integracao com servicos de mapas no MVP.

## Requirements

### Requirement: Distancia de entrega deve ser informada como entrada
O sistema DEVE (MUST) exigir a distancia em quilometros como entrada do pedido, sem consultar servicos de mapas.

#### Scenario: distancia informada e aceita
- **GIVEN** que a distancia de entrega foi informada em quilometros
- **WHEN** o pedido e registrado
- **THEN** a distancia informada e utilizada para calcular a taxa

#### Scenario: distancia nao informada recusa o pedido
- **GIVEN** que nenhuma distancia de entrega foi informada
- **WHEN** o pedido e registrado
- **THEN** o pedido e recusado e uma mensagem indica que a distancia e obrigatoria

#### Scenario: distancia negativa recusa o pedido
- **GIVEN** que a distancia informada e menor que zero quilometros
- **WHEN** o pedido e registrado
- **THEN** o pedido e recusado e uma mensagem indica que a distancia nao pode ser negativa

### Requirement: Distancia informada deve ser arredondada no primeiro decimal
O sistema DEVE (MUST) arredondar a distancia para o quilometro inteiro mais proximo antes de aplicar qualquer regra de taxa ou limite, com casa decimal menor que cinco arredondando para baixo e igual ou maior que cinco arredondando para cima.

#### Scenario: casa decimal menor que cinco arredonda para baixo
- **GIVEN** que a distancia informada e 15,2 quilometros
- **WHEN** a taxa de entrega e calculada
- **THEN** a distancia considerada e 15 quilometros

#### Scenario: casa decimal igual a quatro arredonda para baixo
- **GIVEN** que a distancia informada e 15,4 quilometros
- **WHEN** a taxa de entrega e calculada
- **THEN** a distancia considerada e 15 quilometros

#### Scenario: casa decimal igual a cinco arredonda para cima
- **GIVEN** que a distancia informada e 15,5 quilometros
- **WHEN** a taxa de entrega e calculada
- **THEN** a distancia considerada e 16 quilometros

#### Scenario: casa decimal maior que cinco arredonda para cima
- **GIVEN** que a distancia informada e 15,9 quilometros
- **WHEN** a taxa de entrega e calculada
- **THEN** a distancia considerada e 16 quilometros

### Requirement: Taxa minima de entrega e de doze reais ate quinze quilometros
O sistema DEVE (MUST) cobrar R$ 12,00 de taxa de entrega para distancias de ate 15 quilometros, ja arredondadas.

#### Scenario: distancia igual a quinze quilometros
- **GIVEN** que a distancia considerada e 15 quilometros
- **WHEN** a taxa de entrega e calculada
- **THEN** a taxa e R$ 12,00

#### Scenario: distancia menor que quinze quilometros
- **GIVEN** que a distancia considerada e 10 quilometros
- **WHEN** a taxa de entrega e calculada
- **THEN** a taxa e R$ 12,00

#### Scenario: distancia arredondada para quinze quilometros
- **GIVEN** que a distancia informada e 15,2 quilometros
- **WHEN** a taxa de entrega e calculada
- **THEN** a taxa e R$ 12,00

### Requirement: Acima de quinze quilometros cobra-se um real por quilometro adicional
O sistema DEVE (MUST) cobrar R$ 12,00 mais R$ 1,00 por cada quilometro inteiro adicional acima de 15 quilometros.

#### Scenario: um quilometro adicional
- **GIVEN** que a distancia considerada e 16 quilometros
- **WHEN** a taxa de entrega e calculada
- **THEN** a taxa e R$ 13,00

#### Scenario: cinco quilometros adicionais
- **GIVEN** que a distancia considerada e 20 quilometros
- **WHEN** a taxa de entrega e calculada
- **THEN** a taxa e R$ 17,00

#### Scenario: distancia arredondada para cima entra na faixa adicional
- **GIVEN** que a distancia informada e 15,5 quilometros
- **WHEN** a taxa de entrega e calculada
- **THEN** a taxa e R$ 13,00

### Requirement: Distancia acima de cem quilometros recusa o pedido
O sistema DEVE (MUST) recusar o pedido quando a distancia considerada, apos o arredondamento, for maior que 100 quilometros.

#### Scenario: distancia igual a cem quilometros e aceita
- **GIVEN** que a distancia considerada e 100 quilometros
- **WHEN** o pedido e registrado
- **THEN** o pedido e aceito e a taxa e R$ 97,00

#### Scenario: distancia acima de cem quilometros e recusada
- **GIVEN** que a distancia considerada e 101 quilometros
- **WHEN** o pedido e registrado
- **THEN** o pedido e recusado e uma mensagem indica que o limite de entrega e 100 quilometros

#### Scenario: distancia fracionaria arredondada para cem quilometros e aceita
- **GIVEN** que a distancia informada e 100,4 quilometros
- **WHEN** o pedido e registrado
- **THEN** o pedido e aceito com a distancia considerada de 100 quilometros

#### Scenario: distancia fracionaria arredondada para cem e um quilometros e recusada
- **GIVEN** que a distancia informada e 100,5 quilometros
- **WHEN** o pedido e registrado
- **THEN** o pedido e recusado com a distancia considerada de 101 quilometros
