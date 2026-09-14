using Fitness_Tracker_Domain.Entity;

namespace Fitness_Tracker_Application.Repository.Outbox
{
    public interface IOutboxMessageRepository
    {
        void AddMessage(OutboxMessage message);
    }
}
