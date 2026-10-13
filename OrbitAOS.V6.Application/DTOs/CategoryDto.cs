namespace OrbitAOS.V6.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object for Category entity.
    /// </summary>
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int BookCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
