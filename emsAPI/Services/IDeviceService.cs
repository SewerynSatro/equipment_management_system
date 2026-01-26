using Models;

namespace Services;

public interface IDeviceService
{
    Task<bool> Create(Device device);
    Task<Device?> ReadOne(int id);
    Task<List<Device>> ReadAll();
    Task<bool> Delete(int id);
    Task<bool> Update(int id, Device updatedDevice);
}