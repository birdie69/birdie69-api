using Birdie69.Application.Common.Interfaces;
using Birdie69.Domain.Common;
using Birdie69.Domain.Entities;
using Birdie69.Domain.Interfaces;
using MediatR;

namespace Birdie69.Application.Features.Couples.Commands.LeaveCouple;

public sealed class LeaveCoupleCommandHandler(
    ICoupleRepository coupleRepository,
    IUserRepository userRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LeaveCoupleCommand, Result>
{
    public async Task<Result> Handle(LeaveCoupleCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdAsync(currentUser.ExternalId, cancellationToken);
        if (user is null)
            return Result.Failure(Error.NotFound("User", currentUser.ExternalId));

        var couple = await coupleRepository.GetCurrentCoupleAsync(user.Id, cancellationToken);
        if (couple is null)
            return Result.Failure(Error.NotFound("Couple", user.Id));

        // Delegate the permission/state check to the domain method so error types are consistent.
        Result leaveResult = couple.Status switch
        {
            CoupleStatus.Pending                          => couple.Cancel(user.Id),
            CoupleStatus.Active when couple.IsMember(user.Id) => couple.Disband(),
            _ => Result.Failure(Error.Conflict(
                "Couple.InvalidOperation",
                "You cannot leave the couple in its current state."))
        };

        if (leaveResult.IsFailure)
            return leaveResult;

        coupleRepository.Update(couple);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
