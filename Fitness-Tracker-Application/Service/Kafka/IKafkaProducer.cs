namespace Fitness_Tracker_Application.Service.Kafka
{
    public interface IKafkaProducer
    {
        Task ProduceAsync(string topic, string key, string message, CancellationToken cancellationToken);
    }
}
