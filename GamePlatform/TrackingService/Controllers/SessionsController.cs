using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackingService.Data;
using TrackingService.Models;

namespace TrackingService.Controllers;

[ApiController]
[Route("api/v1/sessions")]
[Authorize]
public class SessionsController : ControllerBase
{
    private readonly TrackingDbContext _db;
    private readonly GamePlatform.Grpc.LibraryChecker.LibraryCheckerClient _grpcClient;

    public SessionsController(TrackingDbContext db, GamePlatform.Grpc.LibraryChecker.LibraryCheckerClient grpcClient)
    {
        _db = db;
        _grpcClient = grpcClient;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpPost("start")]
    public async Task<IActionResult> StartSession([FromBody] StartSessionRequest req)
    {
        var userId = GetUserId();

        var grpcResponse = await _grpcClient.CheckGameInLibraryAsync(new GamePlatform.Grpc.CheckGameRequest 
        { 
            UserId = userId.ToString(), 
            GameId = req.GameId.ToString() 
        });
        
        if (!grpcResponse.HasGame)
            return StatusCode(403, new { error_code = "GAME_NOT_IN_LIBRARY", message = "Этой игры нет в вашей библиотеке." });
        
        await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        try
        {
            var activeSession = await _db.Sessions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "ACTIVE");
            
            if (activeSession != null)
                return Conflict(new { error_code = "ACTIVE_SESSION_ALREADY_EXISTS", message = "У вас уже есть активная сессия." });

            var session = new Session { UserId = userId, GameId = req.GameId };
            _db.Sessions.Add(session);
            await _db.SaveChangesAsync();
            
            await transaction.CommitAsync();
            return StatusCode(201, session);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return Conflict(new { error_code = "CONCURRENT_START_ERROR", message = "Обнаружен параллельный запуск сессии." });
        }
    }

    [HttpPost("{id}/stop")]
    public async Task<IActionResult> StopSession(Guid id)
    {
        var session = await _db.Sessions.FindAsync(id);
        if (session == null || session.UserId != GetUserId()) 
            return NotFound();

        if (session.Status != "ACTIVE") 
            return Conflict(new { error_code = "SESSION_NOT_ACTIVE", message = "Сессия уже завершена." });

        session.Status = "COMPLETED";
        session.EndedAt = DateTime.UtcNow;
        session.DurationSeconds = (int)(session.EndedAt.Value - session.StartedAt).TotalSeconds;

        await _db.SaveChangesAsync();
        return Ok(session);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveSession()
    {
        var userId = GetUserId();
        var session = await _db.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "ACTIVE");

        if (session == null)
            return NotFound(new { error_code = "NO_ACTIVE_SESSION", message = "У вас нет активных игровых сессий." });

        return Ok(session);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetSessionHistory([FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        var userId = GetUserId();
        var history = await _db.Sessions
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.Status == "COMPLETED")
            .OrderByDescending(s => s.EndedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        return Ok(history);
    }
}