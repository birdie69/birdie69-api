using Birdie69.Application.Common.Interfaces;
using Birdie69.Domain.Common;
using Birdie69.Domain.Interfaces;
using MediatR;

namespace Birdie69.Application.Features.Couples.Commands.JoinCouple;

public sealed class JoinCoupleCommandHandler(
    ICoupleRepository coupleRepository,
    IUserRepository userRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<JoinCoupleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        JoinCoupleCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdAsync(currentUser.ExternalId, cancellationToken);
        if (user is null)
            return Result.Failure<Guid>(Error.NotFound("User", currentUser.ExternalId));

        var alreadyInCouple = await coupleRepository.HasActiveCoupleAsync(user.Id, cancellationToken);
        if (alreadyInCouple)
            return Result.Failure<Guid>(Error.Conflict("Couple.AlreadyExists", "You are already in an active couple."));

        var couple = await coupleRepository.GetByInviteCodeAsync(request.InviteCode, cancellationToken);
        if (couple is null)
            return Result.Failure<Guid>(Error.NotFound("Couple", request.InviteCode));

        var result = couple.AcceptInvite(user.Id);
        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        coupleRepository.Update(couple);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return couple.Id;
    }
}
