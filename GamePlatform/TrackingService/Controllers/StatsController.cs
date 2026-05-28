using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackingService.Data;
using TrackingService.Models;

namespace TrackingService.Controllers;

[ApiController]
[Route("api/v1")]
public class StatsController : ControllerBase
{
    private readonly TrackingDbContext _db;

    public StatsController(TrackingDbContext db) => _db = db;

    [HttpPost("internal/sessions/{sessionId}/score")]
    public async Task<IActionResult> AddScore(Guid sessionId, [FromBody] AddScoreRequest req)
    {
        var session = await _db.Sessions.FindAsync(sessionId);
        if (session == null || session.Status != "ACTIVE") return BadRequest(new { error_code = "INVALID_SESSION" });

        var score = new Score { SessionId = sessionId, UserId = session.UserId, GameId = session.GameId, Points = req.Points };
        _db.Scores.Add(score);
        await _db.SaveChangesAsync();
        return StatusCode(201, score);
    }

    [HttpGet("games/{gameId}/leaderboard")]
    public async Task<IActionResult> GetLeaderboard(Guid gameId, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        var leaderboard = await _db.Scores
            .Where(s => s.GameId == gameId)
            .GroupBy(s => s.UserId)
            .Select(g => new { UserId = g.Key, TotalPoints = g.Sum(s => s.Points) })
            .OrderByDescending(x => x.TotalPoints)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        return Ok(new { game_id = gameId, limit, offset, items = leaderboard });
    }
}