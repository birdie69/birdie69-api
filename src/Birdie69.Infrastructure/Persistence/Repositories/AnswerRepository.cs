using Birdie69.Domain.Entities;
using Birdie69.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Birdie69.Infrastructure.Persistence.Repositories;

public sealed class AnswerRepository(AppDbContext context)
    : Repository<Answer>(context), IAnswerRepository
{
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
}
