# Spec Delta

## Purpose

Define o cadastro de clientes do sistema de pedidos, incluindo a validacao do documento (CPF ou CNPJ) e do endereco de entrega que identifica onde o pedido deve ser entregue.

## ADDED Requirements

### Requirement: Cadastro de cliente exige todos os dados obrigatorios
O sistema DEVE (MUST) recusar o cadastro de um cliente que nao informe nome, telefone, documento e endereco de entrega.

#### Scenario: cadastro completo e aceito
- **GIVEN** que o nome, o telefone, o documento e o endereco de entrega foram informados
- **WHEN** o cliente e cadastrado
- **THEN** o cadastro e aceito e o cliente fica disponivel para ser associado a um pedido

#### Scenario: cadastro sem nome e recusado
- **GIVEN** que o telefone, o documento e o endereco foram informados, mas o nome nao
- **WHEN** o cliente e cadastrado
- **THEN** o cadastro e recusado e uma mensagem indica que o nome e obrigatorio

#### Scenario: cadastro sem telefone e recusado
- **GIVEN** que o nome, o documento e o endereco foram informados, mas o telefone nao
- **WHEN** o cliente e cadastrado
- **THEN** o cadastro e recusado e uma mensagem indica que o telefone e obrigatorio

#### Scenario: cadastro sem documento e recusado
- **GIVEN** que o nome, o telefone e o endereco foram informados, mas o documento nao
- **WHEN** o cliente e cadastrado
- **THEN** o cadastro e recusado e uma mensagem indica que o documento e obrigatorio

#### Scenario: cadastro sem endereco de entrega e recusado
- **GIVEN** que o nome, o telefone e o documento foram informados, mas o endereco de entrega nao
- **WHEN** o cliente e cadastrado
- **THEN** o cadastro e recusado e uma mensagem indica que o endereco de entrega e obrigatorio

### Requirement: Documento do cliente deve ser um CPF ou CNPJ valido
O sistema DEVE (MUST) validar o documento do cliente quanto ao formato e aos digitos verificadores, aceitando somente CPF ou CNPJ validos.

#### Scenario: CPF valido e aceito
- **GIVEN** que o documento informado e um CPF com digitos verificadores validos
- **WHEN** o cliente e cadastrado
- **THEN** o documento e aceito

#### Scenario: CNPJ valido e aceito
- **GIVEN** que o documento informado e um CNPJ com digitos verificadores validos
- **WHEN** o cliente e cadastrado
- **THEN** o documento e aceito

#### Scenario: CPF com digito verificador invalido e recusado
- **GIVEN** que o documento informado tem formato de CPF, mas um digito verificador invalido
- **WHEN** o cliente e cadastrado
- **THEN** o cadastro e recusado e uma mensagem indica que o CPF e invalido

#### Scenario: CNPJ com digito verificador invalido e recusado
- **GIVEN** que o documento informado tem formato de CNPJ, mas um digito verificador invalido
- **WHEN** o cliente e cadastrado
- **THEN** o cadastro e recusado e uma mensagem indica que o CNPJ e invalido

#### Scenario: documento que nao e CPF nem CNPJ e recusado
- **GIVEN** que o documento informado nao corresponde ao formato de CPF ou CNPJ
- **WHEN** o cliente e cadastrado
- **THEN** o cadastro e recusado e uma mensagem indica que somente CPF ou CNPJ sao aceitos
