using Data;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Services;

public class DeviceTypeService
{
    private readonly AppDbContext _db;

    public DeviceTypeService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<DeviceType>> GetAllAsync()
    {
        return await _db.DeviceTypes
            .Include(dt => dt.Devices)
            .ToListAsync();
    }

    public async Task<DeviceType?> GetByIdAsync(int id)
    {
        return await _db.DeviceTypes
            .Include(dt => dt.Devices)
            .FirstOrDefaultAsync(dt => dt.Id == id);
    }

    public async Task<DeviceType> CreateAsync(DeviceType deviceType)
    {
        _db.DeviceTypes.Add(deviceType);
        await _db.SaveChangesAsync();
        return deviceType;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deviceType = await _db.DeviceTypes.FindAsync(id);
        if (deviceType is null)
            return false;

        _db.DeviceTypes.Remove(deviceType);
        await _db.SaveChangesAsync();
        return true;
    }
}