using System.ComponentModel.DataAnnotations;

public class ProductWarehouse
{
    [Key]
    public int IdProductWarehouse { get; set; }

    [Required]
    public int IdProduct { get; set; } 

    [Required]
    public int IdWarehouse { get; set; } 

    [Required]
    public int IdOrder { get; set; } 

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public int Amount { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
    public decimal Price { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }
}
