using Fitness_Tracker.Controllers.Authorization;
using Fitness_Tracker.Services;
using Fitness_Tracker_Application.DTO.User;
using Fitness_Tracker_Application.Features.Users.Authorization;
using Fitness_Tracker_Application.Features.Users.JWT;
using Fitness_Tracker_Application.Features.Users.Refresh;
using Fitness_Tracker_Application.Features.Users.Registration;
using FluentAssertions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Moq;

namespace Fitness_Tracker_Tests.Application.Features.Refresh;

public class RefreshSessionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LoginAndRegistration_ShouldStoreRefreshTokenForSevenDays(bool register)
    {
        var user = new UserDTO("user", Guid.NewGuid());
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<AuthorizateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(user));
        mediator.Setup(m => m.Send(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(user));
        mediator.Setup(m => m.Send(It.IsAny<GenerateRefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("refresh-token");
        mediator.Setup(m => m.Send(It.IsAny<GenerateJwtTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("access-token");
        var context = new ControllerContext { HttpContext = new DefaultHttpContext() };

        if (register)
        {
            var controller = new RegisterController(mediator.Object) { ControllerContext = context };
            await controller.RegisterUser(new RegisterUserCommand("user", "password"));
        }
        else
        {
            var controller = new AuthorizationController(mediator.Object) { ControllerContext = context };
            await controller.Login(new AuthorizateUserCommand("user", "password"));
        }

        mediator.Verify(m => m.Send(
            It.Is<AddRefreshTokenCommand>(c => c.RefreshToken == "refresh-token"
                && c.id == user.Id && c.expire == TimeSpan.FromDays(7)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void RefreshCookie_ShouldPersistForSevenDays_WithRestrictedAccess()
    {
        var context = new DefaultHttpContext();
        var before = DateTimeOffset.UtcNow;

        CookiesHelper.SetRefreshTokenCookie(context.Response, "refresh-token");

        var cookie = SetCookieHeaderValue.Parse(context.Response.Headers.SetCookie.ToString());
        cookie.Name.ToString().Should().Be("refreshToken");
        cookie.Value.ToString().Should().Be("refresh-token");
        cookie.MaxAge.Should().Be(TimeSpan.FromDays(7));
        cookie.Expires.Should().NotBeNull();
        cookie.Expires!.Value.Should().BeOnOrAfter(before.AddDays(7).AddSeconds(-1));
        cookie.Expires.Value.Should().BeOnOrBefore(DateTimeOffset.UtcNow.AddDays(7));
        cookie.Path.ToString().Should().Be("/api/v1/auth/refresh");
        cookie.HttpOnly.Should().BeTrue();
        cookie.Secure.Should().BeTrue();
        cookie.SameSite.Should().Be(Microsoft.Net.Http.Headers.SameSiteMode.Strict);
    }
}
