using Birdie69.Domain.Entities;

namespace Birdie69.Domain.Interfaces;

public interface IAnswerRepository : IRepository<Answer>
{
    Task<Answer?> GetByUserAndQuestionAsync(Guid userId, Guid questionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Answer>> GetByQuestionAndCoupleAsync(Guid questionId, Guid coupleId, CancellationToken cancellationToken = default);
    Task<bool> BothPartnersAnsweredAsync(Guid questionId, Guid coupleId, CancellationToken cancellationToken = default);
}
