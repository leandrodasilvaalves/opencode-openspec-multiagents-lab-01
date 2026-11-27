---
name: dotnet-domain-modeling
description: Cria ou estende a camada de Domínio (.NET) seguindo um padrão DDD com event sourcing leve. Entity, ValueObject, Validatable, Events, Commands, Enums e Exceptions de domínio. Use quando precisar criar ou evoluir agregados/entidades, value objects, eventos de domínio, comandos, enums ou regras de domínio em projetos `*.Domain`, incluindo bootstrap dos contratos base em projetos novos.
---

# Padrão de Modelagem de Domínio (.NET)

Skill genérica para criar/evoluir a camada **Domain** em projetos .NET, independente do nome do projeto ou contexto de negócio.

## Princípios

- **Código em inglês** (tipos, membros, parâmetros, namespaces, arquivos, pastas). Artefatos/docs podem ser em pt-BR.
- **Sem comentários/XML doc** por padrão; código autoexplicativo.
- **Domínio rico**: comportamento e invariantes dentro da entidade/VO; sem setters públicos.
- **Validação fail-fast**: `Validate()` lança `DomainRuleException` quando inválido.
- **Eventos de domínio**: mutações de estado relevantes geram evento via `AddEvent`, que incrementa versão e aplica o evento.
- **Sem dependências externas** na camada Domain (apenas BCL/.NET).
- **Adaptar ao projeto existente**: se o projeto já possui contratos base, siga a convenção/nomenclatura já estabelecida. Se estiver iniciando do zero, use os templates canônicos desta skill.

## Estrutura de pastas

```
src/<Project>.Domain/
  <Project>.Domain.csproj
  Contracts/
    Entity.cs
    ValueObject.cs
    Validatable.cs
    Event.cs
    Error.cs
  Entities/
    <Entity>.cs
  ValueObjects/
    <ValueObject>.cs
  Enums/
    <Enum>.cs
  Events/
    <Entities>/            # plural da entidade/agregado
      <Entity><PastAction>.cs
  Commands/
    <Entities>/
      I<Action><Entity>.cs
  Exceptions/
    DomainException.cs
    DomainNullException.cs
    DomainRuleException.cs
    DomainOutOfRangeException.cs
    <SpecificDomainException>.cs   # somente quando fizer sentido
```

> Ajuste namespaces conforme o root namespace do projeto (ex.: `Company.Product.Domain.*`).

## Bootstrap de contratos base (projeto novo)

Crie estes contratos quando o projeto ainda não os possui. Se já existirem, não duplique: estenda/replique o padrão existente.

### Error

```csharp
namespace <RootNamespace>.Contracts;

public record Error(string Code, string Message);
```

### Validatable

```csharp
using <RootNamespace>.Exceptions;

namespace <RootNamespace>.Contracts;

public abstract class Validatable
{
    private readonly List<Error> _errors = [];

    public IReadOnlyCollection<Error> Errors => _errors;

    protected void AddError(string message)
    {
        var error = new Error(ErrorCode(), message);
        if (!HasError(error))
        {
            _errors.Add(error);
        }
    }

    protected void AddErrors(IEnumerable<Error> errors)
    {
        DomainNullException.ThrowIfNull(errors);
        _errors.AddRange(errors);
    }

    protected void ClearErrors() => _errors.Clear();

    protected bool IsValid() => _errors.Count == 0;

    protected bool IsInvalid() => !IsValid();

    protected bool HasError(Error error) => _errors.Contains(error);

    protected abstract void Validate();

    protected abstract string ErrorCode();

    protected void Validate(Validatable? validatable)
    {
        DomainNullException.ThrowIfNull(validatable);

        validatable.Validate();
        if (validatable.IsInvalid())
        {
            var suffixError = validatable.GetType().Name.ToUpperInvariant();
            AddErrors(validatable.Errors.Select(e =>
                new Error($"{ErrorCode()}_{suffixError}", e.Message)));
        }
    }
}
```

### ValueObject<T>

```csharp
namespace <RootNamespace>.Contracts;

public abstract class ValueObject<T> : Validatable, IEquatable<T>
{
    public abstract bool Equals(T? other);
    public abstract override bool Equals(object? obj);
    public abstract override int GetHashCode();
    public abstract override string ToString();
}
```

### Event<T>

```csharp
using <RootNamespace>.Exceptions;

namespace <RootNamespace>.Contracts;

public abstract class Event<T>
{
    public T? AggregateId { get; protected set; }

    public long AggregateVersion { get; protected set; }

    public DateTimeOffset Timestamp { get; protected set; }

    public DateTimeOffset CreatedAt { get; protected set; }

    public DateTimeOffset UpdatedAt { get; protected set; }

    public abstract string Key { get; }

    protected Event()
    {
        Timestamp = DateTimeOffset.UtcNow;
    }

    protected Event(Entity<T> entity) : this()
    {
        SetAggregateId(entity.Id);
        SetAggregateVersion(entity.Version);
        CreatedAt = entity.CreatedAt;
        UpdatedAt = entity.UpdatedAt;
    }

    public void SetAggregateId(T aggregateId)
    {
        DomainNullException.ThrowIfNull(aggregateId);
        AggregateId = aggregateId;
    }

    public void SetAggregateVersion(long aggregateVersion)
    {
        DomainOutOfRangeException.ThrowIfLessThan(aggregateVersion, 0);
        AggregateVersion = aggregateVersion;
    }
}
```

### Entity<T> / Entity

```csharp
using <RootNamespace>.Exceptions;

namespace <RootNamespace>.Contracts;

public abstract class Entity<T> : Validatable
{
    private readonly List<Event<T>> _events = [];

    public T Id { get; private set; }

    public DateTimeOffset CreatedAt { get; protected set; }

    public DateTimeOffset UpdatedAt { get; protected set; }

    public IReadOnlyCollection<Event<T>> Events => _events;

    public long Version { get; protected set; }

    protected Entity()
    {
        Id = GeneratedId();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    protected abstract T GeneratedId();

    protected void UpVersion() => Version += 1;

    public void ClearEvents() => _events.Clear();

    protected void ApplyEvent(Event<T> @event)
    {
        ((dynamic)this).Apply((dynamic)@event);
        Version = @event.AggregateVersion;
        Id = @event.AggregateId!;
    }

    protected void AddEvent<TEvent>(TEvent @event) where TEvent : Event<T>
    {
        UpVersion();
        @event.SetAggregateVersion(Version);
        @event.SetAggregateId(Id);
        _events.Add(@event);
        ApplyEvent(@event);
    }

    public override bool Equals(object? obj)
    {
        var compareTo = obj as Entity<T>;

        if (ReferenceEquals(this, compareTo)) return true;
        if (compareTo is null) return false;

        return Id!.Equals(compareTo.Id);
    }

    public override int GetHashCode()
    {
        return (GetType().GetHashCode() * 907) + Id!.GetHashCode();
    }

    protected override void Validate()
    {
        ClearErrors();
        ValidateRules();
        if (IsInvalid())
        {
            throw new DomainRuleException(Errors);
        }
    }

    protected abstract void ValidateRules();
}

public abstract class Entity : Entity<Guid>
{
    protected override Guid GeneratedId() => Guid.NewGuid();
}
```

## Exceções de domínio

As exceções abaixo são **referência**. Crie-as apenas se o projeto ainda não tiver equivalentes. **Não crie exceções sem motivo**: reutilize as existentes quando a semântica for a mesma. Crie novas exceções somente quando o tipo de falha for semanticamente distinto e útil para o domínio.

Toda exceção de domínio herda de `DomainException`, que herda de `Exception`. Isso permite capturar qualquer falha do domínio em um único tipo base. Nunca herde de exceções de framework (ex.: `ArgumentNullException`).

### DomainException

```csharp
namespace <RootNamespace>.Exceptions;

public class DomainException : Exception
{
    public DomainException() { }
    public DomainException(string? message) : base(message) { }
}
```

### Referências base

```csharp
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace <RootNamespace>.Exceptions;

public class DomainNullException : DomainException
{
    public DomainNullException(string? message) : base(message)
    { }

    public static void ThrowIfNull([NotNull] object? argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null)
        {
            Throw(paramName);
        }
    }

    [DoesNotReturn]
    internal static void Throw(string? paramName) =>
        throw new DomainNullException(paramName);
}
```

```csharp
using System.Diagnostics.CodeAnalysis;
using <RootNamespace>.Contracts;

namespace <RootNamespace>.Exceptions;

public class DomainRuleException : DomainException
{
    public IEnumerable<Error> Errors { get; }

    public DomainRuleException(IEnumerable<Error> errors) => Errors = errors;

    public static void ThrowIfAny(IEnumerable<Error> errors)
    {
        if (errors.Any())
        {
            Throw(errors);
        }
    }

    [DoesNotReturn]
    internal static void Throw(IEnumerable<Error> errors) =>
        throw new DomainRuleException(errors);
}
```

```csharp
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace <RootNamespace>.Exceptions;

public class DomainOutOfRangeException : DomainException
{
    public DomainOutOfRangeException(string? message) : base(message)
    { }

    public static void ThrowIfLessThan<T>(T value, T other,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(other) < 0)
            Throw(value, other, paramName);
    }

    [DoesNotReturn]
    private static void Throw<T>(T value, T other, string? paramName) =>
        throw new DomainOutOfRangeException($"{paramName} must be greater than or equal to {other}.");
}
```

### Quando criar uma nova exceção de domínio

- A falha tem significado de negócio específico (ex.: conflito de estado, violação de política, limite de capacidade) e não é bem representada por `DomainRuleException`/`DomainNullException`/`DomainOutOfRangeException`.
- O consumidor do domínio se beneficia em distinguir o tipo da falha.
- Herde de `DomainException`, nunca diretamente de `Exception` ou de uma exceção de framework.
- Mantenha o padrão: nome claro em inglês, sufixo `Exception`, construtor/hélperes estáticos apenas quando úteis.

## Como criar uma Entidade

1. **Modelar estado e invariantes**: propriedades, status/ciclo de vida, regras.
2. **Enums**: crie enums quando representarem um conjunto fechado de valores de negócio (status, tipo, categoria, modalidade etc.). Nome e membros em inglês; estilo UPPER_CASE ou PascalCase conforme convenção do projeto existente. Não limite a status/ciclo de vida - use enums onde fizer sentido.
3. **Comandos**: interfaces em `Commands/<Entities>/I<Action><Entity>.cs` com os inputs necessários ao comportamento.
4. **Eventos**: um por mutação relevante em `Events/<Entities>/`. Nome no passado; `Key` estável em UPPER_SNAKE_CASE. Capture apenas dados relevantes.
5. **Entidade**: implemente comportamentos, validação e `Apply`.
6. **Testes de domínio**: cubra caminho feliz, invariantes, transições e eventos emitidos.

### Template de Entidade

```csharp
using <RootNamespace>.Commands.<Entities>;
using <RootNamespace>.Contracts;
using <RootNamespace>.Enums;
using <RootNamespace>.Events.<Entities>;
using <RootNamespace>.ValueObjects;

namespace <RootNamespace>.Entities;

public class <Entity> : Entity
{
    public <Type>? <Property> { get; private set; }
    public <EntityStatusEnum> Status { get; private set; }

    protected <Entity>() : base() { }

    protected override string ErrorCode() => nameof(<Entity>).ToUpperInvariant();

    protected override void ValidateRules()
    {
        if (/* invariant violated */)
        {
            AddError("<Business rule message>");
        }

        Validate(<ChildValidatable>);
    }

    public void <Behavior>(I<Behavior><Entity> command)
    {
        <Property> = command.<Property>;
        UpdatedAt = DateTimeOffset.UtcNow;
        // Set Status or other state when the behavior requires it
        Validate();
        AddEvent(new <Entity><BehaviorPast>(this));
    }

    public void Apply(<Entity><BehaviorPast> @event)
    {
        <Property> = @event.<Property>;
        UpdatedAt = @event.UpdatedAt;
        // Restore CreatedAt/Status/other state as needed for rehydration
    }
}
```

### Regras para Entidades

- **Comportamentos são definidos pelo negócio**. `Register`, `Update`, `Activate`, `Inactivate` são apenas exemplos comuns; crie métodos com semântica adequada ao domínio (ex.: `Approve`, `Cancel`, `Assign`, `Suspend`, `Complete`), mantendo o mesmo padrão.
- Mutação de estado **somente** por métodos de comportamento.
- Métodos de comportamento: atribuem estado, atualizam `UpdatedAt`, chamam `Validate()` quando há invariantes a garantir e chamam `AddEvent`.
- `Apply` reconstitui estado a partir do evento; não deve conter regra de negócio nova nem gerar eventos. Deve restaurar todo estado necessário para reidratação correta.
- Propriedades com `private set`; coleções expostas como somente leitura.
- Ctor protegido sem parâmetros para ORM/materialização.
- Invariantes em `ValidateRules()`; use `AddError` para regras simples e `Validate(child)` para VOs/objetos validáveis filhos.

## Como criar um Value Object

```csharp
using <RootNamespace>.Contracts;
using <RootNamespace>.Exceptions;

namespace <RootNamespace>.ValueObjects;

public class <Name> : ValueObject<<Name>>
{
    public <Type> Value { get; }

    public <Name>(<Type> value)
    {
        Value = value;
        Validate();
    }

    protected override string ErrorCode() => "INVALID_<NAME>";

    protected override void Validate()
    {
        ClearErrors();

        if (/* invalid */)
        {
            AddError("<Validation message>");
        }
    }

    public override string ToString() => Value.ToString()!;

    public static bool TryParse(string value, out <Name> result)
    {
        DomainNullException.ThrowIfNull(value);
        result = new <Name>(/* parse */);
        return result.IsValid();
    }

    public override bool Equals(<Name>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value.Equals(other.Value);
    }

    public override bool Equals(object? obj) => Equals(obj as <Name>);

    public override int GetHashCode() =>
        (GetType().GetHashCode() * 907) + Value.GetHashCode();
}
```

### Regras para Value Objects

- Imutável: propriedades somente `get`.
- Validação no construtor; `Validate()` preenche erros.
- Proteja o construtor contra inputs que causem exceção inesperada antes da validação (ex.: coleção vazia, formato nulo).
- Igualdade por valor; implementar `Equals`, `GetHashCode`, `ToString`.
- `TryParse` quando houver conversão de string. Implicit operator somente se for seguro e não mascarar estado inválido.
- Não exponha instância inválida como se fosse válida.

## Como criar um Evento

```csharp
using <RootNamespace>.Contracts;
using <RootNamespace>.Entities;

namespace <RootNamespace>.Events.<Entities>;

public class <Entity><BehaviorPast>(<Entity> entity) : Event<Guid>(entity)
{
    public override string Key => "<ENTITY>_HAS_BEEN_<BEHAVIOR_PAST>";

    public <Type> <Property> { get; } = entity.<Property>;
}
```

- Nome: passado (`Created`, `Updated`, `Activated`, `Approved` etc.), alinhado ao comportamento.
- `Key`: identificador estável, UPPER_SNAKE_CASE.
- Carregar apenas dados relevantes (snapshot do que mudou/estado necessário).
- Preferir tipos simples/serializáveis; para VOs, avaliar expor valor primitivo quando adequado.

## Como criar um Comando (contrato de domínio)

```csharp
namespace <RootNamespace>.Commands.<Entities>;

public interface I<Behavior><Entity>
{
    <Type> <Property> { get; }
}
```

- Interface pequena, por comportamento/caso de uso.
- Define o input que a entidade precisa; evita dependência de DTOs de Application/API.

## Enums

- Use enums para conjuntos fechados e estáveis do domínio: status, tipo, categoria, prioridade, modalidade, canal, etc.
- Nomeie em inglês; siga a convenção do projeto existente para estilo de membros.
- Não crie enum sem significado de negócio claro; prefira VO quando houver comportamento/validação.

## Checklist

- [ ] Segui a convenção de nomenclatura do projeto (ou canônica, se bootstrap).
- [ ] Código em inglês; sem XML doc/comentários desnecessários.
- [ ] Propriedades com setter privado/protegido; estado muda por método de comportamento.
- [ ] Invariantes validadas; falha lança `DomainRuleException`.
- [ ] Evento criado e disparado por mutação relevante.
- [ ] `Apply` correspondente ao evento restaura estado necessário.
- [ ] `ErrorCode()` consistente.
- [ ] Exceções novas criadas apenas quando semanticamente necessárias e herdando de `DomainException`.
- [ ] Testes unitários de domínio cobrindo sucesso, falha de regra e evento/estado.
