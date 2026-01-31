using Models.Dtos.Device;

namespace Services;

public interface IDeviceService
{
    Task<DeviceReadDto?> Create(DeviceCreateDto dto);
    Task<DeviceReadDto?> ReadOne(int id);
    Task<List<DeviceReadDto>> ReadAll();
    Task<bool> Delete(int id);
    Task<DeviceReadDto?> Update(int id, DeviceUpdateDto dto);
    Task<List<DeviceReadDto>> ReadAvailable();
    Task<List<DeviceReadDto>> ReadByProducer(int producerId);
    Task<List<DeviceReadDto>> ReadByType(int typeId);
}