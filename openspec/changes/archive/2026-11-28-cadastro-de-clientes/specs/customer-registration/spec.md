# Spec Delta

## Purpose

Define o cadastro de clientes no domínio, com validação de nome completo, e-mail, telefone no padrão brasileiro e idade mínima de 18 anos.

## ADDED Requirements

### Requirement: Cliente deve informar nome completo
O sistema DEVE (MUST) recusar o cadastro de um cliente cujo nome não contenha nome e sobrenome.

#### Scenario: nome completo informado e aceito
- **GIVEN** que o nome informado contém nome e sobrenome
- **WHEN** o cliente é cadastrado
- **THEN** o cadastro é aceito

#### Scenario: nome com apenas um termo é recusado
- **GIVEN** que o nome informado contém apenas um termo, sem sobrenome
- **WHEN** o cliente é cadastrado
- **THEN** o cadastro é recusado e uma mensagem indica que o nome completo é obrigatório

#### Scenario: nome vazio é recusado
- **GIVEN** que o nome informado está vazio
- **WHEN** o cliente é cadastrado
- **THEN** o cadastro é recusado e uma mensagem indica que o nome completo é obrigatório

### Requirement: E-mail do cliente deve ser válido
O sistema DEVE (MUST) aceitar somente e-mails em formato válido.

#### Scenario: e-mail válido é aceito
- **GIVEN** que o e-mail informado está em formato válido
- **WHEN** o cliente é cadastrado
- **THEN** o cadastro é aceito

#### Scenario: e-mail inválido é recusado
- **GIVEN** que o e-mail informado não está em formato válido
- **WHEN** o cliente é cadastrado
- **THEN** o cadastro é recusado e uma mensagem indica que o e-mail é inválido

### Requirement: Telefone do cliente deve seguir o padrão brasileiro
O sistema DEVE (MUST) aceitar somente telefones válidos no padrão brasileiro.

#### Scenario: telefone brasileiro válido é aceito
- **GIVEN** que o telefone informado segue o padrão brasileiro válido
- **WHEN** o cliente é cadastrado
- **THEN** o cadastro é aceito

#### Scenario: telefone inválido é recusado
- **GIVEN** que o telefone informado não segue o padrão brasileiro
- **WHEN** o cliente é cadastrado
- **THEN** o cadastro é recusado e uma mensagem indica que o telefone é inválido

### Requirement: Cliente deve ser maior de 18 anos
O sistema DEVE (MUST) recusar o cadastro de cliente cuja idade, calculada a partir da data de nascimento, seja inferior a 18 anos.

#### Scenario: cliente maior de 18 anos é aceito
- **GIVEN** que a data de nascimento resulta em idade de 18 anos ou mais
- **WHEN** o cliente é cadastrado
- **THEN** o cadastro é aceito

#### Scenario: cliente menor de 18 anos é recusado
- **GIVEN** que a data de nascimento resulta em idade inferior a 18 anos
- **WHEN** o cliente é cadastrado
- **THEN** o cadastro é recusado e uma mensagem indica que o cliente deve ser maior de 18 anos

### Requirement: Dados do cadastro são obrigatórios
O sistema DEVE (MUST) recusar o cadastro quando nome, data de nascimento, e-mail ou telefone não forem informados.

#### Scenario: todos os dados informados e aceito
- **GIVEN** que nome, data de nascimento, e-mail e telefone foram informados
- **WHEN** o cliente é cadastrado
- **THEN** o cadastro é aceito

#### Scenario: data de nascimento ausente é recusada
- **GIVEN** que nome, e-mail e telefone foram informados, mas a data de nascimento não
- **WHEN** o cliente é cadastrado
- **THEN** o cadastro é recusado e uma mensagem indica que a data de nascimento é obrigatória