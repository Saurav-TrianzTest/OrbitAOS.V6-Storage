// ============================================================
// IDomainEvent.cs – Domain Layer
// Marker interface for all domain events.
// Domain events are raised inside entities and dispatched by
// the infrastructure layer after the unit of work is committed.
// ============================================================

namespace OrbitAOS.Domain.Common
{
    /// <summary>
    /// Marker interface for domain events.
    /// Implement this interface for every domain event in the system.
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>UTC timestamp when the event occurred.</summary>
        DateTime OccurredOn { get; }
    }
}
