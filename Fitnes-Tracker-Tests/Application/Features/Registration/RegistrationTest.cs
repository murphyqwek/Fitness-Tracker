using Fintess_Tracker_Application.Features.Users.Registration;
using Fintess_Tracker_Application.Repository.User;
using Fintess_Tracker_Domain.Entity;
using Moq;

namespace Fitness_Tracker_Tests.Application.Features.Registration
{
    public class RegistrationTest
    {
        [Fact]
        public async Task Handle_WhenAllCorrect_ShouldReturnSuccess()
        {
            var command = new RegisterUserCommand("NewUser", "password", "Петр", new DateOnly(2006, 10, 11));
            var mockUserRepository = new Mock<IUserRepository>();

            mockUserRepository
                .Setup(repo => repo.IsLoginAlreadyTakenAsync(command.Login, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var handler = new RegisterUserCommandHandler(mockUserRepository.Object);

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);

            mockUserRepository.Verify(
                repo => repo.AddNewUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenLoginAlreadyExists_ShouldReturnFailureResult()
        {
            var command = new RegisterUserCommand("NewUser", "password", "Петр", new DateOnly(2006, 10, 11));

            var mockUserRepository = new Mock<IUserRepository>();

            mockUserRepository
                .Setup(repo => repo.IsLoginAlreadyTakenAsync(command.Login, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var handler = new RegisterUserCommandHandler(mockUserRepository.Object);

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("User's login is already taken", result.Errors.First().Message);

            mockUserRepository.Verify(
                repo => repo.AddNewUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

    }
}
