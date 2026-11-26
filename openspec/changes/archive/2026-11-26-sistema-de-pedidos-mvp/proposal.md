# Proposal

## Why

O laboratório precisa de um domínio de pedidos com regras de cálculo explicitas e testáveis. Hoje a solução `Lab.Orders` e apenas um scaffold vazio e nao existe nenhuma spec no projeto, entao qualquer implementacao comecaria com decisoes implicitas sobre desconto, faixas de entrega e composicao do total. Formalizar essas regras antes da implementacao evita ambiguidades no dominio.

## What Changes

- Cadastro de clientes com nome, telefone, documento (CPF ou CNPJ) validado e endereco de entrega.
- Catalogo de produtos com SKU, descricao e preco base.
- Itens de pedido com SKU do catalogo, quantidade, desconto unitario opcional e preco final calculado.
- Politica de descontos unica para itens e para o pedido: valor ou percentual, nunca ambos, nunca negativo, com exclusividade entre desconto de item e desconto de pedido.
- Taxa de entrega calculada a partir da distancia informada em quilometros, sem integracao com servicos de mapas no MVP.
- Registro do pedido agregando cliente, itens, forma de pagamento, desconto e taxa, com recusa de pedidos sem itens e recusa quando o desconto ultrapassa o subtotal.
- Fora de escopo: integracao com mapas, geocodificacao, validacao de cartao, cobranca efetiva, estoque, autenticacao, notificacoes e acompanhamento de entrega.

## Capabilities

### New Capabilities

- `client-registration`: cadastro de cliente e validacao do documento (CPF/CNPJ) e do endereco de entrega.
- `product-catalog`: manutencao do catalogo de produtos com SKU, descricao e preco base.
- `order-item-pricing`: composicao do item do pedido (SKU, quantidade, desconto unitario, preco final).
- `discount-policy`: modalidades de desconto, limites e exclusividade entre desconto de item e desconto de pedido.
- `delivery-fee`: conversao da distancia informada em taxa de entrega, com arredondamento e limite maximo.
- `order-registration`: agregacao do pedido, formas de pagamento aceitas, validacoes de entrada e calculo do total.

### Modified Capabilities

Nenhuma. O projeto nao possui specs existentes.

## Impact

- Domain: novos agregados, entidades e value objects para cliente, produto, item de pedido, desconto, distancia e pedido. **Codigo em ingles.**
- Application: casos de uso de cadastro de cliente, cadastro de produto e registro de pedido, com validacoes de negocio; contratos (`ICustomerRepository`/`IProductRepository`/`IOrderRepository`/`IIdempotencyStore`). **Criacao de pedido idempotente** (`IdempotencyKey`). **Codigo em ingles.**
- Infrastructure: persistencia dos agregados e implementacao dos repositorios; **EF Core + MySQL** + migrations + **Dockerfile/docker-compose**. **Codigo em ingles.**
- Api: endpoints para cadastro de cliente, cadastro de produto e registro de pedido; `ProblemDetails`; mensagens em ingles; camelCase. **Codigo em ingles.**
- Testes: unitarios de dominio e aplicacao, integracao de persistencia (in-memory) e funcionais HTTP; xUnit + FluentAssertions + NSubstitute + AutoFixture. **Codigo em ingles.**
- Dependencias externas: nenhuma para mapas ou provedor de pagamento no MVP; **sem mensageria**; sem CI por enquanto.

## Refinamento tecnico

Decisoes tecnicas, NFRs, contratos, modelo de dados, infra, resiliencia, seguranca, observabilidade e **ADRs** em `design.md`. Convencoes: **artefato em portugues; codigo em ingles; sem comentarios; ver `AGENTS.md`.**
