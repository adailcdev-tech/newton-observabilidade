// ================================================================
//  OrdersController.cs
//
//  TAREFA DA AULA:
//    Adicionar logs estruturados nos pontos marcados com TODO.
//    O Serilog já está configurado — você só precisa chamar _logger.
//
//  Guia rápido de níveis:
//    _logger.LogDebug(...)       → detalhes internos (dev only)
//    _logger.LogInformation(...) → fluxo normal de negócio
//    _logger.LogWarning(...)     → situação inesperada, mas recuperável
//    _logger.LogError(ex, ...)   → falha que afeta o usuário
//
//  REGRA DE OURO — structured logging:
//    ✅  _logger.LogInformation("Pedido {OrderId} criado", order.Id);
//    ❌  _logger.LogInformation($"Pedido {order.Id} criado");
//    O primeiro cria um campo pesquisável. O segundo é só texto.
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
        //
        // Dica: use {Count} como campo estruturado.
        // Exemplo de saída esperada no JSON:
        //   { "msg": "Listagem de pedidos", "Count": 3 }
        //
        // _logger.Log___(...)

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
            //
            // Campos sugeridos: {OrderId}
            // Nível: Warning (o cliente buscou algo inexistente — suspeito,
            // mas não é um erro do sistema)
            //
            // _logger.Log___(...)

            return NotFound(new { error = $"Pedido '{id}' não encontrado." });
        }

        // TODO 3 — Log informativo: pedido encontrado e retornado.
        //
        // Campos sugeridos: {OrderId}, {UserId}, {Status}
        //
        // _logger.Log___(...)

        return Ok(order);
    }

    // ── POST /orders ─────────────────────────────────────────
    [HttpPost]
    public ActionResult<Order> Create([FromBody] CreateOrderRequest request)
    {
        // TODO 4 — Log de debug: chegou uma requisição de criação.
        //
        // Campos sugeridos: {UserId}, {Product}, {Quantity}
        // Nível: Debug (detalhe de baixo nível, útil para investigar bugs)
        //
        // _logger.Log___(...)

        try
        {
            var order = _service.Create(request);

            // TODO 5 — Log informativo: pedido criado com sucesso. ⭐ (mais importante da aula)
            //
            // Campos OBRIGATÓRIOS: {OrderId}, {UserId}, {Product}, {Quantity}
            // Este log é o mais importante: precisa conter tudo para
            // reconstruir o que aconteceu sem consultar o banco.
            //
            // _logger.Log___(...)

            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            // TODO 6 — Log de erro: criação falhou por regra de negócio.
            //
            // Campos sugeridos: {UserId}, {Product}, {Quantity}, {ErrorMessage}
            // Passe a exceção como PRIMEIRO argumento:
            //   _logger.LogError(ex, "mensagem {Campo}", valor)
            // Isso garante que o stack trace fique no JSON.
            //
            // _logger.Log___(...)

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
            //
            // Campos sugeridos: {OrderId}
            //
            // _logger.Log___(...)

            return NotFound(new { error = $"Pedido '{id}' não encontrado ou já cancelado." });
        }

        // TODO 8 — Log informativo: cancelamento realizado.
        //
        // Campos sugeridos: {OrderId}
        //
        // _logger.Log___(...)

        return NoContent();
    }
}
