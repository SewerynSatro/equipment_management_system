using System.ComponentModel.DataAnnotations;

namespace Models;

public class DeviceType
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string? Name { get; set; }
    
    public ICollection<Device> Devices { get; set; } = new List<Device>();
}