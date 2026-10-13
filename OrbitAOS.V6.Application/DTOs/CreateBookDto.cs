using System.ComponentModel.DataAnnotations;

namespace OrbitAOS.V6.Application.DTOs
{
    /// <summary>
    /// DTO for creating a new book.
    /// </summary>
    public class CreateBookDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author is required.")]
        [StringLength(150, ErrorMessage = "Author name cannot exceed 150 characters.")]
        public string Author { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "ISBN cannot exceed 20 characters.")]
        public string? ISBN { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 99999.99, ErrorMessage = "Price must be between 0.01 and 99999.99.")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
        public int Quantity { get; set; }

        public int? CategoryId { get; set; }

        public bool IsAvailable { get; set; } = true;

        public DateTime? PublishedDate { get; set; }
    }
}
