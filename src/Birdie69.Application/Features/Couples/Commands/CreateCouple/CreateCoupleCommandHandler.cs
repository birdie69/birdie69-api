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

        var alreadyInCouple = await coupleRepository.HasActiveCoupleAsync(user.Id, cancellationToken);
        if (alreadyInCouple)
            return Result.Failure<string>(Error.Conflict("Couple.AlreadyExists", "You are already in an active couple."));

        var couple = Couple.Create(Guid.NewGuid(), user.Id);
        await coupleRepository.AddAsync(couple, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return couple.InviteCode.Value;
    }
}
