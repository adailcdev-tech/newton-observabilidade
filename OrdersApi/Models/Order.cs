namespace OrdersApi.Models;

public class Order
{
    public string Id      { get; set; } = string.Empty;
    public string UserId  { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public int    Quantity { get; set; }
    public string Status  { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CreateOrderRequest
{
    public string UserId   { get; set; } = string.Empty;
    public string Product  { get; set; } = string.Empty;
    public int    Quantity { get; set; }
}
