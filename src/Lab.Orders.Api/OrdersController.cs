using Microsoft.AspNetCore.Mvc;
using Lab.Orders.Application;
using Lab.Orders.Domain;

namespace Lab.Orders.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly CreateOrderUseCase _createOrderUseCase;

    public OrdersController(CreateOrderUseCase createOrderUseCase)
    {
        _createOrderUseCase = createOrderUseCase;
    }

    [HttpPost]
    public async Task<IActionResult> Post(CreateOrderCommand command)
    {
        var result = await _createOrderUseCase.ExecuteAsync(command);

        if (result.Success)
        {
            return CreatedAtAction(nameof(Post), new { id = result.Order?.Id }, result.Order);
        }

        if (result.Error!.Contains("Limite de crédito excedido"))
        {
            return Conflict(new { Error = result.Error });
        }

        if (result.Error!.Contains("Cliente não encontrado"))
        {
            return NotFound(new { Error = result.Error });
        }

        // Validacões de domínio
        return BadRequest(new { Error = result.Error });
    }
}
