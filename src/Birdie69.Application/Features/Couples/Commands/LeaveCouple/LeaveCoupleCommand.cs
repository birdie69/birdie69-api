using Birdie69.Domain.Common;
using MediatR;

namespace Birdie69.Application.Features.Couples.Commands.LeaveCouple;

/// <summary>
/// Leaves or cancels the caller's current couple.
/// - Pending + caller is Initiator → Cancel
/// - Active + caller is member → Disband
/// </summary>
public sealed record LeaveCoupleCommand : IRequest<Result>;
