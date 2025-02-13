namespace VetrinaGalaApp.ApiService.Domain.Common;

public abstract class Entity : IHasDomainEvents
{
    private readonly List<IDomianEvent> _domainEvents = [];
    public Guid Id { get; }
    protected Entity(Guid id) => Id = id;
    public IEnumerable<IDomianEvent> DomainEvents => _domainEvents;
    public void AddEvent(IDomianEvent @event) => _domainEvents.Add(@event);
    public IEnumerable<IDomianEvent> PopEvents()
    {
        var domainEvents = _domainEvents;
        _domainEvents.Clear();
        return domainEvents;
    }
}
