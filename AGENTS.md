# Convenções da Solução .NET

- **Stack:** .NET 10, C# 14, Nullable Reference Types habilitado.
- **Testes:** xUnit, FluentAssertions, NSubstitute, AutoFixture.
- **Comandos padrão:**
  - Build: `dotnet build --no-restore`
  - Testes: `dotnet test --no-build --verbosity minimal`
- **Regra de Ouro:** Nenhum agente deve concluir sua tarefa com falha de compilação ou testes quebrados.

## Convenções de Código (OBRIGATÓRIO)

- **Código SEMPRE em inglês — sem exceção.** Todo identificador em inglês: tipos (classes, interfaces, records, enums, structs), membros (métodos, propriedades, campos, parâmetros, variáveis locais), constantes, namespaces, arquivos, projetos, migrations, entidades/tabelas/colunas de banco, eventos, mensagens de exceção. Interfaces `I...`: `IOrderRepository`/`ICustomerRepository`, **nunca** `IProdutoRepository`/`IClienteRepository`. Domínio/Ubiquitous Language em inglês (`Customer`, não `Cliente`; `Product`, não `Produto`).
- **Idioma dos artefatos ≠ idioma do código.** Documentação e artefatos OpenSpec (`proposal.md`, `specs`, `design.md`, `tasks.md`, ADRs) e comentários (quando permitidos) em **português**; **código em inglês**. Qualquer referência a identificador — inclusive em `@tech-architect` (design/ADRs/tasks) ou `@ca-architect` (contratos, delegação, resumos) — usa nome em **inglês**.
- **Sem comentários por padrão — ótica Clean Code.** Código autoexplicativo. **Proibido XML doc (`///`) em classes, interfaces ou métodos.** Sem comentário óbvio/redundante, sem comentar o *o quê*. Comentário **só** quando indispensável: (1) regra de negócio realmente complexa/não óbvia que o código não consegue expressar; (2) decisão técnica contraintuitiva/estranha — explicar o *por quê* (ou registrar em **ADR**); (3) alerta de perigo (workaround, restrição externa, ordem crítica, invariante sutil, `TODO`/`HACK` com contexto). Antes de comentar: melhor nome, extrair método, simplificar, ou mover a explicação pra ADR/teste.