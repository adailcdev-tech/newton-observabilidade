// ================================================================
//  OrdersController_GABARITO.cs
//
//  Este arquivo contém as implementações dos logs estruturados
//  como referência.
//
//  Nota: O código está dentro de um bloco #if GABARITO para
//  ficar "invisível" ao compilador e não causar erro de duplicidade
//  de rotas e classes na primeira execução.
//
//  Você pode remover a diretiva #if / #endif caso decida
//  substituir o OrdersController.cs original por este arquivo.
// ================================================================

#if GABARITO

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

        _logger.LogInformation("Listagem de pedidos. Retornados {Count} pedidos.", orders.Count);

        return Ok(orders);
    }

    // ── GET /orders/{id} ─────────────────────────────────────
    [HttpGet("{id}")]
    public ActionResult<Order> GetById(string id)
    {
        var order = _service.GetById(id);

        if (order is null)
        {
            _logger.LogWarning("Pedido {OrderId} não encontrado.", id);
            return NotFound(new { error = $"Pedido '{id}' não encontrado." });
        }

        _logger.LogInformation("Pedido {OrderId} encontrado. Usuário: {UserId}, Status: {Status}", order.Id, order.UserId, order.Status);

        return Ok(order);
    }

    // ── POST /orders ─────────────────────────────────────────
    [HttpPost]
    public ActionResult<Order> Create([FromBody] CreateOrderRequest request)
    {
        _logger.LogDebug("Recebida requisição de criação de pedido. Usuário: {UserId}, Produto: {Product}, Quantidade: {Quantity}", request.UserId, request.Product, request.Quantity);

        try
        {
            var order = _service.Create(request);

            _logger.LogInformation("Pedido {OrderId} criado com sucesso. UserId: {UserId}, Produto: {Product}, Quantidade: {Quantity}", order.Id, request.UserId, request.Product, request.Quantity);

            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Falha ao criar pedido para {UserId}. Produto: {Product}, Quantidade: {Quantity}. Erro: {ErrorMessage}", request.UserId, request.Product, request.Quantity, ex.Message);

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
            _logger.LogWarning("Falha ao cancelar o pedido {OrderId}. Pedido não encontrado ou já cancelado.", id);
            return NotFound(new { error = $"Pedido '{id}' não encontrado ou já cancelado." });
        }

        _logger.LogInformation("Pedido {OrderId} cancelado com sucesso.", id);

        return NoContent();
    }
}
#endif