using System.ComponentModel.DataAnnotations;

public class Warehouse
{
    [Key]
    public int IdWarehouse { get; set; }

    [Required]
    [StringLength(200, ErrorMessage = "Warehouse name cannot exceed 100 characters.")]
    public string Name { get; set; }

    [Required]
    [StringLength(200, ErrorMessage = "Address cannot exceed 255 characters.")]
    public string Address { get; set; }
}
