using AutoMapper;
using Confluent.Kafka;
using Fintess_Tracker_Analytics.DTO;
using Fintess_Tracker_Analytics.Service;
using Fitness_Tracker_Shared;
using System.Text.Json;

namespace Fintess_Tracker_Analytics.Background.Kafka
{
    public class WorkoutCompletedConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMapper _mapper;

        public WorkoutCompletedConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory, IMapper mapper)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _mapper = mapper;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig()
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
                GroupId = _configuration["Kafka:GroupId"],
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            var consumer = new ConsumerBuilder<string, string>(config).Build();

            consumer.Subscribe("workout.completed");

            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);
                    if (consumeResult == null)
                    {
                        continue;
                    }

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var workoutCompletedEvent = JsonSerializer.Deserialize<WorkoutCompletedV1Event>(consumeResult.Message.Value);

                        var analyticService = scope.ServiceProvider.GetRequiredService<AnalyticService>();

                        var dto = _mapper.Map<CompletedWorkoutDTO>(workoutCompletedEvent);

                        await analyticService.SaveWorkoutAsync(dto, stoppingToken);

                        consumer.Commit(consumeResult);
                    }
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Error occurred: {e.Error.Reason}");
                }
            }
        }
    }
}
