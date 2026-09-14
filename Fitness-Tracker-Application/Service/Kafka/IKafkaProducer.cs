namespace Fitness_Tracker_Application.Service.Kafka
{
    public interface IKafkaProducer
    {
        Task Produce<T>(string topic, string key, T message, CancellationToken cancellationToken);
    }
}
