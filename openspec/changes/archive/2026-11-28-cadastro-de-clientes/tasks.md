# Tasks

> Re-derivado do `design.md` aprovado. **Artefato em portugues; identificadores em ingles.** Sem testes unitários nesta etapa. Build `dotnet build --no-restore`. **Regra de Ouro.**

## 0. Fundacao do dominio

- [x] 0.1 Criar `Contracts/` (`Error`, `Validatable`, `ValueObject<T>`, `Event<T>`, `Entity<T>`) e `Exceptions/` (`DomainException`, `DomainNullException`, `DomainRuleException`, `DomainOutOfRangeException`); remover `Class1.cs`; verificar com `dotnet build --no-restore`

## 1. Value Objects

- [x] 1.1 Implementar `FullName` (>=2 termos, trim, nao-vazio); verificar com `dotnet build --no-restore`
- [x] 1.2 Implementar `Email` (formato: um `@`, dominio com ponto; normalizado); verificar com `dotnet build --no-restore`
- [x] 1.3 Implementar `PhoneNumber` (10/11 digitos + DDD valido; normalizado); verificar com `dotnet build --no-restore`
- [x] 1.4 Implementar `DateOfBirth` (`DateOnly`; idade derivada; recusa <18 e data futura); verificar com `dotnet build --no-restore`

## 2. Agregado Customer

- [x] 2.1 Implementar `Customer : Entity` com `Register`, `ValidateRules` (obrigatorios + `Validate(child)`) e evento `CustomerRegistered`; verificar com `dotnet build --no-restore`

## 3. Verificacao

- [x] 3.1 Compilar a solucao com `dotnet build --no-restore` e confirmar build sem erros

## Workflow follow-up

- Implementacao com `@ca-architect` + `/opsx-apply`.