# Proposal

## Why

A solução `Lab.Orders` está hoje apenas esqueletada (quatro projetos de domínio, aplicação, infraestrutura e API, sem regras implementadas), e as regras do sistema de pedidos vivem apenas em conversa. Sem especificações, a política de descontos e da taxa de entrega corre o risco de ser interpretada de forma diferente por cada camada e por cada agente do pipeline, gerando implementações divergentes e testes que não protegem as invariantes. Este MVP estabelece um contrato único e testável para essas regras antes de qualquer código de produção.

## What Changes

- Cadastro de cliente com nome, telefone, documento (CPF ou CNPJ) e endereço de entrega.
- Catálogo independente de produtos com SKU único, descrição, preço, desconto unitário opcional e preço final calculado.
- Registro de pedido composto por cliente, um ou mais itens (SKU do catálogo com quantidade inteira >= 1), forma de pagamento, distância e totais calculados. Pedido sem itens é recusado.
- Desconto unitário e desconto de pedido podem ser percentual **ou** valor monetário, nunca ambos; se as duas modalidades forem informadas, o pedido é recusado.
- Desconto de pedido e desconto de produto são mutuamente exclusivos: havendo qualquer produto com desconto, aplicam-se apenas os descontos individuais.
- Descontos negativos são recusados; o total calculado nunca pode ser menor que zero.
- Cálculo do total: `total = subtotal - descontoPedido + taxaEntrega`, com o desconto percentual do pedido incidindo sobre o subtotal e nunca sobre a taxa de entrega.
- Taxa de entrega por distância: R$ 12,00 para até 15 km; R$ 12,00 + R$ 1,00 por km adicional entre 15 e 100 km; pedido recusado acima de 100 km.
- Fora de escopo: integração com serviços de mapas, estoque, acompanhamento/status de pedido, cancelamento, pagamento real e múltiplos idiomas.

## Capabilities

### New Capabilities

- `customer-registration`: cadastro e validação dos dados do cliente, incluindo documento (CPF/CNPJ) e endereço de entrega.
- `product-catalog`: produtos identificados por SKU com preço, desconto unitário opcional e preço final calculado.
- `pricing`: regras compartilhadas de cálculo de preço final, subtotal e total do pedido.
- `discount-policy`: modalidades de desconto, não negatividade, exclusividade produto/pedido e comportamento quando o desconto excede a base.
- `delivery-fee`: cálculo da taxa por distância, faixas de quilometragem e rejeição de distâncias acima do limite.
- `order-registration`: composição e recusa de pedidos, incluindo itens, quantidade, forma de pagamento e distância.

### Modified Capabilities

Nenhuma. O projeto ainda não possui specs aprovadas.

## Impact

- `src/Lab.Orders.Domain`: entidades e value objects para cliente, produto, item de pedido, dinheiro/desconto e distância, com as invariantes acima.
- `src/Lab.Orders.Application`: casos de uso de cadastro e registro, interfaces de repositório e regras de aplicação das políticas de desconto e entrega.
- `src/Lab.Orders.Infrastructure`: persistência dos agregados e implementação dos repositórios.
- `src/Lab.Orders.Api`: endpoints de cadastro de cliente/produto e registro de pedido.
- Projetos de teste correspondentes (domínio, aplicação, infraestrutura e funcional API), usando xUnit, FluentAssertions, NSubstitute e AutoFixture.
- Nenhuma dependência externa de mapas ou serviços de pagamento no MVP.

## Suposições registradas

Estes pontos foram sinalizados na exploração e precisam de confirmação humana; as specs abaixo adotam explicitamente uma das alternativas para não ficarem incompletas:

- **Desconto que excede a base**: adotado **recusar** a operação, em vez de truncar em zero, seguindo o padrão já confirmado de recusar entradas inválidas (distância > 100 km e modalidades duplas).
- **Arredondamento**: duas casas decimais para valores monetários e `MidpointRounding.AwayFromZero`, inclusive no desconto percentual e no km fracionário.
- **Origem da distância**: informada no próprio pedido, sem consulta a mapas.
- **Forma de pagamento**: enum fechado com `PIX`, `Credito`, `Debito` e `Dinheiro`, sem regras próprias de taxa neste MVP; permanece como value object de `order-registration`.
- **Modalidade dupla no catálogo**: a validação `%` xor `R$` também é aplicada no cadastro do produto; a recusa no pedido, que foi confirmada explicitamente, continua valendo para o desconto do pedido.
- **Documento do cliente**: assume-se validação de formato de CPF e CNPJ, incluindo dígitos verificadores, e não apenas a distinção entre os dois tipos.
- **Endereço de entrega**: assume-se estrutura com CEP, logradouro, número, bairro, cidade e unidade federativa, com complemento opcional.
