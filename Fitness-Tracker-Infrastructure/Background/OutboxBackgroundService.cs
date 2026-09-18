using Fitness_Tracker_Application.Service.Kafka;
using Fitness_Tracker_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fitness_Tracker_Infrastructure.BackgroundServices;

public sealed class OutboxBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IKafkaProducer _kafkaProducer;

    public OutboxBackgroundService(IServiceScopeFactory scopeFactory, IKafkaProducer kafkaProducer)
    {
        _scopeFactory = scopeFactory;
        _kafkaProducer = kafkaProducer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(2));

        await ProcessOutboxAsync(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessOutboxAsync(stoppingToken);
        }
    }

    private async Task ProcessOutboxAsync(
    CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        await using var transaction =
            await context.Database.BeginTransactionAsync(
                cancellationToken);

        var messages = await context.OutboxMessages
            .FromSqlRaw("""
                        SELECT *
                        FROM "OutboxMessages"
                        WHERE "ProcessedAt" IS NULL
                        ORDER BY "OccurredAt"
                        LIMIT 100
                        FOR UPDATE SKIP LOCKED
                        """)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await _kafkaProducer.ProduceAsync(
                    message.Topic,
                    message.Key,
                    message.Payload,
                    cancellationToken);

                message.ProcessedAt =
                    DateTimeOffset.UtcNow;
            }
            catch
            {
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }
}