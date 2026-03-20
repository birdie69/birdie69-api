using Birdie69.Application.Common.Interfaces;
using Birdie69.Domain.Common;
using Birdie69.Domain.Interfaces;
using MediatR;

namespace Birdie69.Application.Features.Streaks.Queries.GetStreak;

public sealed class GetStreakQueryHandler(
    IStreakRepository streakRepository,
    IUserRepository userRepository,
    ICurrentUser currentUser)
    : IRequestHandler<GetStreakQuery, Result<StreakDto>>
{
    public async Task<Result<StreakDto>> Handle(
        GetStreakQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdAsync(currentUser.ExternalId, cancellationToken);
        if (user is null)
            return Result.Failure<StreakDto>(Error.NotFound("User", currentUser.ExternalId));

        var streak = await streakRepository.GetByUserIdAsync(user.Id, cancellationToken);

        if (streak is null)
            return Result.Success(new StreakDto(0, 0, null));

        return Result.Success(new StreakDto(
            streak.CurrentCount,
            streak.LongestCount,
            streak.LastActivityDate == default
                ? null
                : streak.LastActivityDate.ToString("yyyy-MM-dd")));
    }
}
