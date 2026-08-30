using AutoMapper;
using Fitness_Tracker_Application.DTO.User;
using Fitness_Tracker_Application.Repository.User;
using Fitness_Tracker_Infrastructure.Data;
using Fitness_Tracker_Infrastructure.Model;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StackExchange.Redis;

namespace Fitness_Tracker_Infrastructure.Repository.User
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IDatabase _cache;

        public UserRepository(ApplicationDbContext dbContext, IMapper mapper, IConnectionMultiplexer connectionMultiplexer)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _cache = connectionMultiplexer.GetDatabase();
        }

        public async Task<Result> AddNewUserAsync(Fitness_Tracker_Domain.Entity.User user, CancellationToken cancellationToken)
        {
            UserEntity userEntity = _mapper.Map<UserEntity>(user);

            try
            {
                await _dbContext.Users.AddAsync(userEntity, cancellationToken);

                await _dbContext.UserInformation.AddAsync(new UserInformatonEntity()
                {
                    Id = userEntity.Id,
                    User = userEntity
                });

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return Result.Fail($"User's login {user.Login} is already taken");
            }
            catch (Exception)
            {
                return Result.Fail("Unexpectd error accuired during creation of new User");
            }


            string userKey = $"user:{user.Id}";
            var hashEntries = new HashEntry[]
                                {
                                    new HashEntry(nameof(UserInformationDTO.Login), user.Login),
                                };

            await _cache.HashSetAsync(userKey, hashEntries);
            await _cache.KeyExpireAsync(userKey, TimeSpan.FromHours(1));

            return Result.Ok();
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
