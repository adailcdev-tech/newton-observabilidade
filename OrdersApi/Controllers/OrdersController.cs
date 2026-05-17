// ================================================================
//  OrdersController.cs
//
//  TAREFA DA AULA:
//    Adicionar logs estruturados nos pontos marcados com TODO.
//    O Serilog já está configurado — você só precisa chamar _logger.
// ================================================================

using Microsoft.AspNetCore.Mvc;
using OrdersApi.Models;
using OrdersApi.Services;

namespace OrdersApi.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService service, ILogger<OrdersController> logger)
    {
        _service = service;
        _logger  = logger;
    }

    // ── GET /orders ──────────────────────────────────────────
    [HttpGet]
    public ActionResult<List<Order>> GetAll()
    {
        var orders = _service.GetAll();
        
        // TODO 1 — Log informativo: quantos pedidos foram retornados?
        _logger.LogInformation("Listagem de pedidos retornada com {Count} itens", orders.Count);

        return Ok(orders);
    }

    // ── GET /orders/{id} ─────────────────────────────────────
    [HttpGet("{id}")]
    public ActionResult<Order> GetById(string id)
    {
        var order = _service.GetById(id);
        
        if (order is null)
        {
            // TODO 2 — Log de aviso: pedido não encontrado.
            _logger.LogWarning("Pedido não encontrado. {OrderId}", id);
            return NotFound(new { error = $"Pedido '{id}' não encontrado." });
        }

        // TODO 3 — Log informativo: pedido encontrado e retornado.
        _logger.LogInformation("Pedido encontrado e retornado. {OrderId} {UserId} {Status}", 
            order.Id, order.UserId, order.Status);

        return Ok(order);
    }

    // ── POST /orders ─────────────────────────────────────────
    [HttpPost]
    public ActionResult<Order> Create([FromBody] CreateOrderRequest request)
    {
        // TODO 4 — Log de debug: chegou uma requisição de criação.
        _logger.LogDebug("Requisição de criação de pedido recebida. {UserId} {Product} {Quantity}", 
            request.UserId, request.Product, request.Quantity);

        try
        {
            var order = _service.Create(request);
            
            // TODO 5 — Log informativo: pedido criado com sucesso. ⭐ (mais importante da aula)
            _logger.LogInformation("Pedido criado com sucesso. {OrderId} {UserId} {Product} {Quantity}", 
                order.Id, order.UserId, order.Product, order.Quantity);

            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            // TODO 6 — Log de erro: criação falhou por regra de negócio.
            _logger.LogError(ex, "Falha ao criar pedido por regra de negócio. {UserId} {Product} {Quantity} {ErrorMessage}", 
                request.UserId, request.Product, request.Quantity, ex.Message);

            return BadRequest(new { error = ex.Message });
        }
    }

    // ── DELETE /orders/{id}/cancel ───────────────────────────
    [HttpDelete("{id}/cancel")]
    public ActionResult Cancel(string id)
    {
        var success = _service.Cancel(id);
        
        if (!success)
        {
            // TODO 7 — Log de aviso: tentativa de cancelar pedido inexistente ou já cancelado.
            _logger.LogWarning("Tentativa de cancelamento falhou. Pedido inexistente ou já cancelado. {OrderId}", id);
            
            return NotFound(new { error = $"Pedido '{id}' não encontrado ou já cancelado." });
        }

        // TODO 8 — Log informativo: cancelamento realizado.
        _logger.LogInformation("Pedido cancelado com sucesso. {OrderId}", id);

        return NoContent();
    }
}