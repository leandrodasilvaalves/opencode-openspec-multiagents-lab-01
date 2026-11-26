# Tasks

> Re-derivado do `design.md` aprovado. **Artefato em portugues; identificadores/filtros em ingles (convencoes de codigo).**
> **Nada sobrescrito em silencio:** tasks do `@product-manager` preservadas (texto intacto em pt-BR); apenas **identificadores renomeados** `Cliente->Customer`, `Produto->Product`, `Pedido->Order`, `ItemDePedido->OrderItem`, `Desconto->Discount`, `Distancia->Distance`, `TaxaDeEntrega->DeliveryFee`, `Pagamento->PaymentMethod`, `Documento->Document`, `EnderecoDeEntrega->Address`, `Cadastrar*->Register*`; **tecnicas adicionadas**; renumeradas. Build `dotnet build --no-restore`; teste `dotnet test --no-build --verbosity minimal`. **Regra de Ouro.** Nenhum teste ignorado.
> Filtros usam nomes de classe de teste em ingles; **evitar `FullyQualifiedName~Order`** (colide com namespace `Lab.Orders`); preferir `FullyQualifiedName~OrderTests`. (Validar nomes reais na aplicacao.)

## 0. Fundacao tecnica (infra/ops; ADR-016/017/018)

- [x] 0.1 Adicionar **`Directory.Packages.props`** (CPM) + **`Directory.Build.props`**; pacotes em ingles; versoes fixas (`dotnet package search`); nao fixar versao de provider MySQL aqui sem verificar; verificar com `dotnet build --no-restore`
- [x] 0.2 Referenciar **`Microsoft.EntityFrameworkCore`** 10 + **provider MySQL** (Oracle `MySql.EntityFrameworkCore` rec. **licenca GPL-2.0-only + Universal-FOSS-exception; revisao legal** / `Microting.EntityFrameworkCore.MySql` alt MIT renomeado de Pomelo) + `Microsoft.EntityFrameworkCore.Design` + `.Tools`; versao exata verificar; verificar com `dotnet build --no-restore`
- [x] 0.3 `Microsoft.EntityFrameworkCore.InMemory` no `Lab.Orders.Infrastructure.IntegrationTests`; **AutoFixture** nos testes (AGENTS.md); verificar com `dotnet build --no-restore`
- [x] 0.4 **`Microsoft.NET.Sdk.Web`** na API + `Program.cs` + `appsettings.json`/`appsettings.Development.json` + `launchSettings`; `AddProblemDetails` + global exception handler; `AddOpenApi`; `AddHealthChecks`; `CultureInfo.InvariantCulture`; `AddApplication`/`AddInfrastructure`; **app sobe sem excecao de resolucao DI**; verificar com `dotnet build --no-restore`
- [x] 0.5 Remover `Class1.cs`; **sem comentarios; sem XML doc**; verificar com `dotnet build --no-restore`

## 1. Fundacao do dominio (`Money`, `Percentage`, precision; ADR-002/003)

- [x] 1.1 Criar estrutura de pastas do dominio (`Customers`, `Products`, `Orders`, `Discounts`, `Delivery`, `Shared`/`ValueObjects`); verificar com `dotnet build --no-restore`
- [x] 1.2 Implementar `Money` (`decimal`, 2 casas, `MidpointRounding.AwayFromZero`, `InvariantCulture`, **sem `double/float`**, nao negativo quando aplicavel) com testes de fronteira; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~MoneyTests`
- [x] 1.3 Implementar `Percentage` (`decimal`, nao negativo; **>100 recusado** quando aplicado a base; conversao arredondada **1x** ao centavo) com testes; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~PercentageTests`

## 2. Cadastro de cliente (`client-registration`; ADR-007/011/012)

- [x] 2.1 Implementar `Document` com validacao formato + DV **CPF(11)/CNPJ(14) somente digitos numericos**, normalizacao p/ digitos; testes CPF valido, CNPJ valido, DV invalido, formato desconhecido, **alfanumerico recusado**; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~DocumentTests`
- [x] 2.2 Implementar `Address` (`Street`,`Number`,`Complement?`,`Neighborhood`,`City`,`State`,`PostalCode`; required nao-vazio; `State`=UF; `PostalCode`=CEP normalizado); testes; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~AddressTests`
- [x] 2.3 Implementar `Customer` (`CustomerId` `Guid`, `Name`, `Phone`, `Document`, `Address` obrigatorios); testes cadastro completo + cada campo ausente + subcampo de `Address` ausente; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~CustomerTests`
- [x] 2.4 Implementar `RegisterCustomer` + `ICustomerRepository` (contrato Application); testes sucesso + recusa `Document` invalido; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~Application`
  - _(PM original 2.x preservada; ids renomeados; `Address` estruturado adicionado.)_

## 3. Catalogo de produtos (`product-catalog`; ADR-009/012)

- [x] 3.1 Implementar `Product` (`ProductId` `Guid`, `Sku`, `Description`, `BasePrice` `Money` >=0 obrigatorios); testes completo + cada campo ausente + preco negativo + preco zero aceito; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~ProductTests`
- [x] 3.2 Implementar `RegisterProduct` + `IProductRepository`; testes sucesso + recusas preco negativo + campos obrigatorios; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~Application`
  - _(PM original 3.x preservada; ids renomeados.)_

## 4. Politica de descontos (`discount-policy`; ADR-004)

- [x] 4.1 Implementar `Discount` + `DiscountType { Amount, Percentage }` (1 modalidade; nunca ambos; nunca negativo; `null` = sem); testes valor/percentual/ambos/negativos; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~DiscountTests`
- [x] 4.2 Regra de exclusividade `OrderDiscount` vs `OrderItem.Discount`; testes 3 estados; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~DiscountTests`
- [x] 4.3 Calculo do desconto sobre subtotal; piso zero; **recusa `OrderDiscount > Subtotal`**; testes menor/igual/maior; percentual >100 recusa; nao incide em taxa; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~DiscountTests`
  - _(PM original 4.x preservada; ids renomeados.)_

## 5. Taxa de entrega (`delivery-fee`; ADR-005)

- [x] 5.1 `Distance` (km `decimal`, obrigatoria, nao negativa, `InvariantCulture`); **half-up 1o decimal -> km inteiro antes**; testes 15,2/15,4/15,5/15,9 e 100,4/100,5; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~DistanceTests`
- [x] 5.2 `DeliveryFee` (R$12,00 ate 15km; +R$1,00/km inteiro; **>100km recusa**; 100 aceito R$97,00; sem mapas); testes 10/15/16/20/100/101 + rounding; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~DeliveryFeeTests`
  - _(PM original 5.x preservada; ids renomeados.)_

## 6. Itens de pedido (`order-item-pricing`; ADR-003/004/009)

- [x] 6.1 `OrderItem` (`Sku`, `Quantity` int >0, `Discount` opcional; `Sku` deve existir no catalogo); testes `Sku` inexistente, qtd 0, qtd negativa, ambos `DiscountType`, negativos; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~OrderItemTests`
- [x] 6.2 Preco final `(BasePrice - Discount) * Quantity`, arredondado 1x, **piso zero**; `Discount == BasePrice` -> 0 aceito; **`Discount > BasePrice` recusa**; percentual >100 recusa; testes; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~OrderItemTests`
  - _(PM original 6.x preservada; ids renomeados; reconciliacao piso-zero vs recusa explicita.)_

## 7. Registro de pedido (`order-registration`; ADR-006/008/013)

- [x] 7.1 `Order` agregado (`OrderId` `Guid`, `CustomerId`, `OrderItem`s, `PaymentMethod`, `Discount`, `DistanceKm`, `DeliveryFee`, `Total`, snapshots); recusa sem `Customer`, `Customer` inexistente, sem itens, sem `PaymentMethod`, `PaymentMethod` invalido; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~OrderTests`
- [x] 7.2 `PaymentMethod { Cash, Pix, CreditCard, DebitCard }`; sem validacao de cartao; testes 4 opcoes + desconhecido + ausente; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~PaymentMethodTests`
- [x] 7.3 Total `max(0, Subtotal - OrderDiscount) + DeliveryFee`; testes sem desconto / valor / percentual / ==subtotal / >subtotal / exclusividade com item; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~OrderTests`
- [x] 7.4 `RegisterOrder` integrando `Customer`, `OrderItem`, `Discount`, `Distance` + `ICustomerRepository`/`IProductRepository`/`IOrderRepository`; testes valido + cada recusa; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~Application`
- [x] 7.5 **Idempotencia**: `IdempotencyKey` + `IIdempotencyStore` + `IdempotencyRecord`; mesma chave + mesmo payload -> mesmo resultado; nao duplica; chave nova -> novo; testes; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~Application`
  - _(PM original 7.x preservada; ids renomeados; 7.5 adicionada — `order-registration` idempotente.)_

## 8. Persistencia (`Infrastructure`; ADR-010/012/013)

- [x] 8.1 `OrdersDbContext`; **value objects** owned/conversoes; `numeric(18,2)`; `Guid` PK; `Product.Sku` unique; FK `Order.CustomerId`; `IdempotencyRecord` unique `Key`; **tabelas/colunas em ingles**; verificar com `dotnet build --no-restore`
- [x] 8.2 **Provider MySQL** + connection string segura (env/secret, **nao logar**); `AddDbContext`; verificar com `dotnet build --no-restore`
- [x] 8.3 **Migrations** `dotnet ef migrations add <EnglishName>` + `dotnet ef database update`; migrations em ingles; verificar com `dotnet build --no-restore`
- [x] 8.4 `ICustomerRepository`, `IProductRepository`, `IOrderRepository`, `IIdempotencyStore` impls; **testes de integracao in-memory** (`Microsoft.EntityFrameworkCore.InMemory`) gravacao + recuperacao de cada agregado; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~IntegrationTests`
- [x] 8.5 Registrar impls no DI (`AddInfrastructure`); app sobe sem excecao; verificar com `dotnet build --no-restore`
  - _(PM original 8.x preservada; tecnologia EF Core+MySQL; in-memory testes; idempotencia adicionada.)_

## 9. API (ADR-014/017)

- [x] 9.1 Endpoints **`POST /customers`** + **`POST /products`**; `ProblemDetails`; `400/422` (+`409` SKU); mensagens **ingles**; camelCase; invariant; testes funcionais HTTP sucesso + recusas `Document`/preco; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~FunctionalTests`
- [x] 9.2 **`POST /orders`** + **`Idempotency-Key`**; `ProblemDetails`; `404` cliente/produto; `422` regra; mensagens ingles; testes funcionais pedido valido + sem itens + desconto>subtotal + km>100 + ambos `DiscountType` + **reenvio idempotente**; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~FunctionalTests`
- [x] 9.3 Documentar endpoints + regras + exemplos no **README da API** (exemplos = contratos dos testes funcionais); verificar correspondencia; verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~FunctionalTests`
- [x] 9.4 **`Dockerfile`** (multi-stage; non-root; sem segredos) + **`docker-compose.yml`** (app + `mysql:8`; volumes; healthcheck; connection string env/secret); **sem CI**; verificar `docker compose up` + `/health`
  - _(PM original 9.x preservada; `Idempotency-Key`, `ProblemDetails`, mensagens ingles, Docker adicionados.)_

## 10. Resiliencia / observabilidade / seguranca

- [x] 10.1 `AddProblemDetails` + global exception handler; mapear `DomainException`->`422`, not found->`404`, invalid->`400`, conflict->`409`; **mensagens ingles**; verificar com `dotnet test --no-build --verbosity minimal`
- [x] 10.2 Logs estruturados **sem PII** (documento nao logado); correlation id; `/health`; metricas; sem segredos; verificar com `dotnet build --no-restore`
- [x] 10.3 (Opcional; so com ADR) `Microsoft.Extensions.Resilience` p/ transiente DB; verificar com `dotnet build --no-restore`

## 11. Verificacao integrada + OpenSpec

- [x] 11.1 `dotnet build --no-restore` + `dotnet test --no-build --verbosity minimal` na solution inteira; **zero falhas; nenhum teste ignorado**; **Regra de Ouro**; verificar
  - _(PM original 10.1 preservada.)_
- [x] 11.2 Revisar os cenarios dos seis specs + **idempotencia** + **`Address`**; **todo Scenario tem teste**; listar lacunas antes do archive; verificar
  - _(PM original 10.2 preservada + emendas.)_
- [x] 11.3 `openspec validate sistema-de-pedidos-mvp` + `openspec status --change sistema-de-pedidos-mvp`; consistente; artefatos **portugues + ids ingles**; **sem criar diretorios na mao**
