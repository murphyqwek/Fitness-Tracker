using Fintess_Tracker_Infrastructure.Data;
using Fintess_Tracker_Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fintes_Tracker.Controllers
{
    [Route("api/fitnes")]
    [ApiController]
    public class FitnesControler : ControllerBase
    {
        private readonly FintessDbContext _dbContext;
        public FitnesControler(FintessDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var fitneses = await _dbContext.FitnesTests.ToListAsync();

            return Ok(fitneses);
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            Random rnd = new Random();

            string muscle = rnd.Next(0, 3) switch
            {
                0 => "Chest",
                1 => "Back",
                2 => "Legs",
                _ => "Unknown"
            };

            var fitnes = new Fitnes()
            {
                Muscle = muscle,
                Name = $"{muscle} Training",
                Repations = rnd.Next(1, 20),
                MoodId = rnd.Next(0, 5)
            };

            await _dbContext.FitnesTests.AddAsync(fitnes);
            await _dbContext.SaveChangesAsync();

            return Ok(fitnes);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            _dbContext.FitnesTests.RemoveRange(_dbContext.FitnesTests);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
