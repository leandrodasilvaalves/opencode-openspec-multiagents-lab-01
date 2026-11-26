# Spec Delta

## Purpose

Define os dados que identificam um cliente e o local de entrega, garantindo que todo pedido possa ser atribuído a uma pessoa e a um endereço válidos antes de ser registrado.

## ADDED Requirements

### Requirement: Cliente deve possuir dados de identificação completos

O sistema MUST exigir nome, telefone, documento e endereço de entrega no cadastro e MUST recusar o cadastro quando algum desses dados estiver ausente.

#### Scenario: cadastro com todos os dados obrigatórios

- **WHEN** um cliente é cadastrado com nome, telefone, documento e endereço de entrega
- **THEN** o cliente é registrado e fica disponível para ser usado em um pedido

#### Scenario: cadastro sem telefone

- **WHEN** um cliente é cadastrado sem informar o telefone
- **THEN** o cadastro é recusado e a resposta indica o campo obrigatório ausente

### Requirement: Documento do cliente deve ser CPF ou CNPJ válido

O sistema MUST aceitar documentos somente nos tipos CPF e CNPJ e MUST validar o formato do documento antes de registrar o cliente.

#### Scenario: cadastro com CPF válido

- **WHEN** um cliente é cadastrado com um CPF em formato válido
- **THEN** o documento é registrado como do tipo CPF

#### Scenario: cadastro com CNPJ válido

- **WHEN** um cliente é cadastrado com um CNPJ em formato válido
- **THEN** o documento é registrado como do tipo CNPJ

#### Scenario: cadastro com documento em formato inválido

- **WHEN** um cliente é cadastrado com um documento que não é CPF nem CNPJ válidos
- **THEN** o cadastro é recusado e a resposta indica que o documento é inválido

### Requirement: Endereço de entrega deve estar completo

O sistema MUST registrar o endereço de entrega com CEP, logradouro, número, bairro, cidade e unidade federativa, permitindo complemento opcional.

#### Scenario: endereço completo informado

- **WHEN** um cliente é cadastrado com CEP, logradouro, número, bairro, cidade e unidade federativa
- **THEN** o endereço de entrega é registrado

#### Scenario: endereço sem unidade federativa

- **WHEN** um cliente é cadastrado com endereço sem a unidade federativa
- **THEN** o cadastro é recusado e a resposta indica o campo ausente
