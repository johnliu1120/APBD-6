using System.ComponentModel.DataAnnotations;

public class WarehouseRequest
{
    [Required(ErrorMessage = "Product ID is required.")]
    public int IdProduct { get; set; }

    [Required(ErrorMessage = "Warehouse ID is required.")]
    public int IdWarehouse { get; set; }

    [Required(ErrorMessage = "Amount is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public int Amount { get; set; }

    [Required(ErrorMessage = "CreatedAt timestamp is required.")]
    public DateTime CreatedAt { get; set; }
}
