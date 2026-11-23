# Tasks

## 1. Domain — Entidades e Regras de Negócio

- [x] 1.1 Implementar o agregado `Customer` (Id, CreditLimit) no projeto Lab.Orders.Domain e verificar que o projeto compila.
- [x] 1.2 Implementar a entidade `OrderItem` (UnitPrice, Quantity, DiscountPercent, LineTotal) no projeto Lab.Orders.Domain e verificar que o projeto compila.
- [x] 1.3 Implementar o agregado `Order` (CustomerId, Items, Status, CreatedAt, Total; AddItem; Validate; Total com cálculo de desconto por item) no projeto Lab.Orders.Domain e verificar que o projeto compila.
- [x] 1.4 Escrever suíte unitária de Domínio cobrindo os cenários das specs: criação válida, sem items, sem customer, quantidade zero, unitPrice não positivo, DiscountPercent fora de 0–100, LineTotal com desconto e Total como soma das linhas. Verificar `dotnet test` aprovado.

## 2. Application — Caso de Uso e Contratos

- [x] 2.1 Implementar `ICustomersRepository`, `IOrdersRepository` e o use case `CreateOrderUseCase` + `CreateOrderCommand` no projeto Lab.Orders.Application, e verificar que o projeto compila.
- [x] 2.2 Implementar `CreateOrderUseCase.ExecuteAsync` (carregar Customer, criar Order, `Validate()`, verificar limite de crédito → `OrderCreditLimitExceededException`, persistir). Verificar que o projeto compila.
- [x] 2.3 Escrever suíte unitária de Application: Customer com limite suficiente → pedido retornado; limite excedido → exceção; Order inválido → exceção propagada. Verificar `dotnet test` aprovado.

## 3. Infrastructure — Implementação dos Repositórios

- [x] 3.1 Implementar o projeto Lab.Orders.Infrastructure com as implementações concretas dos repositórios e verificar que o projeto compila.

## 4. API — Endpoint de Criação

- [x] 4.1 Implementar o projeto Lab.Orders.Api com `OrdersController` (POST /orders), mapeamento de DTO → command e retorno 201 com OrderDto; mapear `OrderCreditLimitExceededException` → 409 e exceções de validação → 400. Verificar que o projeto compila.

## 5. Verificação Final

- [x] 5.1 Executar `dotnet build --no-restore` no repositório e verificar que a solução builda sem erros.
- [x] 5.2 Executar `dotnet test --no-build --verbosity minimal` no repositório e verificar que todos os testes passam.
