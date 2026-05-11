using Serilog.Context;

namespace OrdersApi.Middleware;

/// <summary>
/// Middleware pronto — não precisa modificar.
///
/// O que ele faz:
///   1. Lê o header X-Correlation-Id da requisição (ou gera um novo GUID).
///   2. Devolve o mesmo valor no header da resposta.
///   3. Injeta o CorrelationId no LogContext do Serilog.
///      Resultado: TODOS os logs gerados durante essa requisição
///      terão automaticamente o campo "CorrelationId" no JSON.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        context.Response.Headers["X-Correlation-Id"] = correlationId;

        // Injeta no contexto do Serilog — aparece em todos os logs do request
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
