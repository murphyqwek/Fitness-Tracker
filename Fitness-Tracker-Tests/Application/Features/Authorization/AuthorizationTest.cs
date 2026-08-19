using AutoMapper;
using Fitness_Tracker_Application.Features.Users.Authorization;
using Fitness_Tracker_Application.Mapping;
using Fitness_Tracker_Application.Repository.User;
using Fitness_Tracker_Domain.Entity;
using FluentResults;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Fitness_Tracker_Tests.Application.Features.Authorization
{
    public class AuthorizationTest
    {
        private readonly IMapper _mapper;

        public AuthorizationTest()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<UserMappingProfile>();
                },
                NullLoggerFactory.Instance
            );

            mapperConfig.AssertConfigurationIsValid();

            _mapper = mapperConfig.CreateMapper();
        }

        [Fact]
        public async Task Handle_WhenUserExistsAndPasswordIsCorrect_ShouldReturnSuccess()
        {
            var guid = Guid.CreateVersion7();
            var command = new AuthorizateUserCommand("ExistingUser", "correctPassword");
            var mockUserRepository = new Mock<IUserRepository>();
            var user = new User
            (
                guid,
                "ExistingUser",
                BCrypt.Net.BCrypt.HashPassword("correctPassword")
            );

            mockUserRepository
                .Setup(repo => repo.GetUserByLoginAsync(command.Login, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok(user));


            var handler = new AuthorizateUserCommandHandler(mockUserRepository.Object, _mapper);
            var result = await handler.Handle(command, CancellationToken.None);


            Assert.True(result.IsSuccess);
            Assert.Equal(user.Id, result.Value.Id);
        }

        [Fact]
        public async Task Handle_WhenUserExistsAndPasswordIsNotCorrect_ShouldReturnFailure()
        {
            var guid = Guid.CreateVersion7();
            var command = new AuthorizateUserCommand("ExistingUser", "wrongPassword");
            var mockUserRepository = new Mock<IUserRepository>();
            var user = new User
            (
                guid,
                "ExistingUser",
                BCrypt.Net.BCrypt.HashPassword("correctPassword")
            );

            mockUserRepository
                .Setup(repo => repo.GetUserByLoginAsync(command.Login, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok(user));


            var handler = new AuthorizateUserCommandHandler(mockUserRepository.Object, _mapper);
            var result = await handler.Handle(command, CancellationToken.None);


            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task Handle_WhenUserNotExists_ShouldReturnFailure()
        {
            var guid = Guid.CreateVersion7();
            var command = new AuthorizateUserCommand("NotExistingUser", "wrongPassword");
            var mockUserRepository = new Mock<IUserRepository>();

            mockUserRepository
                .Setup(repo => repo.GetUserByLoginAsync(command.Login, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Fail<User>("User not found"));


            var handler = new AuthorizateUserCommandHandler(mockUserRepository.Object, _mapper);
            var result = await handler.Handle(command, CancellationToken.None);


            Assert.False(result.IsSuccess);
            Assert.Equal("User not found", result.Errors.First().Message);
        }
    }
}
