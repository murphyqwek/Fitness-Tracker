using Fitness_Tracker_Application.DTO.User;
using Fitness_Tracker_Application.Features.Users.Infomration;
using Fitness_Tracker_Application.Repository.User;
using FluentAssertions;
using FluentResults;
using Moq;
using Xunit;

namespace Fitness_Tracker_Tests.Application.Features.Users
{
    public class UserInformationHandlerTests
    {
        private readonly Mock<IUserInformationRepository> _userRepoMock;

        public UserInformationHandlerTests()
        {
            _userRepoMock = new Mock<IUserInformationRepository>();
        }

        [Fact]
        public async Task GetUserInformation_WhenUserFound_ShouldReturnSuccess()
        {
            var userId = Guid.NewGuid();
            var expectedDto = new UserInformationDTO("user_login", "Alex", new DateOnly(1995, 5, 20), 180, 78.5m);

            _userRepoMock
                .Setup(r => r.GetUserInformationAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok(expectedDto));

            var handler = new GetUserInformationCommandHandler(_userRepoMock.Object);
            var command = new GetUserInformationCommand(userId);

            var result = await handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(expectedDto);
        }

        [Fact]
        public async Task GetUserInformation_WhenUserNotFound_ShouldReturnFail()
        {
            var userId = Guid.NewGuid();
            _userRepoMock
                .Setup(r => r.GetUserInformationAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Fail<UserInformationDTO>($"No user with id: {userId}"));

            var handler = new GetUserInformationCommandHandler(_userRepoMock.Object);
            var command = new GetUserInformationCommand(userId);

            var result = await handler.Handle(command, CancellationToken.None);

            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Contain(userId.ToString());
        }

        [Fact]
        public async Task UpdateUserInformation_WhenUpdateSuccessful_ShouldReturnSuccess()
        {
            var userId = Guid.NewGuid();
            var updateDto = new UserUpdateDTO("Alexander", new DateOnly(1995, 5, 20), 182, 80.0m);

            _userRepoMock
                .Setup(r => r.UpdateUserInformationAsync(userId, updateDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok());

            var handler = new UpdateUserInformationCommandHandler(_userRepoMock.Object);
            var command = new UpdateUserInformationCommand(userId, updateDto);

            var result = await handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _userRepoMock.Verify(r => r.UpdateUserInformationAsync(userId, updateDto, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}