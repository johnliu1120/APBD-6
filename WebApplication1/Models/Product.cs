using System.ComponentModel.DataAnnotations;

public class Product
{
    [Key]
    public int IdProduct { get; set; }

    [Required]
    [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters.")]
    public string Name { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
    public decimal Price { get; set; }

    [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
    public string Description { get; set; }
}
