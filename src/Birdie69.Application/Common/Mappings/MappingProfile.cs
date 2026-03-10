using AutoMapper;
using Birdie69.Application.Features.Couples.Queries.GetCouple;
using Birdie69.Application.Features.Answers.Queries.GetAnswers;
using Birdie69.Application.Features.Users.Queries.GetProfile;
using Birdie69.Domain.Entities;

namespace Birdie69.Application.Common.Mappings;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Question → QuestionDto removed: questions come from Strapi via ICmsService,
        // not from the local Question domain entity.
        CreateMap<Couple, CoupleDto>()
            .ForMember(d => d.InviteCode, o => o.MapFrom(s => s.InviteCode.Value));
        CreateMap<Answer, AnswerDto>();
        CreateMap<User, UserProfileDto>();
    }
}
