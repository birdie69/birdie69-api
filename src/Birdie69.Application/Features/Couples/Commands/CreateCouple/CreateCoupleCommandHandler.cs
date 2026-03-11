using Birdie69.Application.Common.Interfaces;
using Birdie69.Domain.Common;
using Birdie69.Domain.Entities;
using Birdie69.Domain.Interfaces;
using MediatR;

namespace Birdie69.Application.Features.Couples.Commands.CreateCouple;

public sealed class CreateCoupleCommandHandler(
    ICoupleRepository coupleRepository,
    IUserRepository userRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCoupleCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreateCoupleCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdAsync(currentUser.ExternalId, cancellationToken);
        if (user is null)
            return Result.Failure<string>(Error.NotFound("User", currentUser.ExternalId));

        var existingCouple = await coupleRepository.GetCurrentCoupleAsync(user.Id, cancellationToken);

        if (existingCouple is not null)
        {
            if (existingCouple.Status == CoupleStatus.Active)
                return Result.Failure<string>(
                    Error.Conflict("Couple.AlreadyExists", "You are already in an active couple."));

            // Pending couple — regenerate the invite code so the old link is invalidated
            existingCouple.RegenerateCode();
            coupleRepository.Update(existingCouple);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return existingCouple.InviteCode.Value;
        }

        var couple = Couple.Create(Guid.NewGuid(), user.Id);
        await coupleRepository.AddAsync(couple, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return couple.InviteCode.Value;
    }
}
