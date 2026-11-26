# Tasks

## 1. Fundação do domínio (value objects)

- [ ] 1.1 Criar `Money` e `Percentage` em `Lab.Orders.Domain` com arredondamento de duas casas `MidpointRounding.AwayFromZero` e faixa de 0 a 100 para percentual; verificar com testes unitários parametrizados em `Lab.Orders.Domain.UnitTests`.
- [ ] 1.2 Criar `Desconto` como value object discriminado (`Percentual` / `Monetario`) que recusa modalidade dupla, valor negativo e valor acima da base; verificar com testes unitários dos cenários de `discount-policy`.

## 2. Cliente e catálogo (domínio)

- [ ] 2.1 Criar `Documento` (CPF/CNPJ) com validação de formato e dígitos verificadores, `Endereco` e a entidade `Cliente`; verificar com testes unitários dos cenários de `customer-registration`.
- [ ] 2.2 Criar `Sku`, a entidade `Produto` e o cálculo do preço final com desconto unitário opcional; verificar com testes unitários dos cenários de `product-catalog`.

## 3. Políticas de negócio (domínio)

- [ ] 3.1 Implementar o cálculo de preço final, subtotal por quantidade e `total = subtotal - descontoPedido + taxaEntrega`, recusando desconto que exceda a base; verificar com testes unitários usando os valores dos cenários de `pricing`.
- [ ] 3.2 Implementar a regra de exclusividade entre desconto de pedido e desconto de produto, incluindo o uso de descontos individuais quando qualquer produto tiver desconto; verificar com testes unitários dos três cenários de `discount-policy`.
- [ ] 3.3 Implementar a política `TaxaEntrega` por faixa de distância (R$ 12,00 até 15 km, + R$ 1,00 por km até 100 km, recusa acima de 100 km e distância negativa); verificar com testes unitários dos cenários de `delivery-fee`.

## 4. Casos de uso e contratos (aplicação)

- [ ] 4.1 Definir `IClienteRepository`, `IProdutoRepository` e `IPedidoRepository` em `Lab.Orders.Application` e os DTOs de entrada; verificar compilação com `dotnet build --no-restore`.
- [ ] 4.2 Implementar o caso de uso de cadastro de cliente; verificar com testes unitários de aplicação usando NSubstitute para o repositório.
- [ ] 4.3 Implementar o caso de uso de cadastro de produto com checagem de SKU duplicado; verificar com testes unitários de aplicação cobrindo o SKU já existente.
- [ ] 4.4 Implementar o caso de uso de registro de pedido, resolvendo cliente e SKUs, aplicando desconto, entrega e total, e gravando snapshot do preço final no item; verificar com testes unitários cobrindo pedido sem itens, SKU inexistente, quantidade zero e distância acima de 100 km.

## 5. Persistência (infraestrutura)

- [ ] 5.1 Implementar os repositórios em memória em `Lab.Orders.Infrastructure` com chave por identificador e SKU; verificar com testes de integração em `Lab.Orders.Infrastructure.IntegrationTests` cobrindo unicidade de SKU e recuperação de cliente, produto e pedido.

## 6. Endpoints HTTP (API)

- [ ] 6.1 Ajustar `Lab.Orders.Api.csproj` para `Microsoft.NET.Sdk.Web`, registrar os repositórios no container de DI e configurar respostas de erro com `ProblemDetails`; verificar com `dotnet build --no-restore`.
- [ ] 6.2 Expor endpoints de cadastro de cliente, cadastro de produto e registro de pedido, mapeando falhas de domínio para HTTP 400; verificar com testes funcionais em `Lab.Orders.Api.FunctionalTests` cobrindo uma criação bem-sucedida e uma recusa por regra de negócio.

## 7. Validação final

- [ ] 7.1 Rodar `dotnet build --no-restore` e `dotnet test --no-build --verbosity minimal`; verificar compilação e suíte completa sem falhas.
- [ ] 7.2 Rodar `openspec validate order-management-mvp`; verificar que a change permanece consistente e que todos os artefatos de planejamento existem.
