using GamePlatform.Grpc;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using UserCatalogService.Data;

namespace UserCatalogService.Services;

public class LibraryGrpcService : LibraryChecker.LibraryCheckerBase
{
    private readonly AppDbContext _db;

    public LibraryGrpcService(AppDbContext db) => _db = db;

    public override async Task<CheckGameResponse> CheckGameInLibrary(CheckGameRequest request, ServerCallContext context)
    {
        var hasGame = await _db.LibraryRecords.AnyAsync(l => 
            l.UserId == Guid.Parse(request.UserId) && 
            l.GameId == Guid.Parse(request.GameId));

        return new CheckGameResponse { HasGame = hasGame };
    }
}