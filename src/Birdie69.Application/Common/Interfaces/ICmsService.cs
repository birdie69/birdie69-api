using Birdie69.Application.Features.Questions.Queries.GetTodayQuestion;

namespace Birdie69.Application.Common.Interfaces;

/// <summary>
/// Reads content from Strapi CMS (read-only).
/// The API never writes to Strapi — content is managed by the content team via the CMS admin.
/// </summary>
public interface ICmsService
{
    Task<QuestionDto?> GetTodayQuestionAsync(DateOnly date, CancellationToken ct = default);
}
