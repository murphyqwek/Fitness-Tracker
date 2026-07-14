using Fitness_Tracker_Application.Features.Users.JWT;
using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fitness_Tracker_Application.Features.Users.Refresh
{
    public record RefreshTokenCommand(string? RefreshToken) : IRequest<Result<RefreshTokenResponse>>;

    public record RefreshTokenResponse(string AccessToken, string RefreshToken);

    public class RefreshToken : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>> 
    {
        private readonly IMediator _mediator;

        public RefreshToken(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var refreshToken = request.RefreshToken;

            if (refreshToken == null)
            {
                return Result.Fail<RefreshTokenResponse>("Refresh token is missing");
            }

            var resultGuid = await _mediator.Send(new CheckRefreshTokenCommand(refreshToken));

            if (resultGuid.IsFailed)
            {
                return Result.Fail<RefreshTokenResponse>("Refresh token is invalid or expired");
            }

            var userResult = await _mediator.Send(new GetUserByIdCommand(resultGuid.Value));

            if (userResult.IsFailed)
            {
                return Result.Fail<RefreshTokenResponse>("User not found");
            }

            var accessToken = await _mediator.Send(new GenerateJwtTokenCommand(userResult.Value));
            var newRefreshToken = await _mediator.Send(new GenerateRefreshTokenCommand());

            await _mediator.Send(new AddRefreshTokenCommand(newRefreshToken, userResult.Value.Id, TimeSpan.FromHours(24)));
            

            return Result.Ok(new RefreshTokenResponse(accessToken, newRefreshToken));
        }
    }
}
