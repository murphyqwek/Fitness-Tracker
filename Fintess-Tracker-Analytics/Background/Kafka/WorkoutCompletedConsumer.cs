using AutoMapper;
using Confluent.Kafka;
using Fintess_Tracker_Analytics.DTO;
using Fintess_Tracker_Analytics.Service;
using Fitness_Tracker_Shared;
using System.Diagnostics;
using System.Text.Json;

namespace Fintess_Tracker_Analytics.Background.Kafka
{
    public class WorkoutCompletedConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMapper _mapper;
        private readonly ILogger<WorkoutCompletedConsumer> _logger;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public WorkoutCompletedConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory, IMapper mapper, ILogger<WorkoutCompletedConsumer> logger)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _mapper = mapper;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig()
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
                GroupId = _configuration["Kafka:GroupId"],
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();

            consumer.Subscribe("workout.completed");
            _logger.LogInformation("Workout consumer subscribed to {Topic} in group {ConsumerGroup}",
                "workout.completed", config.GroupId);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(stoppingToken);
                        if (consumeResult is not null)
                        {
                            await ProcessMessageAsync(consumer, consumeResult, stoppingToken);
                        }
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (ConsumeException exception)
                    {
                        _logger.LogWarning(exception, "Kafka consume failed with code {KafkaErrorCode} in group {ConsumerGroup}",
                            exception.Error.Code, config.GroupId);
                        if (exception.Error.IsFatal)
                        {
                            throw;
                        }
                    }
                }
            }
            finally
            {
                consumer.Close();
            }
        }

        internal async Task ProcessMessageAsync(
            IConsumer<string, string> consumer,
            ConsumeResult<string, string> message,
            CancellationToken cancellationToken)
        {
            var startedAt = Stopwatch.GetTimestamp();
            WorkoutCompletedV1Event? workoutEvent = null;

            using var kafkaScope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["Topic"] = message.Topic,
                ["Partition"] = message.Partition.Value,
                ["Offset"] = message.Offset.Value
            });

            try
            {
                workoutEvent = JsonSerializer.Deserialize<WorkoutCompletedV1Event>(message.Message.Value, JsonOptions)
                    ?? throw new JsonException("Workout event is null.");

                using var eventScope = _logger.BeginScope(new Dictionary<string, object>
                {
                    ["EventId"] = workoutEvent.EventId,
                    ["WorkoutId"] = workoutEvent.WorkoutId
                });
                using var scope = _scopeFactory.CreateScope();
                var analyticService = scope.ServiceProvider.GetRequiredService<AnalyticService>();
                var dto = _mapper.Map<CompletedWorkoutDTO>(workoutEvent);
                var result = await analyticService.SaveWorkoutAsync(dto, cancellationToken);

                if (result.IsFailed)
                {
                    throw new InvalidOperationException("Analytics persistence returned a failed result.");
                }

                consumer.Commit(message);
                _logger.LogInformation("Workout event processed and Kafka offset committed in {ElapsedMs} ms",
                    Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to process workout event {EventId} for workout {WorkoutId}; offset was not confirmed as processed",
                    workoutEvent?.EventId, workoutEvent?.WorkoutId);
                throw;
            }
        }
    }
}
