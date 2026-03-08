using AutoMapper;
using Birdie69.Application.Common.Interfaces;
using Birdie69.Domain.Common;
using Birdie69.Domain.Interfaces;
using MediatR;

namespace Birdie69.Application.Features.Couples.Queries.GetCouple;

public sealed class GetCoupleQueryHandler(
    ICoupleRepository coupleRepository,
    IUserRepository userRepository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<GetCoupleQuery, Result<CoupleDto>>
{
    public async Task<Result<CoupleDto>> Handle(
        GetCoupleQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdAsync(currentUser.ExternalId, cancellationToken);
        if (user is null)
            return Result.Failure<CoupleDto>(Error.NotFound("User", currentUser.ExternalId));

        var couple = await coupleRepository.GetActiveByUserIdAsync(user.Id, cancellationToken);
        if (couple is null)
            return Result.Failure<CoupleDto>(Error.NotFound("Couple", user.Id));

        return mapper.Map<CoupleDto>(couple);
    }
}
