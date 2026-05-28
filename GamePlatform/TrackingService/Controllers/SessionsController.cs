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

    // Вот этот метод случайно стерся при прошлом копировании
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
            return Forbid(); // Ошибка 403: Доступ запрещен

        var activeSession = await _db.Sessions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "ACTIVE");
        
        if (activeSession != null)
            return Conflict(new { error_code = "ACTIVE_SESSION_ALREADY_EXISTS" });

        var session = new Session { UserId = userId, GameId = req.GameId };
        _db.Sessions.Add(session);
        await _db.SaveChangesAsync();

        return StatusCode(201, session);
    }

    [HttpPost("{id}/stop")]
    public async Task<IActionResult> StopSession(Guid id)
    {
        var session = await _db.Sessions.FindAsync(id);
        if (session == null || session.UserId != GetUserId()) 
            return NotFound();

        if (session.Status != "ACTIVE") 
            return Conflict(new { error_code = "SESSION_NOT_ACTIVE" });

        session.Status = "COMPLETED";
        session.EndedAt = DateTime.UtcNow;
        session.DurationSeconds = (int)(session.EndedAt.Value - session.StartedAt).TotalSeconds;

        await _db.SaveChangesAsync();
        return Ok(session);
    }
}