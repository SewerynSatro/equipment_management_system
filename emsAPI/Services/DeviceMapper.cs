using Models;
using Models.Dtos.Device;

namespace Services;

public static class DeviceMapper
{
    public static DeviceReadDto ToReadDto(Device device)
    {
        return new DeviceReadDto
        {
            Id = device.Id,
            TypeId = device.TypeId,
            ProducerId = device.ProducerId,
            Available = device.Available,
            SerialNumber = device.SerialNumber,
            TypeName = device.Type?.Name,
            ProducerName = device.Producer?.Name
        };
    }
    
    public static List<DeviceReadDto> ToReadDtoList(IEnumerable<Device> devices)
    {
        return devices.Select(ToReadDto).ToList();
    }
    
    public static Device ToEntity(DeviceCreateDto dto)
    {
        return new Device
        {
            TypeId = dto.TypeId,
            ProducerId = dto.ProducerId,
            Available = dto.Available,
            SerialNumber = dto.SerialNumber
        };
    }
    
    public static void UpdateEntity(Device device, DeviceUpdateDto dto)
    {
        device.TypeId = dto.TypeId;
        device.ProducerId = dto.ProducerId;
        device.Available = dto.Available;
        device.SerialNumber = dto.SerialNumber;
    }
}