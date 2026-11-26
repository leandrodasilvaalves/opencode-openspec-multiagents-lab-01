using FluentAssertions;
using Lab.Orders.Domain.Customers;
using Lab.Orders.Infrastructure.Repositories;
using Xunit;

namespace Lab.Orders.Infrastructure.IntegrationTests;

public class CustomerRepositoryTests
{
    private const string ValidCpf = "52998224725";

    [Fact]
    public async Task AddAsync_And_GetByIdAsync_ShouldPersistAndRetrieveCustomerWithAddressAndDocument()
    {
        var dbName = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid();
        var document = Document.Create(ValidCpf);
        var address = Address.Create(
            "Avenida Paulista",
            "1000",
            "Apt 12",
            "Bela Vista",
            "São Paulo",
            "SP",
            "01310100");

        var customer = Customer.Create(customerId, "John Doe", "11988887777", document, address);

        await using (var context = DbContextFactory.Create(dbName))
        {
            var repository = new CustomerRepository(context);
            await repository.AddAsync(customer);
        }

        await using (var context = DbContextFactory.Create(dbName))
        {
            var repository = new CustomerRepository(context);
            var retrieved = await repository.GetByIdAsync(customerId);

            retrieved.Should().NotBeNull();
            retrieved!.CustomerId.Should().Be(customerId);
            retrieved.Name.Should().Be("John Doe");
            retrieved.Phone.Should().Be("11988887777");
            retrieved.Document.Value.Should().Be(ValidCpf);
            retrieved.Document.Kind.Should().Be(DocumentKind.Cpf);
            retrieved.Address.Should().NotBeNull();
            retrieved.Address.Street.Should().Be("Avenida Paulista");
            retrieved.Address.Number.Should().Be("1000");
            retrieved.Address.Complement.Should().Be("Apt 12");
            retrieved.Address.Neighborhood.Should().Be("Bela Vista");
            retrieved.Address.City.Should().Be("São Paulo");
            retrieved.Address.State.Should().Be("SP");
            retrieved.Address.PostalCode.Should().Be("01310100");
        }
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerDoesNotExist_ShouldReturnNull()
    {
        var dbName = Guid.NewGuid().ToString();

        await using var context = DbContextFactory.Create(dbName);
        var repository = new CustomerRepository(context);
        var retrieved = await repository.GetByIdAsync(Guid.NewGuid());

        retrieved.Should().BeNull();
    }
}
