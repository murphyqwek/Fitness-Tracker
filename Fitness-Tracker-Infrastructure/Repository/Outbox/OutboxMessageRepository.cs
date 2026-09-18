using Fitness_Tracker_Application.Repository.Outbox;
using Fitness_Tracker_Domain.Entity;
using Fitness_Tracker_Infrastructure.Data;

namespace Fitness_Tracker_Infrastructure.Repository.Outbox
{
    public class OutboxMessageRepository : IOutboxMessageRepository
    {
        private readonly ApplicationDbContext _context;

        public OutboxMessageRepository(ApplicationDbContext context) 
        { 
            _context = context;
        }

        public void AddMessage(OutboxMessage message)
        {
            _context.OutboxMessages.Add(message);
        }
    }
}
