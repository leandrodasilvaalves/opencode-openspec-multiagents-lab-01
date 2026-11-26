# Tasks

## 1. Fundacao do dominio

- [ ] 1.1 Remover `Class1.cs` e criar a estrutura de pastas do dominio para cliente, produto, pedido, desconto e entrega, e verificar com `dotnet build --no-restore`
- [ ] 1.2 Implementar o value object de valor monetario com duas casas decimais e arredondamento half-up, cobrindo com testes unitarios os casos de fronteira, e verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~Domain`
- [ ] 1.3 Implementar o value object de percentual com conversao para valor sobre uma base informada, cobrindo com testes unitarios o arredondamento ao centavo, e verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~Domain`

## 2. Cadastro de cliente (`client-registration`)

- [ ] 2.1 Implementar o value object de documento com validacao de formato e digitos verificadores de CPF e CNPJ, apos normalizacao para digitos, cobrindo com testes unitarios CPF valido, CNPJ valido, digito invalido e formato desconhecido, e verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~Document`
- [ ] 2.2 Implementar a entidade de cliente com nome, telefone, documento e endereco de entrega obrigatorios, cobrindo com testes unitarios o cadastro completo e cada campo ausente, e verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~Cliente`
- [ ] 2.3 Implementar o caso de uso de cadastro de cliente com a interface de repositorio correspondente, cobrindo com testes unitarios o sucesso e a recusa por documento invalido, e verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~Application`

## 3. Catalogo de produtos (`product-catalog`)

- [ ] 3.1 Implementar a entidade de produto com SKU, descricao e preco base obrigatorios, cobrindo com testes unitarios o cadastro completo, cada campo ausente e preco negativo, e verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~Produto`
- [ ] 3.2 Implementar o caso de uso de cadastro de produto com a interface de repositorio correspondente, cobrindo com testes unitarios o sucesso e as recusas de preco negativo e campos obrigatorios, e verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~Application`

## 4. Politica de descontos (`discount-policy`)

- [ ] 4.1 Implementar o value object de desconto com modalidade exclusiva (valor ou percentual) e proibicao de valor negativo, cobrindo com testes unitarios valor, percentual, ambas as modalidades e negativos, e verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~Desconto`
- [ ] 4.2 Implementar a regra de exclusividade entre desconto do pedido e desconto de itens, cobrindo com testes unitarios os tres estados (sem desconto em itens, com desconto em itens, confronto entre os dois), e verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~Desconto`
- [ ] 4.3 Implementar o calculo do desconto sobre o subtotal com piso zero e recusa quando o desconto excede o subtotal, cobrindo com testes unitarios desconto menor, igual e maior que o subtotal, e verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~Desconto`

## 5. Taxa de entrega (`delivery-fee`)

- [ ] 5.1 Implementar o value object de distancia com arredondamento half-up no primeiro decimal e recusa de distancia negativa, cobrindo com testes unitarios 15,2 / 15,4 / 15,5 / 15,9 e 100,4 / 100,5, e verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~Distancia`
- [ ] 5.2 Implementar o calculo da taxa (R$ 12,00 ate 15 km, R$ 1,00 por km adicional, recusa acima de 100 km), cobrindo com testes unitarios 10 km, 15 km, 16 km, 20 km, 100 km e 101 km, e verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~TaxaDeEntrega`

## 6. Itens de pedido (`order-item-pricing`)

- [ ] 6.1 Implementar a entidade de item de pedido com SKU, quantidade maior que zero e desconto unitario opcional, cobrindo com testes unitarios SKU inexistente, quantidade zero, quantidade negativa e desconto em ambas as modalidades, e verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~ItemDePedido`
- [ ] 6.2 Implementar o calculo do preco final como (preco base menos desconto unitario) multiplicado pela quantidade, cobrindo com testes unitarios sem desconto, desconto em valor, desconto percentual, desconto igual ao preco e desconto maior que o preco, e verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~ItemDePedido`

## 7. Registro de pedido (`order-registration`)

- [ ] 7.1 Implementar o agregado de pedido com cliente, itens, forma de pagamento, desconto, taxa e total, cobrindo com testes unitarios a recusa sem cliente, sem itens e sem forma de pagamento, e verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~Pedido`
- [ ] 7.2 Implementar o conjunto fechado de formas de pagamento (dinheiro, pix, credito, debito) sem validacao de cartao, cobrindo com testes unitarios as quatro opcoes e uma opcao desconhecida, e verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~Pagamento`
- [ ] 7.3 Implementar o calculo do total como `max(0, subtotal - desconto) + taxa`, cobrindo com testes unitarios sem desconto, desconto em valor, desconto percentual, desconto igual ao subtotal e desconto maior que o subtotal, e verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~Pedido`
- [ ] 7.4 Implementar o caso de uso de registro de pedido integrando cliente, itens, desconto e entrega, cobrindo com testes unitarios o pedido valido e cada recusa de regra de negocio, e verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~Application`

## 8. Persistencia (`Infrastructure`)

- [ ] 8.1 Implementar os repositorios de cliente, produto e pedido com mapeamento dos value objects, cobrindo com testes de integracao a gravacao e a recuperacao de cada agregado, e verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~IntegrationTests`
- [ ] 8.2 Registrar as implementacoes no container de injecao de dependencias da API e verificar com `dotnet build --no-restore` e inicializacao da aplicacao sem excecao de resolucao

## 9. API

- [ ] 9.1 Implementar os endpoints de cadastro de cliente e de produto com respostas de erro para entradas invalidas, cobrindo com testes funcionais HTTP o sucesso e as recusas de documento e preco, e verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~FunctionalTests`
- [ ] 9.2 Implementar o endpoint de registro de pedido, cobrindo com testes funcionais HTTP o pedido valido, o pedido sem produtos, o desconto maior que o subtotal, a distancia acima de 100 km e as duas modalidades de desconto ao mesmo tempo, e verificar com `dotnet test --no-build --verbosity minimal --filter FullyQualifiedName~FunctionalTests`
- [ ] 9.3 Documentar os endpoints e as regras de entrada no README da API e verificar que os exemplos documentados correspondem aos contratos dos testes funcionais

## 10. Verificacao integrada

- [ ] 10.1 Executar `dotnet build --no-restore` e `dotnet test --no-build --verbosity minimal` na solution inteira e verificar zero falhas e nenhum teste ignorado
- [ ] 10.2 Revisar os cenarios dos seis specs em `openspec/changes/sistema-de-pedidos-mvp/specs/` e verificar que cada cenario possui um teste correspondente, listando as lacunas antes de solicitar o archive
