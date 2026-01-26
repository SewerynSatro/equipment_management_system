using System.ComponentModel.DataAnnotations;

namespace Models;

public class Producer
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    
    
    public ICollection<Device> Device { get; set; } = new List<Device>();
}gi