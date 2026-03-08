using Birdie69.Domain.Common;
using MediatR;

namespace Birdie69.Application.Features.Users.Queries.GetProfile;

public sealed record GetProfileQuery : IRequest<Result<UserProfileDto>>;
