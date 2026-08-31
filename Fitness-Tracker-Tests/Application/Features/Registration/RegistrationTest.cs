using AutoMapper;
using Fitness_Tracker_Application.DTO.User;
using Fitness_Tracker_Application.Features.Users.Registration;
using Fitness_Tracker_Application.Mapping;
using Fitness_Tracker_Application.Repository.User;
using Fitness_Tracker_Domain.Entity;
using FluentResults;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Fitness_Tracker_Tests.Application.Features.Registration
{
    public class RegistrationTest
    {
        private readonly IMapper _mapper;

        public RegistrationTest()
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
        public async Task Handle_WhenAllCorrect_ShouldReturnSuccess()
        {
            var command = new RegisterUserCommand("NewUser", "password");
            var mockUserRepository = new Mock<IUserRepository>();

            mockUserRepository
                .Setup(repo => repo.IsLoginAlreadyTakenAsync(command.Login, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            mockUserRepository
                .Setup(repo => repo.AddNewUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok());

            var handler = new RegisterUserCommandHandler(mockUserRepository.Object, _mapper);

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);

            var userDTO = new UserDTO(command.Login, result.Value.Id);

            Assert.Equal(userDTO, result.Value);

            mockUserRepository.Verify(
                repo => repo.AddNewUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenLoginAlreadyExists_ShouldReturnFailureResult()
        {
            var command = new RegisterUserCommand("NewUser", "password");

            var mockUserRepository = new Mock<IUserRepository>();

            mockUserRepository
                .Setup(repo => repo.IsLoginAlreadyTakenAsync(command.Login, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var handler = new RegisterUserCommandHandler(mockUserRepository.Object, _mapper);

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.False(result.IsSuccess);

            Assert.Equal($"User's login {command.Login} is already taken", result.Errors.First().Message);

            mockUserRepository.Verify(
                repo => repo.AddNewUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}