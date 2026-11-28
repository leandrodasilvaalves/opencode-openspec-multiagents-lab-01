# Design

## Context

A camada `Lab.Orders.Domain` está vazia (apenas `Class1.cs`) e não possui contratos base. Esta change introduz o primeiro agregado do laboratório seguindo o padrão da skill `dotnet-domain-modeling` (event sourcing leve). Ver motivação em `proposal.md` e requisitos em `specs/customer-registration/spec.md`.

Stack: **.NET 10, C# 14, Nullable habilitado**; build `dotnet build --no-restore`; **Regra de Ouro** (verificado por `@dotnet-reviewer`). Escopo **só Domain**; **sem testes unitários** nesta etapa.

## Goals / Non-Goals

**Goals:**
- Modelar `Customer` como agregado com invariantes de nome, e-mail, telefone e idade.
- Isolar regras de formato em value objects reutilizáveis.
- Bootstrap dos contratos base da skill (projeto não os possui).

**Non-Goals:**
- Persistência, Application, API, Infraestrutura e testes unitários.
- Documento (CPF/CNPJ) e endereço (tratados em outra change).

## Convencoes de codigo (OBRIGATORIO)

- **Codigo em ingles**; artefatos em portugues; **sem comentarios/XML doc** (AGENTS.md).
- Naming: `Customer`, `CustomerId`, `FullName`, `Email`, `PhoneNumber`, `DateOfBirth`, `CustomerRegistered`.

## Decisions

### 1. Bootstrap dos contratos base
Criar `Contracts/` (`Error`, `Validatable`, `ValueObject<T>`, `Event<T>`, `Entity<T>`) e `Exceptions/` (`DomainException` + `DomainNullException`, `DomainRuleException`, `DomainOutOfRangeException`) conforme a skill. Remover `Class1.cs`. **Alternativa:** duplicar lógicas — descartada.

### 2. Agregado `Customer`
`Customer : Entity` (Guid) com value objects `FullName`, `Email`, `PhoneNumber`, `DateOfBirth`. Mutação por `Register(...)`; `ValidateRules()` valida obrigatórios e chama `Validate(child)`; emite `CustomerRegistered`. **Alternativa:** validar tudo no agregado — descartada (acopla formato).

### 3. `FullName`
Ao menos dois termos (nome + sobrenome), trim, não-vazio. **Alternativa:** só não-vazio — descartada (spec exige nome completo).

### 4. `Email`
Formato válido: um `@`, domínio com ponto; normalizado (lower/trim). Sem verificação de existência. **Alternativa:** regex complexa — descartada.

### 5. `PhoneNumber`
Normaliza para dígitos; aceita 10 (fixo) ou 11 (móvel com 9º dígito) dígitos + DDD válido. **Alternativa:** texto livre — descartada.

### 6. `DateOfBirth` + maioridade
`DateOnly`; idade derivada no cadastro; recusa `< 18` e data futura. Idade não armazenada. **Alternativa:** armazenar idade — descartada.

## Risks / Trade-offs

- [Conflito com `client-registration` de `sistema-de-pedidos-mvp` (modelo com documento/endereço)] → resolvido no refinamento com `@tech-architect`/humano; esta change não altera aquela spec.
- [Variações de telefone brasileiro] → regra 10/11 dígitos + DDD; ajustável com teste.
- [Event sourcing leve adiciona complexidade] → padrão do laboratório; reversível.

## Migration Plan / Reversibilidade

Sem dados/código anterior. Rollback = não aplicar a change. Reversível: contratos base podem ser removidos; VOs ajustados sem impacto externo.

## ADRs

- **ADR-001 — Contratos base + event sourcing leve:** bootstrap `Contracts`/`Exceptions`; `Customer` gera `CustomerRegistered`. Reverter: remover eventos.
- **ADR-002 — `FullName`/`Email`/`PhoneNumber`:** VOs auto-validáveis; telefone BR 10/11 dígitos + DDD. Reverter: ajustar regra.
- **ADR-003 — `DateOfBirth` + maioridade:** `DateOnly`, idade derivada, ≥18, não-futura. Reverter: mudar cálculo.