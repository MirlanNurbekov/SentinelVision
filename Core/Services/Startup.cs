var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext());

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

builder.Services
    .AddHealthChecks()
    .AddRabbitMq("amqp://rabbitmq:5672", name: "rabbitmq")
    .AddNpgSql(builder.Configuration.GetConnectionString("Postgres"));

builder.Services
    .AddGrpc();