namespace TestShelfordBuildPro.Domain.Common;

// =====================================================
// IDomainEvent — marker interface for domain events
// =====================================================
// This is intentionally EMPTY.
// It's just a "label" that says "I am a domain event"
//
// In Angular terms: like an empty interface that you use
// for type checking — if it implements IDomainEvent,
// MediatR (our event dispatcher) knows to handle it
// =====================================================

public interface IDomainEvent
{
    // Intentionally empty — just a marker
}