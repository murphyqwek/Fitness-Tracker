namespace Fitness_Tracker_Domain.Entity
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }

        public string Type { get; set; } = null!;

        public string Topic { get; set; } = null!;

        public string Key { get; set; } = null!;

        public string Payload { get; set; } = null!;

        public DateTimeOffset OccurredAt { get; set; }

        public DateTimeOffset? ProcessedAt { get; set; }
    }
}
