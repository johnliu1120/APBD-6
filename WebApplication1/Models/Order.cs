using System.ComponentModel.DataAnnotations;

public class Order
{
    [Key]
    public int IdOrder { get; set; }

    [Required]
    public int IdProduct { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public int Amount { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public DateTime? FulfilledAt { get; set; }
}
