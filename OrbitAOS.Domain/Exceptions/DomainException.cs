// ============================================================
// DomainException.cs – Domain Layer
// Base exception for all domain-level business rule violations.
// Throw this (or a derived type) when a domain invariant is broken.
// ============================================================

namespace OrbitAOS.Domain.Exceptions
{
    /// <summary>
    /// Represents a domain-level business rule violation.
    /// These exceptions should be caught at the Application layer
    /// and translated into user-friendly error responses.
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }

        public DomainException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
