using Birdie69.Domain.Entities;
using Birdie69.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Birdie69.Infrastructure.Persistence.Repositories;

public sealed class AnswerRepository : Repository<Answer>, IAnswerRepository
{
    private readonly AppDbContext _context;

    public AnswerRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Answer?> GetByUserAndQuestionAsync(
        Guid userId, Guid questionId, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(
            a => a.UserId == userId && a.QuestionId == questionId,
            cancellationToken);

    public async Task<IReadOnlyList<Answer>> GetByQuestionAndCoupleAsync(
        Guid questionId, Guid coupleId, CancellationToken cancellationToken = default)
        => await DbSet
            .Where(a => a.QuestionId == questionId && a.CoupleId == coupleId)
            .ToListAsync(cancellationToken);

    public async Task<bool> BothPartnersAnsweredAsync(
        Guid questionId, Guid coupleId, CancellationToken cancellationToken = default)
        => await DbSet.CountAsync(
            a => a.QuestionId == questionId && a.CoupleId == coupleId,
            cancellationToken) >= 2;

    public async Task<(IReadOnlyList<(Answer MyAnswer, Answer? PartnerAnswer, Question Question)> Items, int TotalCount)>
        GetHistoryByCoupleAsync(Guid coupleId, Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var answersForCouple = DbSet
            .Where(a => a.CoupleId == coupleId)
            .Join(_context.Questions,
                a => a.QuestionId,
                q => q.Id,
                (a, q) => new { Answer = a, Question = q });

        // Group by QuestionId to find all questions answered by at least one partner
        var grouped = await answersForCouple
            .GroupBy(x => x.Question.Id)
            .Select(g => new
            {
                QuestionId = g.Key,
                ScheduledDate = g.First().Question.ScheduledDate,
                MyAnswer = g.Where(x => x.Answer.UserId == userId).Select(x => x.Answer).FirstOrDefault(),
                PartnerAnswer = g.Where(x => x.Answer.UserId != userId).Select(x => x.Answer).FirstOrDefault(),
                Question = g.First().Question
            })
            .OrderByDescending(x => x.ScheduledDate)
            .ToListAsync(cancellationToken);

        var totalCount = grouped.Count;
        var pagedItems = grouped
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(g => (g.MyAnswer!, g.PartnerAnswer, g.Question))
            .ToList();

        return ((IReadOnlyList<(Answer, Answer?, Question)>)pagedItems, totalCount);
    }
}
