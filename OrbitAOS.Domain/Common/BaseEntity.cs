// ============================================================
// BaseEntity.cs – Domain Layer
// Clean Architecture: All domain entities inherit from this base.
//
// Provides:
//   - Strongly-typed Id (Guid)
//   - Audit fields (CreatedAt, UpdatedAt)
//   - Domain event collection for eventual consistency patterns
// ============================================================

namespace OrbitAOS.Domain.Common
{
    /// <summary>
    /// Base class for all domain entities.
    /// Provides identity, audit timestamps, and domain event support.
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>Primary key – Guid preferred over int for distributed systems.</summary>
        public Guid Id { get; protected set; } = Guid.NewGuid();

        /// <summary>UTC timestamp when the entity was first persisted.</summary>
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

        /// <summary>UTC timestamp of the last update; null until first update.</summary>
        public DateTime? UpdatedAt { get; protected set; }

        // Domain events – raised within the entity, dispatched by the infrastructure layer
        private readonly List<IDomainEvent> _domainEvents = new();

        /// <summary>Read-only collection of uncommitted domain events.</summary>
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        /// <summary>Registers a domain event to be dispatched after persistence.</summary>
        protected void AddDomainEvent(IDomainEvent domainEvent)
            => _domainEvents.Add(domainEvent);

        /// <summary>Clears all domain events after they have been dispatched.</summary>
        public void ClearDomainEvents() => _domainEvents.Clear();

        /// <summary>Updates the UpdatedAt audit timestamp.</summary>
        public void Touch() => UpdatedAt = DateTime.UtcNow;
    }
}
