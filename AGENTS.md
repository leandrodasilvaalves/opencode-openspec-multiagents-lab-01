# Convenções da Solução .NET

- **Stack:** .NET 10, C# 14, Nullable Reference Types habilitado.
- **Testes:** xUnit, FluentAssertions, NSubstitute, AutoFixture.
- **Comandos padrão:**
  - Build: `dotnet build --no-restore`
  - Testes: `dotnet test --no-build --verbosity minimal`
- **Regra de Ouro:** Nenhum agente deve concluir sua tarefa com falha de compilação ou testes quebrados.