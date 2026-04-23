namespace Birdie69.Application.Features.Streaks.Queries.GetStreak;

public sealed record StreakDto(
    int CurrentStreak,
    int LongestStreak,
    string? LastActivityDate);
