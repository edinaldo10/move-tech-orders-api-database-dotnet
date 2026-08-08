using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloudApplication.Models;

[Table("orders")]
public class Order
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string Customer { get; set; } = string.Empty;
    
    public string Status { get; set; } = "open";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public List<Item> Items { get; set; } = new();
}

[Table("items")]
public class Item
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string OrderId { get; set; } = string.Empty;
    
    [Required]
    public string Sku { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public int Quantity { get; set; }
    
    [ForeignKey("OrderId")]
    public Order Order { get; set; } = null!;
}