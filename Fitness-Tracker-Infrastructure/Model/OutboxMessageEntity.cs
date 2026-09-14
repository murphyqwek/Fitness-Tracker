namespace Fitness_Tracker_Infrastructure.Model
{
    public class OutboxMessageEntity
    {
        public Guid Id { get; set; }

        public string Type { get; set; } = null!;

        public string Payload { get; set; } = null!;

        public DateTimeOffset OccurredAt { get; set; }

        public DateTimeOffset? ProcessedAt { get; set; }
    }
}
