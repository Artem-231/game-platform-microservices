using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserCatalogService.Data;
using UserCatalogService.Models;

namespace UserCatalogService.Controllers;

[ApiController]
[Route("api/v1/users/me/library")]
[Authorize] // Защищаем ручки токеном
public class LibraryController : ControllerBase
{
    private readonly AppDbContext _db;

    public LibraryController(AppDbContext db) => _db = db;

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetLibrary()
    {
        var userId = GetUserId();
        var games = await _db.LibraryRecords
            .Where(lr => lr.UserId == userId)
            .Join(_db.Games, lr => lr.GameId, g => g.Id, (lr, g) => g)
            .ToListAsync();
        return Ok(games);
    }

    [HttpPost("{gameId}")]
    public async Task<IActionResult> AddToLibrary(Guid gameId)
    {
        var userId = GetUserId();
        
        if (!await _db.Games.AnyAsync(g => g.Id == gameId))
            return NotFound(new { error_code = "GAME_NOT_FOUND" });

        var exists = await _db.LibraryRecords.AnyAsync(lr => lr.UserId == userId && lr.GameId == gameId);
        if (exists)
            return Conflict(new { error_code = "GAME_ALREADY_IN_LIBRARY" });

        _db.LibraryRecords.Add(new LibraryRecord { UserId = userId, GameId = gameId });
        await _db.SaveChangesAsync();
        
        return StatusCode(201, new { message = "Game added to library" });
    }
}