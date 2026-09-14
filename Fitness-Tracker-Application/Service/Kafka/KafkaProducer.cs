using System.Text.Json;

namespace Fitness_Tracker_Application.Service.Kafka
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly IKafkaProducer _producer;

        public KafkaProducer(IKafkaProducer producer)
        {
            _producer = producer;
        }

        public Task Produce<T>(string topic, string key, T message, CancellationToken cancellationToken)
        {
            string serialized = JsonSerializer.Serialize(message);

            return _producer.Produce(topic, key, serialized, cancellationToken);
        }
    }
}
