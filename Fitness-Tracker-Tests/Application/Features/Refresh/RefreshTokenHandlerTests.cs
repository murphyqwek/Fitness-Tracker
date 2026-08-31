using Fitness_Tracker_Application.DTO.User;
using Fitness_Tracker_Application.Features.Users;
using Fitness_Tracker_Application.Features.Users.JWT;
using Fitness_Tracker_Application.Features.Users.Refresh;
using FluentAssertions;
using FluentResults;
using MediatR;
using Moq;
using Xunit;

namespace Fitness_Tracker_Tests.Application.Features.Refresh
{
    public class RefreshTokenHandlerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly RefreshToken _handler;

        public RefreshTokenHandlerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _handler = new RefreshToken(_mediatorMock.Object);
        }

        [Fact]
        public async Task Handle_WhenRefreshTokenIsNull_ShouldReturnFail()
        {
            var command = new RefreshTokenCommand(null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be("Refresh token is missing");
            _mediatorMock.Verify(m => m.Send(It.IsAny<CheckRefreshTokenCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenCheckRefreshTokenFails_ShouldReturnFail()
        {
            var refreshToken = "invalid-or-expired-token";
            var command = new RefreshTokenCommand(refreshToken);

            _mediatorMock
                .Setup(m => m.Send(It.Is<CheckRefreshTokenCommand>(c => c.RefreshToken == refreshToken), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Fail<Guid>("Expired"));

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be("Refresh token is invalid or expired");
            _mediatorMock.Verify(m => m.Send(It.IsAny<GetUserByIdCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenUserNotFound_ShouldReturnFail()
        {
            var refreshToken = "valid-token";
            var userId = Guid.NewGuid();
            var command = new RefreshTokenCommand(refreshToken);

            _mediatorMock
                .Setup(m => m.Send(It.Is<CheckRefreshTokenCommand>(c => c.RefreshToken == refreshToken), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok(userId));

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetUserByIdCommand>(c => c.Id == userId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Fail<UserDTO>("User not found"));

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be("User not found");
            _mediatorMock.Verify(m => m.Send(It.IsAny<GenerateJwtTokenCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenTokenAndUserValid_ShouldRotateTokensAndReturnNewPair()
        {
            var oldRefreshToken = "old-refresh-token";
            var newRefreshToken = "new-refresh-token";
            var newAccessToken = "new-access-jwt-token";
            var userId = Guid.NewGuid();
            var userDto = new UserDTO("john_doe", userId);
            var command = new RefreshTokenCommand(oldRefreshToken);

            _mediatorMock
                .Setup(m => m.Send(It.Is<CheckRefreshTokenCommand>(c => c.RefreshToken == oldRefreshToken), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok(userId));

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetUserByIdCommand>(c => c.Id == userId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok(userDto));

            _mediatorMock
                .Setup(m => m.Send(It.Is<GenerateJwtTokenCommand>(c => c.User == userDto), It.IsAny<CancellationToken>()))
                .ReturnsAsync(newAccessToken);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GenerateRefreshTokenCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(newRefreshToken);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.AccessToken.Should().Be(newAccessToken);
            result.Value.RefreshToken.Should().Be(newRefreshToken);

            _mediatorMock.Verify(m => m.Send(
                It.Is<DeleteRefreshTokenCommand>(c => c.RefreshToken == oldRefreshToken),
                It.IsAny<CancellationToken>()), Times.Once);

            _mediatorMock.Verify(m => m.Send(
                It.Is<AddRefreshTokenCommand>(c => c.RefreshToken == newRefreshToken && c.id == userId && c.expire == TimeSpan.FromDays(7)),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}