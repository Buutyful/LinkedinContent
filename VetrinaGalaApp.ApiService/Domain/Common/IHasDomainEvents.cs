using MediatR;

namespace VetrinaGalaApp.ApiService.Domain.Common;
public interface IDomianEvent : INotification;

public interface IHasDomainEvents
{
    IEnumerable<IDomianEvent> DomainEvents { get; }
    void AddEvent(IDomianEvent @event);
    IEnumerable<IDomianEvent> PopEvents();
}
