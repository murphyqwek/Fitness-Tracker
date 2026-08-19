using AutoMapper;
using Fitness_Tracker_Application.Repository.User;
using Fitness_Tracker_Infrastructure.Data;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Tracker_Infrastructure.Repository.User
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public UserRepository(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task AddNewUserAsync(Fitness_Tracker_Domain.Entity.User user, CancellationToken cancellationToken)
        {
            await _dbContext.Users.AddAsync(_mapper.Map<Model.UserEntity>(user), cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> IsLoginAlreadyTakenAsync(string login, CancellationToken cancellationToken)
        {
            return await _dbContext.Users.AnyAsync(user => user.Login == login, cancellationToken);
        }

        public async Task<Result<Fitness_Tracker_Domain.Entity.User>> GetUserByLoginAsync(string login, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.Where(user => user.Login == login).FirstOrDefaultAsync(cancellationToken);

            if(user == null)
            {
                return Result.Fail<Fitness_Tracker_Domain.Entity.User>("User not found");
            }

            return Result.Ok(_mapper.Map<Fitness_Tracker_Domain.Entity.User>(user));
        }

        public async Task<Result<Fitness_Tracker_Domain.Entity.User>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

            if(user == null)
            {
                return Result.Fail<Fitness_Tracker_Domain.Entity.User>("User not found");
            }

            return Result.Ok(_mapper.Map<Fitness_Tracker_Domain.Entity.User>(user));
        }
    }
}
