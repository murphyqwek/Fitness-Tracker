using Fintess_Tracker_Analytics.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fintess_Tracker_Analytics.Controllers
{
    [Route("api/analytics/me")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly AnalyticService _analyticsService;

        public AnalyticsController(AnalyticService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [Authorize]
        [HttpGet("monthly-volume")]
        public async Task<IActionResult> GetMonthlyVolume()
        {
            var userId = User.GetUserId();
            var now = DateTimeOffset.UtcNow;

            var result = await _analyticsService.GetMonthlyVolumeAsync(userId, now.Year, now.Month, new CancellationToken());


            return result.IsSuccess ? Ok(result.Value) : StatusCode(StatusCodes.Status500InternalServerError, result.Errors);
        }

        [Authorize]
        [HttpGet("weekly-record")]
        public async Task<IActionResult> GetWeeklyRecord(
    CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();

            var today = DateOnly.FromDateTime(
                DateTimeOffset.UtcNow.UtcDateTime);

            var daysFromMonday = ((int)today.DayOfWeek + 6) % 7;
            var weekStart = today.AddDays(-daysFromMonday);

            var result = await _analyticsService.GetWeeklyRecordAsync(
                userId,
                weekStart,
                cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : StatusCode(
                    StatusCodes.Status500InternalServerError,
                    result.Errors);
        }
    }
}
