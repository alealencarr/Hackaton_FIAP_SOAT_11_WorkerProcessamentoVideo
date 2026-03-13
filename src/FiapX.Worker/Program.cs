using FiapX.Infrastructure;
using FiapX.Worker;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/worker-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Iniciando FIAP X Worker...");

    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddInfrastructureForWorker(builder.Configuration);
    builder.Services.AddHostedService<VideoProcessingWorker>();

    var host = builder.Build();
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker terminou inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}