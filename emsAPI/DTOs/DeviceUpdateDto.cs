using System.ComponentModel.DataAnnotations;

namespace Models.Dtos.Device;

public class DeviceUpdateDto
{
    [Required]
    public int TypeId { get; set; }

    [Required]
    public int ProducerId { get; set; }

    [Required]
    public bool Available { get; set; }

    [Required]
    [MaxLength(100)]
    public string SerialNumber { get; set; } = null!;
}