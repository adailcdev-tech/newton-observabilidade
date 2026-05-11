// ============================================================
//  OrdersApi — Template de Observabilidade
//  Serilog já está configurado. Sua tarefa: adicionar logs!
// ============================================================

using OrdersApi.Middleware;
using OrdersApi.Services;
using Serilog;
using Serilog.Formatting.Compact;

// ── 1. Configurar Serilog ANTES de construir o app ──────────
//
//  CompactJsonFormatter  →  cada linha de log é um JSON válido.
//  Enrich.FromLogContext →  campos injetados pelo middleware
//                           (CorrelationId) aparecem em todos os logs.
//  Enrich.WithMachineName → útil quando há vários containers.
//
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .WriteTo.Console(new CompactJsonFormatter())   // JSON no console
    .WriteTo.File(
        formatter: new CompactJsonFormatter(),
        path: "logs/orders-.jsonl",               // JSON em arquivo também
        rollingInterval: RollingInterval.Day)
    .MinimumLevel.Debug()                          // Mude para Information em produção
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// ── 2. Substituir o logger padrão pelo Serilog ──────────────
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Serviço de pedidos (in-memory para a aula)
builder.Services.AddSingleton<IOrderService, OrderService>();

var app = builder.Build();

// ── 3. Middleware de CorrelationId ───────────────────────────
//  Injeta um X-Correlation-Id em cada requisição.
//  Ele fica disponível em todos os logs via LogContext.
app.UseMiddleware<CorrelationIdMiddleware>();

// ── 4. Middleware de request logging do Serilog ─────────────
//  Gera automaticamente um log estruturado para CADA request:
//    { "RequestMethod":"GET", "RequestPath":"/orders", "StatusCode":200, ... }
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.000} ms";
});

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

// ── 5. Log de inicialização ──────────────────────────────────
Log.Information("OrdersApi iniciada. Ambiente: {Environment}", app.Environment.EnvironmentName);

app.Run();
