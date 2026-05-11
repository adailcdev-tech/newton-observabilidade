# OrdersApi — Template de Observabilidade

API de pedidos já configurada com **Serilog** em JSON estruturado.  
Sua única tarefa: **adicionar os logs** nos pontos marcados com `TODO` no controller.

---

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- VS Code com extensão C# (C# Dev Kit)
- Postman (para testar os endpoints)

Nada mais. Sem Docker. Sem Prometheus. Sem Grafana. 🎉

---

## Como rodar

```bash
# 1. Restaurar dependências
dotnet restore

# 2. Rodar a API
dotnet run

# A API sobe em http://localhost:5000
# Swagger UI em http://localhost:5000/swagger
```

Ao rodar, você verá os logs JSON diretamente no terminal:

```json
{"@t":"2024-03-15T14:23:01Z","@mt":"OrdersApi iniciada. Ambiente: {Environment}","Environment":"Development"}
{"@t":"2024-03-15T14:23:05Z","@mt":"HTTP GET /orders responded 200 in 4.532 ms","RequestMethod":"GET",...}
```

Os logs também são gravados em `logs/orders-YYYYMMDD.jsonl`.

---

## Endpoints disponíveis

| Método | Rota | Descrição |
|--------|------|-----------|
| GET    | `/orders` | Lista todos os pedidos |
| GET    | `/orders/{id}` | Busca um pedido por ID |
| POST   | `/orders` | Cria um novo pedido |
| DELETE | `/orders/{id}/cancel` | Cancela um pedido |

### Exemplos com Postman / curl

**Listar pedidos (já tem 3 pré-cadastrados):**
```
GET http://localhost:5000/orders
```

**Buscar pedido existente:**
```
GET http://localhost:5000/orders/ORD-001
```

**Buscar pedido inexistente (gera Warning no log):**
```
GET http://localhost:5000/orders/ORD-999
```

**Criar pedido com sucesso:**
```json
POST http://localhost:5000/orders
Content-Type: application/json

{
  "userId": "USR-42",
  "product": "Headset",
  "quantity": 2
}
```

**Criar pedido com erro (Quantity > 10 — gera LogError):**
```json
POST http://localhost:5000/orders
Content-Type: application/json

{
  "userId": "USR-42",
  "product": "SSD 1TB",
  "quantity": 15
}
```

**Cancelar pedido:**
```
DELETE http://localhost:5000/orders/ORD-001/cancel
```

---

## Sua tarefa — os 8 TODOs

Abra `Controllers/OrdersController.cs` e implemente cada TODO:

| # | Endpoint | Nível | Campos obrigatórios |
|---|----------|-------|---------------------|
| 1 | GET /orders | Information | `{Count}` |
| 2 | GET /orders/{id} — não encontrado | Warning | `{OrderId}` |
| 3 | GET /orders/{id} — encontrado | Information | `{OrderId}`, `{UserId}`, `{Status}` |
| 4 | POST /orders — início | Debug | `{UserId}`, `{Product}`, `{Quantity}` |
| 5 | POST /orders — sucesso ⭐ | Information | `{OrderId}`, `{UserId}`, `{Product}`, `{Quantity}` |
| 6 | POST /orders — erro | Error | `{UserId}`, `{Product}`, `{Quantity}`, `{ErrorMessage}` |
| 7 | DELETE — não encontrado | Warning | `{OrderId}` |
| 8 | DELETE — cancelado | Information | `{OrderId}` |

### Regra de ouro

```csharp
// ✅ CERTO — campo estruturado e pesquisável
_logger.LogInformation("Pedido {OrderId} criado para {UserId}", order.Id, order.UserId);

// ❌ ERRADO — só texto, não é pesquisável por OrderId
_logger.LogInformation($"Pedido {order.Id} criado para {order.UserId}");
```

### Para o TODO 6 (erro com exceção)

```csharp
// A exceção é sempre o PRIMEIRO argumento
_logger.LogError(ex, "Mensagem {Campo}", valor);
//               ^^
//               Garante que o stack trace apareça no JSON
```

---

## Como verificar se seu log está estruturado

Após implementar um TODO, faça a chamada no Postman e observe o terminal.  
Um log bem estruturado deve parecer assim:

```json
{
  "@t": "2024-03-15T14:23:01.000Z",
  "@mt": "Pedido {OrderId} criado com sucesso — UserId: {UserId}, ...",
  "@l": "Information",
  "OrderId": "ORD-A3F7C1",
  "UserId": "USR-42",
  "Product": "Headset",
  "Quantity": 2,
  "CorrelationId": "c2d4e6f8-...",
  "MachineName": "meu-pc"
}
```

Repare: `OrderId`, `UserId`, `Product` são **campos separados**, não parte da string.  
Isso permite filtrar: _"todos os pedidos do USR-42 com erro nas últimas 24h"_.

---

## Estrutura do projeto

```
OrdersApi/
├── Controllers/
│   ├── OrdersController.cs          ← 🎯 Sua tarefa está aqui
│   └── OrdersController_GABARITO.cs ← 🔒 Abra só depois de tentar!
├── Middleware/
│   └── CorrelationIdMiddleware.cs   ← Já pronto, não mexer
├── Models/
│   └── Order.cs                     ← Modelos de dados
├── Services/
│   └── OrderService.cs              ← Lógica de negócio (in-memory)
├── logs/                            ← Criada automaticamente ao rodar
├── Program.cs                       ← Serilog já configurado aqui
├── appsettings.json
└── OrdersApi.csproj
```
