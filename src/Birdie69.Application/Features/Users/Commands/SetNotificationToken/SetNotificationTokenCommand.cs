using Birdie69.Domain.Common;
using MediatR;

namespace Birdie69.Application.Features.Users.Commands.SetNotificationToken;

public sealed record SetNotificationTokenCommand(string Token) : IRequest<Result>;
