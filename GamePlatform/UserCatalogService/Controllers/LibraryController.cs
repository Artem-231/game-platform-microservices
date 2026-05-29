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

    [HttpPost("me/library/{gameId}")]
public async Task<IActionResult> AddToLibrary(Guid gameId)
{
    int maxRetries = 3;
    bool externalCheckPassed = false;
    
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            externalCheckPassed = SimulateExternalLicenseApiCall();
            break;
        }
        catch (HttpRequestException)
        {
            Console.WriteLine($"[SLA Warning] Попытка {attempt} вызвать External API сорвалась. Повторяем...");
            if (attempt == maxRetries) 
            {
                return StatusCode(503, new { error = "Внешний сервис проверки лицензии временно недоступен. Нарушение SLA внешнего провайдера." });
            }
            await Task.Delay(200);
        }
    }

    var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
    
    var record = new UserCatalogService.Models.LibraryRecord 
    { 
        UserId = userId, 
        GameId = gameId 
    };
    
    _db.LibraryRecords.Add(record);
    await _db.SaveChangesAsync();
    
    return StatusCode(201, new { message = "Игра успешно добавлена в библиотеку после верификации внешней системой" });
}

private bool SimulateExternalLicenseApiCall()
{
    var random = new Random();
    // Имитируем 10% шанс падения сети или таймаута стороннего провайдера
    if (random.Next(1, 11) == 1) 
    {
        throw new HttpRequestException("Таймаут соединения с внешним шлюзом лицензирования.");
    }
    return true; // В 90% случаев всё отлично
}
}