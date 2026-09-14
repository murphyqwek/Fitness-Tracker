using Confluent.Kafka;
using System.Text.Json;

namespace Fitness_Tracker_Application.Service.Kafka
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer<string, string> _producer;

        public KafkaProducer(IProducer<string, string> producer)
        {
            _producer = producer;
        }

        public async Task ProduceAsync<T>(string topic, string key, T message, CancellationToken cancellationToken)
        {
            string serialized = JsonSerializer.Serialize(message);

            await _producer.ProduceAsync(topic,    
                        new Message<string, string>
                        {
                            Key = key,
                            Value = serialized
                        },
                        cancellationToken);
        }
    }
}
