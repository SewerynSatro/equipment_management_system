using Data;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Services;

public class DeviceTypeService : IDeviceTypeService
{
    private readonly AppDbContext _context;

    public DeviceTypeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Create(DeviceType deviceType)
    {
        await _context.DeviceTypes.AddAsync(deviceType);
        var created = await _context.SaveChangesAsync();
        return created > 0;
    }

    public async Task<DeviceType?> ReadOne(int id)
    {
        return await _context.DeviceTypes
            .Include(dt => dt.Devices)
            .FirstOrDefaultAsync(dt => dt.Id == id);
    }

    public async Task<List<DeviceType>> ReadAll()
    {
        return await _context.DeviceTypes
            .Include(dt => dt.Devices)
            .ToListAsync();
    }

    public async Task<bool> Delete(int id)
    {
        var deviceType = await _context.DeviceTypes.FindAsync(id);
        if (deviceType == null) return false;

        _context.DeviceTypes.Remove(deviceType);
        var deleted = await _context.SaveChangesAsync();
        return deleted > 0;
    }

    public async Task<bool> Update(int id, DeviceType updatedDeviceType)
    {
        var deviceType = await _context.DeviceTypes.FindAsync(id);
        if (deviceType == null) return false;

        deviceType.Name = updatedDeviceType.Name;

        _context.DeviceTypes.Update(deviceType);
        var updated = await _context.SaveChangesAsync();
        return updated > 0;
    }
}