using Birdie69.Domain.Entities;

namespace Birdie69.Domain.Interfaces;

public interface IQuestionRepository : IRepository<Question>
{
    Task<Question?> GetByScheduledDateAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<Question?> GetTodayAsync(CancellationToken cancellationToken = default);
}
