using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackingService.Controllers;
using TrackingService.Data;
using TrackingService.Models;

namespace TrackingService.Tests;

public class SessionsControllerTests
{
    private readonly DbContextOptions<TrackingDbContext> _dbContextOptions;

    public SessionsControllerTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<TrackingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task StopSession_WhenSessionIsActive_ChangesStatusToCompleted_AndCalculatesDuration()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddMinutes(-5);

        await using var context = new TrackingDbContext(_dbContextOptions);
        context.Sessions.Add(new Session 
        { 
            Id = sessionId, 
            UserId = userId, 
            Status = "ACTIVE", 
            StartedAt = startTime 
        });
        await context.SaveChangesAsync();

        var controller = new SessionsController(context, null!);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "mock"));
        
        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };

        var result = await controller.StopSession(sessionId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var session = Assert.IsType<Session>(okResult.Value);
        
        Assert.Equal("COMPLETED", session.Status);
        Assert.NotNull(session.EndedAt);

        Assert.True(session.DurationSeconds >= 299 && session.DurationSeconds <= 301, 
            $"Ожидалось около 300 секунд, но получилось {session.DurationSeconds}");
    }
    
    [Fact]
    public async Task StopSession_WhenSessionAlreadyCompleted_ReturnsError()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        await using var context = new TrackingDbContext(_dbContextOptions);
        context.Sessions.Add(new Session 
        { 
            Id = sessionId, 
            UserId = userId, 
            Status = "COMPLETED",
            StartedAt = DateTime.UtcNow.AddMinutes(-10),
            EndedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var controller = new SessionsController(context, null!);
        
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "mock"));
        
        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };

        var result = await controller.StopSession(sessionId);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.True(objectResult.StatusCode == 400 || objectResult.StatusCode == 409, 
            $"Ожидался статус ошибки (400/409), но получен {objectResult.StatusCode}");
    }

    [Fact]
    public async Task StopSession_WhenSessionNotFound_Returns404()
    {
        await using var context = new TrackingDbContext(_dbContextOptions);
        var controller = new SessionsController(context, null!);

        var result = await controller.StopSession(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }
    
    [Fact]
    public async Task GetActiveSession_ReturnsNotFound_WhenNoActiveSessionExists()
    {
        await using var context = new TrackingDbContext(_dbContextOptions);
        var controller = new SessionsController(context, null!);
        
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        }, "mock"));
        
        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };

        var result = await controller.GetActiveSession();

        Assert.IsType<NotFoundObjectResult>(result);
    }
    
    [Fact]
    public async Task GetActiveSession_ReturnsOkWithSession_WhenActiveSessionExists()
    {
        var userId = Guid.NewGuid();
        await using var context = new TrackingDbContext(_dbContextOptions);
        context.Sessions.Add(new Session 
        { 
            Id = Guid.NewGuid(), 
            UserId = userId, 
            Status = "ACTIVE", 
            StartedAt = DateTime.UtcNow 
        });
        await context.SaveChangesAsync();

        var controller = new SessionsController(context, null!);
        
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "mock"));
        
        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };

        var result = await controller.GetActiveSession();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var session = Assert.IsType<Session>(okResult.Value);
        Assert.Equal("ACTIVE", session.Status);
    }
}