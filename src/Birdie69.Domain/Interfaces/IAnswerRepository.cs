using Birdie69.Domain.Entities;

namespace Birdie69.Domain.Interfaces;

public interface IAnswerRepository : IRepository<Answer>
{
    Task<Answer?> GetByUserAndQuestionAsync(Guid userId, Guid questionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Answer>> GetByQuestionAndCoupleAsync(Guid questionId, Guid coupleId, CancellationToken cancellationToken = default);
    Task<bool> BothPartnersAnsweredAsync(Guid questionId, Guid coupleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns paginated answer history for a couple, ordered by question ScheduledDate descending.
    /// Only includes questions where at least one partner answered.
    /// </summary>
    Task<(IReadOnlyList<(Answer MyAnswer, Answer? PartnerAnswer, Question Question)> Items, int TotalCount)>
        GetHistoryByCoupleAsync(Guid coupleId, Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
}
