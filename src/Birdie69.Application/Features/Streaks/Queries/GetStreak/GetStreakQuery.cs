using Birdie69.Domain.Common;
using MediatR;

namespace Birdie69.Application.Features.Streaks.Queries.GetStreak;

public sealed record GetStreakQuery : IRequest<Result<StreakDto>>;
