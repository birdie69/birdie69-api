using Birdie69.Application.Common.Interfaces;
using Birdie69.Domain.Common;
using Birdie69.Domain.Interfaces;
using MediatR;

namespace Birdie69.Application.Features.Couples.Commands.SetNotificationTime;

public sealed class SetNotificationTimeCommandHandler(
    ICoupleRepository coupleRepository,
    IUserRepository userRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SetNotificationTimeCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        SetNotificationTimeCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdAsync(currentUser.ExternalId, cancellationToken);
        if (user is null)
            return Result.Failure<string>(Error.NotFound("User", currentUser.ExternalId));

        var couple = await coupleRepository.GetCurrentCoupleAsync(user.Id, cancellationToken);
        if (couple is null)
            return Result.Failure<string>(Error.NotFound("Couple.NotFound", "You are not in a couple."));

        if (!couple.IsMember(user.Id))
            return Result.Failure<string>(Error.Unauthorized("Couple.NotMember", "You are not a member of this couple."));

        var time = TimeOnly.Parse(request.Time);
        couple.SetNotificationTime(time);
        coupleRepository.Update(couple);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(couple.NotificationTime.ToString("HH:mm"));
    }
}
