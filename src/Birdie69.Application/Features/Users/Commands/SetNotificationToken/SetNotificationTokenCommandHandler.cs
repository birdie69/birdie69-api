using Birdie69.Application.Common.Interfaces;
using Birdie69.Domain.Common;
using Birdie69.Domain.Interfaces;
using MediatR;

namespace Birdie69.Application.Features.Users.Commands.SetNotificationToken;

public sealed class SetNotificationTokenCommandHandler(
    IUserRepository userRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SetNotificationTokenCommand, Result>
{
    public async Task<Result> Handle(
        SetNotificationTokenCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdAsync(currentUser.ExternalId, cancellationToken);
        if (user is null)
            return Result.Failure(Error.NotFound("User", currentUser.ExternalId));

        user.SetNotificationToken(request.Token);
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
