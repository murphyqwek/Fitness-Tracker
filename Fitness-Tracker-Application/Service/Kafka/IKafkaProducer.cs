namespace Fitness_Tracker_Application.Service.Kafka
{
    public interface IKafkaProducer
    {
        Task ProduceAsync<T>(string topic, string key, T message, CancellationToken cancellationToken);
    }
}
