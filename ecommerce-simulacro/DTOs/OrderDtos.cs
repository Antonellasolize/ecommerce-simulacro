namespace EcommerceSimulacro.DTOs;
public record CreateOrderItem(int ProductId, int Quantity);
public record CreateOrderRequest(List<CreateOrderItem> Items);