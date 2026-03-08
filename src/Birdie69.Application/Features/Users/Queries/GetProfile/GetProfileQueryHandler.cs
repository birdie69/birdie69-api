using AutoMapper;
using Birdie69.Application.Common.Interfaces;
using Birdie69.Domain.Common;
using Birdie69.Domain.Interfaces;
using MediatR;

namespace Birdie69.Application.Features.Users.Queries.GetProfile;

public sealed class GetProfileQueryHandler(
    IUserRepository userRepository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<GetProfileQuery, Result<UserProfileDto>>
{
    public async Task<Result<UserProfileDto>> Handle(
        GetProfileQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdAsync(currentUser.ExternalId, cancellationToken);
        if (user is null)
            return Result.Failure<UserProfileDto>(Error.NotFound("User", currentUser.ExternalId));

        return mapper.Map<UserProfileDto>(user);
    }
}
