# Spec Delta

## Purpose

Determina o valor cobrado pela entrega a partir da distância informada no pedido, sem dependência de serviços externos de mapas, e define o limite além do qual a entrega não é aceita.

## ADDED Requirements

### Requirement: Taxa mínima de entrega é R$ 12,00 até 15 km

O sistema MUST cobrar R$ 12,00 de taxa de entrega para qualquer distância maior que zero e menor ou igual a 15 km, sem consulta a serviços externos.

#### Scenario: entrega a 10 km

- **WHEN** a distância informada no pedido é 10 km
- **THEN** a taxa de entrega calculada é R$ 12,00

#### Scenario: entrega no limite de 15 km

- **WHEN** a distância informada no pedido é 15 km
- **THEN** a taxa de entrega calculada é R$ 12,00

### Requirement: Acima de 15 km a taxa acrescida de R$ 1,00 por km adicional

O sistema MUST cobrar R$ 12,00 mais R$ 1,00 por quilômetro que exceda 15 km, para distâncias superiores a 15 km e inferiores ou iguais a 100 km.

#### Scenario: entrega a 20 km

- **WHEN** a distância informada no pedido é 20 km
- **THEN** a taxa de entrega calculada é R$ 17,00

#### Scenario: entrega no limite de 100 km

- **WHEN** a distância informada no pedido é 100 km
- **THEN** a taxa de entrega calculada é R$ 97,00

#### Scenario: distância fracionária

- **WHEN** a distância informada no pedido é 15,5 km
- **THEN** a taxa de entrega calculada é R$ 12,50

### Requirement: Pedido com distância acima de 100 km é recusado

O sistema MUST recusar o pedido quando a distância informada for superior a 100 km.

#### Scenario: entrega a 101 km

- **WHEN** a distância informada no pedido é 101 km
- **THEN** o pedido é recusado e a resposta indica que a distância máxima é 100 km

### Requirement: Distância informada não pode ser negativa

O sistema MUST recusar o pedido quando a distância informada for menor que zero.

#### Scenario: distância negativa

- **WHEN** a distância informada no pedido é -5 km
- **THEN** o pedido é recusado e a resposta indica que a distância é inválida
