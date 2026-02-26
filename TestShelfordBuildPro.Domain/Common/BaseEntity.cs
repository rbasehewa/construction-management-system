namespace TestShelfordBuildPro.Domain.Common;

// =====================================================
// BaseEntity — the foundation of EVERY domain object
// =====================================================

// Every single thing in Shelford's
// domain (Project, Client, Milestone) inherits from this.
//
// WHY?
// Because every object in the system needs:
//   1. A unique ID         → so we can find it
//   2. Who created it      → audit trail
//   3. When it was created → audit trail
//   4. Domain Events       → notify other parts of the system
// =====================================================

public abstract class BaseEntity
{
    // ---------------------------------------------------
    // IDENTITY
    // ---------------------------------------------------
    // Guid = a globally unique ID like "3f2504e0-4f89-11d3-9a0c-0305e82c3301"
    // In Angular you'd use a string ID from an API — same concept
    // We generate it immediately so it exists before saving to DB
    public Guid Id { get; protected set; } = Guid.NewGuid();

    // ---------------------------------------------------
    // AUDIT TRAIL
    // ---------------------------------------------------
    // Shelford needs to know: who created this? when? who last changed it?
    // This appears on every contract, every project, every invoice
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public string CreatedBy { get; protected set; } = string.Empty;
    public DateTime? LastModifiedAt { get; protected set; }  // ? means nullable — might not have been modified yet
    public string? LastModifiedBy { get; protected set; }    // ? means nullable

    // ---------------------------------------------------
    // DOMAIN EVENTS
    // ---------------------------------------------------
    // Think of these like Angular EventEmitters.
    // When something important happens (contract signed, project created)
    // we "emit" an event. Other parts of the system listen and react.
    //
    // Private list — only THIS class can add events to it
    private readonly List<IDomainEvent> _domainEvents = new();

    // ReadOnly view — other classes can READ events but not ADD to the list directly
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Child classes call this to fire an event
    // e.g. RaiseDomainEvent(new ProjectCreatedEvent(Id))
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    // Infrastructure layer calls this after it has handled the events
    // so they don't fire again on the next save
    public void ClearDomainEvents()
        => _domainEvents.Clear();

    // Called when updating — stamps who changed it and when
    protected void SetModified(string modifiedBy)
    {
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }
}