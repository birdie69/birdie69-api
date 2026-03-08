using Birdie69.Domain.Common;
using MediatR;

namespace Birdie69.Application.Features.Couples.Queries.GetCouple;

public sealed record GetCoupleQuery : IRequest<Result<CoupleDto>>;
