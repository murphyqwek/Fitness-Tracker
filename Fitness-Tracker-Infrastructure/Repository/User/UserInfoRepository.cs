using AutoMapper;
using AutoMapper.QueryableExtensions;
using Fitness_Tracker_Application.DTO.User;
using Fitness_Tracker_Application.Repository.User;
using Fitness_Tracker_Infrastructure.Data;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Globalization;
using System.Xml.Linq;

namespace Fitness_Tracker_Infrastructure.Repository.User
{
    public class UserInfoRepository : IUserInformationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDatabase _cache;

        private enum CacheState { Miss, Negative, Hit }

        public UserInfoRepository(ApplicationDbContext context, IMapper mapper, IConnectionMultiplexer cache)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache.GetDatabase();
        }

        public async Task<Result<UserInformationDTO>> GetUserInformationAsync(Guid id, CancellationToken cancellationToken)
        {
            string userKey = $"user:{id.ToString()}";

            var redisResult = await ReadFromRedis(userKey);

            if (redisResult.State == CacheState.Hit)
            {
                return Result.Ok(redisResult.Data!);
            }

            if(redisResult.State == CacheState.Negative) 
            {
                return Result.Fail($"No user with id: {id}");
            }

            var userInfoDTO = await _context.UserInformation.Where(info => info.Id == id)
                                                    .ProjectTo<UserInformationDTO>(_mapper.ConfigurationProvider)
                                                    .FirstOrDefaultAsync(cancellationToken);

            if (userInfoDTO == null)
            {
                HashEntry[] entry = { new HashEntry("IsNotFound", "true") };

                await _cache.HashSetAsync(userKey, entry);
                await _cache.KeyExpireAsync(userKey, TimeSpan.FromMinutes(2));

                return Result.Fail($"No user with id: {id}");
            }

            var entries = UserInfoDTOtoHashEntries(userInfoDTO);

            await _cache.HashSetAsync(userKey, entries.ToArray());
            await _cache.KeyExpireAsync(userKey, TimeSpan.FromHours(1));

            return Result.Ok(userInfoDTO);
        }

        private List<HashEntry> UserInfoDTOtoHashEntries(UserInformationDTO userInfoDTO) {
            List<HashEntry> userHashEntries = new List<HashEntry>() { new HashEntry(nameof(userInfoDTO.login), userInfoDTO.login) };

            if (userInfoDTO.name != null)
            {
                userHashEntries.Add(new HashEntry(nameof(UserInformationDTO.name), userInfoDTO.name));
            }

            if (userInfoDTO.birthDay.HasValue)
            {
                userHashEntries.Add(new HashEntry(nameof(UserInformationDTO.birthDay), userInfoDTO.birthDay.Value.ToString("O")));
            }

            if (userInfoDTO.height.HasValue)
            {
                userHashEntries.Add(new HashEntry(nameof(UserInformationDTO.height), userInfoDTO.height.Value));
            }

            if (userInfoDTO.weight.HasValue)
            {
                userHashEntries.Add(new HashEntry(nameof(UserInformationDTO.weight), userInfoDTO.weight.Value.ToString(CultureInfo.InvariantCulture)));
            }

            return userHashEntries;
        }

        private async Task<(CacheState State, UserInformationDTO? Data)> ReadFromRedis(string key) 
        {
            HashEntry[] userHashEntry = await _cache.HashGetAllAsync(key);

            if(userHashEntry.Length == 0) 
            {
                return (CacheState.Miss, null);
            }

            var notFound = userHashEntry.FirstOrDefault(x => x.Name == "IsNotFound");

            if(notFound.Value == "true") 
            {
                return (CacheState.Negative, null);
            }

            var userDict = userHashEntry.ToDictionary(
                entry => entry.Name.ToString(),
                entry => (object)entry.Value.ToString()
            );

            var userInfoDTO = _mapper.Map<UserInformationDTO>(userDict);

            return (CacheState.Hit, userInfoDTO);
        }

        public async Task<Result> UpdateUserInformationAsync(Guid id, UserUpdateDTO userUpdateDTO, CancellationToken cancellationToken)
        {
            var user = await _context.UserInformation.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

            if (user == null)
            {
                return Result.Fail($"No user with id: {id}");
            }

            var entries = new List<HashEntry>();

            if (userUpdateDTO.name != null && user.Name != userUpdateDTO.name)
            {
                user.Name = userUpdateDTO.name;
                entries.Add(new HashEntry(nameof(UserInformationDTO.name), userUpdateDTO.name));
            }

            if (userUpdateDTO.birthDay.HasValue && user.BirthDay != userUpdateDTO.birthDay)
            {
                user.BirthDay = userUpdateDTO.birthDay;
                entries.Add(new HashEntry(nameof(UserInformationDTO.birthDay), userUpdateDTO.birthDay.Value.ToString("O")));
            }

            if (userUpdateDTO.height.HasValue && user.Height != userUpdateDTO.height)
            {
                user.Height = userUpdateDTO.height;
                entries.Add(new HashEntry(nameof(UserInformationDTO.height), userUpdateDTO.height.Value));
            }

            if (userUpdateDTO.weight.HasValue && user.Weight != userUpdateDTO.weight)
            {
                user.Weight = userUpdateDTO.weight;
                entries.Add(new HashEntry(nameof(UserInformationDTO.weight), userUpdateDTO.weight.Value.ToString(CultureInfo.InvariantCulture)));
            }

            if (entries.Count == 0)
            {
                return Result.Ok();
            }

            await _context.SaveChangesAsync(cancellationToken);

            string userKey = $"user:{id}";
            if (await _cache.KeyExistsAsync(userKey))
            {
                await _cache.HashSetAsync(userKey, entries.ToArray());
                await _cache.KeyExpireAsync(userKey, TimeSpan.FromHours(1));
            }

            return Result.Ok();
        }
    }
}
