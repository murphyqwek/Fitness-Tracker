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

        public async Task ProduceAsync(string topic, string key, string message, CancellationToken cancellationToken)
        {
            await _producer.ProduceAsync(topic,    
                        new Message<string, string>
                        {
                            Key = key,
                            Value = message
                        },
                        cancellationToken);
        }
    }
}
