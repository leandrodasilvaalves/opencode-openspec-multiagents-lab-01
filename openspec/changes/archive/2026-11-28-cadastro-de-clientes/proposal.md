# Proposal

## Why

O laboratório precisa de um domínio de cadastro de clientes com regras de validação explícitas e testáveis. Hoje a solução `Lab.Orders` é apenas um scaffold vazio na camada de domínio (`Class1.cs`), sem nenhuma regra de cliente formalizada. Começar pela definição do agregado `Customer` e de seus value objects evita decisões implícitas sobre formato de nome, e-mail, telefone e idade mínima.

## What Changes

- Novo agregado de domínio `Customer` com os dados: nome completo, data de nascimento, e-mail e telefone.
- Value objects de domínio para `Email`, `PhoneNumber` (padrão brasileiro) e `FullName`, além do cálculo de idade a partir da data de nascimento.
- Regras de validação no domínio:
  - Nome completo obrigatório (nome e sobrenome).
  - E-mail em formato válido.
  - Telefone válido no padrão brasileiro.
  - Cliente maior de 18 anos.
- **Fora de escopo:** documento (CPF/CNPJ), endereço de entrega, persistência, Application, API, Infraestrutura e testes unitários. O cadastro de clientes com documento/endereço já é tratado em outra change (`sistema-de-pedidos-mvp`) e não é alterado aqui.

## Capabilities

### New Capabilities

- `customer-registration`: cadastro de cliente no domínio, com validação de nome completo, e-mail, telefone (padrão brasileiro) e idade mínima de 18 anos.

### Modified Capabilities

Nenhuma. Esta change não altera requirements de specs existentes.

## Impact

- Domain: novo agregado `Customer` e value objects (`FullName`, `Email`, `PhoneNumber`, `DateOfBirth`), com invariantes e exceções de domínio. **Código em inglês; sem testes unitários nesta etapa.**
- Application/Infrastructure/Api: não afetadas nesta change.
- Conflito conhecido: a capability `client-registration` da change `sistema-de-pedidos-mvp` descreve um modelo de cliente diferente (documento/endereço). A coexistência dos dois modelos deverá ser resolvida pelo `@tech-architect`/humano no refinamento; esta change não a modifica.
- Convenções: artefato em português; código em inglês; sem comentários; ver `AGENTS.md`.