# Design

## Context

A solucao `Lab.Orders` e atualmente um scaffold vazio: apenas `Class1.cs` no projeto de dominio e nenhum teste escrito. Nao existe spec anterior e nao ha persistencia, API ou integracoes a preservar. Veja `proposal.md` para a motivacao e os seis specs em `specs/` para o comportamento esperado. As decisoes abaixo fecham as ambiguidades do prompt original antes da implementacao.

## Goals / Non-Goals

**Goals:**

- Tornar as regras de desconto e de taxa de entrega determinísticas e verificaveis por teste.
- Manter o dominio independente de framework, persistencia e transporte HTTP.
- Dar a cada regra de negocio um unico dono entre as capabilities, evitando duplicacao de validacao.

**Non-Goals:**

- Resolver calculo de distancia real, rotas ou geocodificacao.
- Validar cartao de credito/debito ou integrar provedor de pagamento.
- Definir estoque, impostos, multas por atraso, historico de pedidos ou acompanhamento de entrega.
- Migrar dados: nao existe base anterior.

## Decisions

### 1. Fronteiras entre capabilities

Seis capabilities com responsabilidades exclusivas:

```
  +--------------------------------------------------------+
  |                    order-registration                   |
  |  agregacao, pagamento, validacoes de entrada, total     |
  +--+-----------+-----------+-----------------------------+
     |           |           |
     v           v           v
+---------+  +---------+  +---------+
| client- |  | order-  |  |delivery-|
| regist. |  | item-   |  | fee     |
|         |  | pricing |  |         |
+---------+  +----+----+  +---------+
                  |
       +----------+----------+
       v                     v
+--------------+    +------------------+
| product-     |    | discount-policy  |
| catalog      |    | (transversal)    |
+--------------+    +------------------+
```

`discount-policy` e transversal: e consultada por `order-item-pricing` (desconto unitario) e por `order-registration` (desconto do pedido). `delivery-fee` nao consulta `discount-policy`: a taxa nunca recebe desconto.

**Alternativa considerada:** um unico modulo `order-management`. Descartada porque misturaria regras de negocio com vidas diferentes (catalogo, documento, tarifa) e tornaria os testes de fronteira impossiveis de isolar.

### 2. Composicao do total

Adotada a opcao (c):

```
subtotal = SUM(preco_final_item)
parcela  = max(0, subtotal - desconto_do_pedido)
total    = parcela + taxa_de_entrega
```

O desconto incide somente sobre produtos, nunca sobre a taxa. A recusa por total negativo dispara quando `desconto > subtotal`, antes de somar a taxa.

**Alternativas consideradas:** (a) `subtotal - desconto + taxa` sem piso explicito — deixava a regra de negativo ambigua; (b) `(subtotal + taxa) - desconto` — permitia que o desconto comesse a entrega. A opcao (c) torna o piso explicito e preserva a taxa integralmente.

### 3. Desconto como conceito unico

Um desconto e um valor com uma unica modalidade: valor monetario ou percentual. As duas modalidades preenchidas ao mesmo tempo recusam o pedido, tanto no item quanto no pedido (opcao B). Valores negativos sao recusados em ambos os niveis. Ha exclusividade: se qualquer item possui desconto unitario, o pedido nao pode ter desconto proprio.

**Alternativa considerada:** modalidade unica apenas no pedido, deixando o item livre. Descartada porque criaria dois modelos de desconto e um caso especial dificil de testar.

### 4. Arredondamento e precisao

- Valores monetarios: duas casas decimais (centavos), com arredondamento `half-up`.
- Distancia: arredondamento `half-up` no primeiro decimal, antes de qualquer regra de faixa ou limite. `15,2 -> 15`, `15,5 -> 16`.
- Teto de 100 km avaliado apos o arredondamento: `100,4 -> 100` (aceito), `100,5 -> 101` (recusado).
- O desconto percentual e aplicado ao preco base unitario do item ou ao subtotal do pedido, conforme o nivel, e o resultado e arredondado uma unica vez ao centavo.

**Alternativa considerada:** manter a distancia fracionaria no calculo da taxa. Descartada porque o prompt define cobranca por quilometro adicional inteiro.

### 5. Produto no catalogo x item do pedido

O catalogo possui SKU, descricao e preco base. O item do pedido referencia o SKU e guarda quantidade e desconto unitario; o preco final e calculado, nao persistido como entrada. Isso evita que um desconto de um pedido altere o catalogo.

**Alternativa considerada:** produto como parte do pedido, sem catalogo. Descartada porque o prompt confirmou que havera catalogo.

### 6. Documento e forma de pagamento

CPF e CNPJ sao validados quanto a formato e digitos verificadores antes do cadastro. A entrada e normalizada para digitos antes da validacao, de modo que mascara nao altere o resultado. Forma de pagamento e um conjunto fechado: dinheiro, pix, credito, debito. Cartao nao exige dados nem validacao neste MVP.

**Alternativa considerada:** documento como texto livre. Descartada porque o prompt exigiu validacao.

### 7. Organizacao por camadas

Dominio puro com agregados e value objects; Application com casos de uso e validacoes de negocio; Infrastructure com persistencia; Api com endpoints.Os testes acompanham: unitarios de dominio e aplicacao, integracao de persistencia e funcionais HTTP.

**Alternativa considerada:** regras de calculo na camada de API. Descartada porque impediria testar as invariantes sem HTTP.

## Risks / Trade-offs

- [Arredondamento em cascata pode gerar centavos diferentes conforme a ordem das operacoes] → Arredondar uma unica vez ao centavo, no calculo de preco final do item e no total do pedido, e cobrir os casos com testes de fronteira.
- [A mesma regra de desconto pode ser implementada duas vezes e divergir] → `discount-policy` e o unico dono da regra; item e pedido apenas a consomem.
- [Distancia informada manualmente pode nao refletir a entrega real] → Aceito no MVP; a ausencia de integracao com mapas e explicitamente declarada no proposal.
- [Mascara de CPF/CNPJ pode variar entre clients] → Normalizar para digitos antes de validar, mantendo uma unica regra de validacao.
- [Specs com cenarios sobrepostos entre `discount-policy` e `order-registration`] → Manter a regra em `discount-policy` e usar `order-registration` apenas para a integracao no calculo do total.

## Migration Plan

Nao ha dados ou codigo anterior a preservar. A implementacao comeca a partir do scaffold atual. Rollback consiste em nao aplicar a change; nao ha migracao de base nem contrato externo a reverter.
