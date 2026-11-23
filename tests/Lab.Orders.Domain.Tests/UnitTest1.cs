using FluentAssertions;
using NetArchTest.Rules;

namespace Lab.Orders.Domain.Tests;

public class UnitTest1
{
  [Fact]
  public void Domain_Nao_Deve_Depender_De_Outras_Camadas()
  {
    var domainAssembly = typeof(Lab.Orders.Domain.Class1).Assembly;

    var result = Types.InAssembly(domainAssembly)
        .ShouldNot()
        .HaveDependencyOnAny(
            "SeuProjeto.Application",
            "SeuProjeto.Infrastructure",
            "SeuProjeto.Api",
            "Microsoft.EntityFrameworkCore")
        .GetResult();

    result.IsSuccessful.Should().BeTrue(
        $"Violação de dependência no Domain: {string.Join(", ", result.FailingTypeNames ?? [])}");
  }
}
