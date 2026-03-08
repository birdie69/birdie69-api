using Birdie69.Domain.Common;
using MediatR;

namespace Birdie69.Application.Features.Couples.Commands.CreateCouple;

public sealed record CreateCoupleCommand : IRequest<Result<string>>;
