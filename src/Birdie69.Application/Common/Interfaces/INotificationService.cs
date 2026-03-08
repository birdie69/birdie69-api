namespace Birdie69.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendPushAsync(string deviceToken, string title, string body, CancellationToken cancellationToken = default);
}
