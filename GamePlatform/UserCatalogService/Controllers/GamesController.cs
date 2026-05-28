using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserCatalogService.Data;
using UserCatalogService.Models;

namespace UserCatalogService.Controllers;

[ApiController]
[Route("api/v1/games")]
public class GamesController : ControllerBase
{
    private readonly AppDbContext _db;

    public GamesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAllGames()
    {
        return Ok(await _db.Games.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGame(Guid id)
    {
        var game = await _db.Games.FindAsync(id);
        if (game == null) return NotFound(new { error_code = "GAME_NOT_FOUND" });
        return Ok(game);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest req)
    {
        var game = new Game
        {
            Id = Guid.NewGuid(),
            Title = req.Title,
            Genre = req.Genre,
            Developer = req.Developer
        };

        _db.Games.Add(game);
        await _db.SaveChangesAsync();
    
        return StatusCode(201, game);
    }
}