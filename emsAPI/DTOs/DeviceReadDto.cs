namespace Models.Dtos.Device;

public class DeviceReadDto
{
    public int Id { get; set; }
    public int TypeId { get; set; }
    public int ProducerId { get; set; }
    public bool Available { get; set; }
    public string SerialNumber { get; set; } = null!;
    
    public string? TypeName { get; set; }
    public string? ProducerName { get; set; }
}