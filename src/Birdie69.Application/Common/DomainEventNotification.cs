using Birdie69.Domain.Common;
using MediatR;

namespace Birdie69.Application.Common;

/// <summary>
/// Wraps a domain event as a MediatR INotification so it can be published
/// without the Domain layer depending on MediatR.
/// </summary>
public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent)
    : INotification
    where TDomainEvent : DomainEvent;
