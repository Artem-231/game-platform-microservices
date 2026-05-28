namespace TrackingService.Models;

public class Session
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public string Status { get; set; } = "ACTIVE"; 
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public int? DurationSeconds { get; set; }
}

public class Score
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public int Points { get; set; }
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
}

public record StartSessionRequest(Guid GameId);
public record AddScoreRequest(int Points, string Reason);