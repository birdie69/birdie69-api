using Birdie69.Domain.Common;
using Birdie69.Domain.Entities;
using Birdie69.Domain.Interfaces;
using MediatR;

namespace Birdie69.Application.Features.Users.Commands.UpsertUser;

public sealed class UpsertUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpsertUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        UpsertUserCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await userRepository.GetByExternalIdAsync(request.ExternalId, cancellationToken);

        if (existing is not null)
        {
            existing.UpdateProfile(request.DisplayName, request.AvatarUrl);
            userRepository.Update(existing);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return existing.Id;
        }

        var user = User.Create(Guid.NewGuid(), request.ExternalId, request.DisplayName, request.AvatarUrl);
        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return user.Id;
    }
}
