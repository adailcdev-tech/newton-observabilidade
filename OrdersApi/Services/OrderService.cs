using OrdersApi.Models;
using System.Collections.Concurrent;

namespace OrdersApi.Services;

public interface IOrderService
{
    Order?        GetById(string id);
    List<Order>   GetAll();
    Order         Create(CreateOrderRequest request);
    bool          Cancel(string id);
}

// In-memory store — sem banco de dados necessário para a aula.
public class OrderService : IOrderService
{
    private readonly ConcurrentDictionary<string, Order> _store = new();

    // Pre-seed com alguns pedidos para facilitar testes
    public OrderService()
    {
        var seed = new[]
        {
            new Order { Id = "ORD-001", UserId = "USR-01", Product = "Teclado Mecânico", Quantity = 1, Status = "Completed" },
            new Order { Id = "ORD-002", UserId = "USR-02", Product = "Monitor 4K",       Quantity = 2, Status = "Pending"   },
            new Order { Id = "ORD-003", UserId = "USR-01", Product = "Mouse Gamer",       Quantity = 1, Status = "Cancelled" },
        };
        foreach (var o in seed) _store[o.Id] = o;
    }

    public Order? GetById(string id) =>
        _store.TryGetValue(id, out var order) ? order : null;

    public List<Order> GetAll() =>
        _store.Values.OrderBy(o => o.CreatedAt).ToList();

    public Order Create(CreateOrderRequest request)
    {
        // Simula validação de estoque: recusa se Quantity > 10
        if (request.Quantity > 10)
            throw new InvalidOperationException(
                $"Estoque insuficiente para '{request.Product}': solicitado {request.Quantity}, máximo 10.");

        var order = new Order
        {
            Id        = $"ORD-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            UserId    = request.UserId,
            Product   = request.Product,
            Quantity  = request.Quantity,
            Status    = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _store[order.Id] = order;
        return order;
    }

    public bool Cancel(string id)
    {
        if (!_store.TryGetValue(id, out var order)) return false;
        if (order.Status == "Cancelled") return false;

        order.Status = "Cancelled";
        return true;
    }
}
