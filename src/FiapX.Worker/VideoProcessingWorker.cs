using FiapX.Application.Controllers.Videos;
using FiapX.Application.Interfaces.DataSources;
using FiapX.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace FiapX.Worker;

public class VideoProcessingWorker : BackgroundService
{
    private readonly ILogger<VideoProcessingWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string QueueName = "video-processing";

    public VideoProcessingWorker(
        ILogger<VideoProcessingWorker> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = configuration["RabbitMQ:UserName"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                var videoMessage = JsonSerializer.Deserialize<VideoMessage>(message);
                if (videoMessage != null)
                {
                    _logger.LogInformation("Processando vídeo: {VideoId}", videoMessage.VideoId);
                    await ProcessVideoAsync(videoMessage.VideoId);
                    _logger.LogInformation("Vídeo processado com sucesso: {VideoId}", videoMessage.VideoId);
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem: {Message}", message);
                _channel.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _channel.BasicConsume(
            queue: QueueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("Worker iniciado. Aguardando mensagens...");

        return Task.CompletedTask;
    }

    private async Task ProcessVideoAsync(Guid videoId)
    {
        using var scope = _serviceProvider.CreateScope();

        var videoDataSource = scope.ServiceProvider.GetRequiredService<IVideoDataSource>();
        var userDataSource = scope.ServiceProvider.GetRequiredService<IUserDataSource>();
        var storageService = scope.ServiceProvider.GetRequiredService<IStorageService>();
        var processingService = scope.ServiceProvider.GetRequiredService<IVideoProcessingService>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var messageQueueService = scope.ServiceProvider.GetRequiredService<IMessageQueueService>();

        var controller = new VideoController(
            videoDataSource,
            userDataSource,
            storageService,
            messageQueueService,
            processingService,
            notificationService);

        await controller.ProcessVideo(videoId);
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        base.Dispose();
    }
}

public class VideoMessage
{
    public Guid VideoId { get; set; }
}
