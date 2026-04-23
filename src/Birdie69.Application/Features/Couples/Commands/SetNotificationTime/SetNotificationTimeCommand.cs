using Birdie69.Domain.Common;
using MediatR;

namespace Birdie69.Application.Features.Couples.Commands.SetNotificationTime;

public sealed record SetNotificationTimeCommand(string Time) : IRequest<Result<string>>;
