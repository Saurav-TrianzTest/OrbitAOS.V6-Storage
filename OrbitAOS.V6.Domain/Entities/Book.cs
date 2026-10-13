using OrbitAOS.V6.Domain.Common;

namespace OrbitAOS.V6.Domain.Entities
{
    /// <summary>
    /// Represents a book in the book management system.
    /// </summary>
    public class Book : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? ISBN { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int? CategoryId { get; set; }
        public bool IsAvailable { get; set; } = true;
        public DateTime? PublishedDate { get; set; }

        // Navigation property
        public Category? Category { get; set; }
    }
}
