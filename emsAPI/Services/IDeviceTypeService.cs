using Models;

namespace Services;

public interface IDeviceTypeService
{
    Task<bool> Create(DeviceType deviceType);
    Task<DeviceType?> ReadOne(int id);
    Task<List<DeviceType>> ReadAll();
    Task<bool> Delete(int id);
    Task<bool> Update(int id, DeviceType updatedDeviceType);
}