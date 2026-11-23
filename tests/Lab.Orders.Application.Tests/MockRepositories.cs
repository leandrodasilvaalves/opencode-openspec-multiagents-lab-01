using Lab.Orders.Application;
using Lab.Orders.Domain;

namespace Lab.Orders.Application.Tests
{
    /// <summary>
    /// Mock implementation of ICustomersRepository for testing.
    /// </summary>
    public class MockCustomerRepository : ICustomersRepository
    {
        private List<Order> _orders = new List<Order>();

        private readonly Customer? _customer;

        public MockCustomerRepository(Customer? customer)
        {
            _customer = customer;
        }

        public List<Order> Orders
        {
            get => _orders;
            set => _orders = value;
        }

        public Task<Customer?> GetByIdAsync(Guid id)
        {
            // Handle null customer case
            if (_customer is null)
            {
                return Task.FromResult<Customer?>(null);
            }
            return Task.FromResult(_customer.Id == id ? _customer : null);
        }

        public Task AddAsync(Customer customer)
        {
            // No-op for mock
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            // No-op for mock
            return Task.CompletedTask;
        }

        public void AddOrder(Order order)
        {
            _orders.Add(order);
        }
    }

    /// <summary>
    /// Helper class for mock OrdersRepository.
    /// </summary>
    public static class Mock
    {
        public static IOrdersRepository OrdersRepository()
        {
            var repo = new MockOrdersRepository();
            return repo;
        }
    }

    /// <summary>
    /// Mock implementation of IOrdersRepository for testing.
    /// </summary>
    public class MockOrdersRepository : IOrdersRepository
    {
        private List<Order> _orders = new List<Order>();

        public Task<Order?> GetByIdAsync(Guid id)
        {
            return Task.FromResult(_orders.FirstOrDefault(o => o.Id == id));
        }

        public Task AddAsync(Order order)
        {
            _orders.Add(order);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            return Task.CompletedTask;
        }
    }
}
