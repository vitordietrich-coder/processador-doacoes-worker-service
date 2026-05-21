using Microsoft.EntityFrameworkCore;
using Processador.Doacoes.Worker.Service.Api.Data;
using Processador.Doacoes.WorkerService.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Processador.Doacoes.WorkerService.Workers;

public class DonationConsumerWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DonationConsumerWorker> _logger;

    private IConnection? _connection;
    private IModel? _channel;

    public DonationConsumerWorker(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<DonationConsumerWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Host"],
            UserName = _configuration["RabbitMQ:Username"],
            Password = _configuration["RabbitMQ:Password"]
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        var queueName = _configuration["RabbitMQ:DonationQueue"] ?? "donations";

        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        _channel.BasicQos(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false);

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (_, eventArgs) =>
        {
            try
            {
                var body = eventArgs.Body.ToArray();

                var json = Encoding.UTF8.GetString(body);

                var message = JsonSerializer.Deserialize<DonationReceivedEvent>(json);

                if (message is null)
                {
                    _channel.BasicNack(eventArgs.DeliveryTag, false, false);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();

                var context = scope.ServiceProvider
                    .GetRequiredService<WorkerDbContext>();

                var campaign = await context.Campanhas
                    .FirstOrDefaultAsync(
                        x => x.Id == message.CampaignId,
                        stoppingToken);

                if (campaign is null)
                {
                    _logger.LogWarning(
                        "Campaign {CampaignId} not found.",
                        message.CampaignId);

                    _channel.BasicNack(eventArgs.DeliveryTag, false, false);
                    return;
                }

                campaign.AddDonation(message.Amount);

                await context.SaveChangesAsync(stoppingToken);

                _channel.BasicAck(eventArgs.DeliveryTag, false);

                _logger.LogInformation(
                    "Donation {DonationId} processed. Campaign {CampaignId} updated with amount {Amount}.",
                    message.DonationId,
                    message.CampaignId,
                    message.Amount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing donation message.");

                _channel?.BasicNack(eventArgs.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();

        base.Dispose();
    }
}