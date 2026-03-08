using Birdie69.Domain.Entities;
using Birdie69.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Birdie69.Infrastructure.Persistence.Repositories;

public sealed class QuestionRepository(AppDbContext context)
    : Repository<Question>(context), IQuestionRepository
{
    public async Task<Question?> GetByScheduledDateAsync(DateOnly date, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(q => q.ScheduledDate == date, cancellationToken);

    public async Task<Question?> GetTodayAsync(CancellationToken cancellationToken = default)
        => await GetByScheduledDateAsync(DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
}
