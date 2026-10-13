using OrbitAOS.V6.Domain.Common;

namespace OrbitAOS.V6.Domain.Entities
{
    /// <summary>
    /// Represents a book category in the book management system.
    /// </summary>
    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
