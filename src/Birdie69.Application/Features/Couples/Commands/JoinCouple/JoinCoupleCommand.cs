using Birdie69.Domain.Common;
using MediatR;

namespace Birdie69.Application.Features.Couples.Commands.JoinCouple;

public sealed record JoinCoupleCommand(string InviteCode) : IRequest<Result<Guid>>;
