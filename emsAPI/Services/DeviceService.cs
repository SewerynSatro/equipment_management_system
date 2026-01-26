using Data;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Services;

public class DeviceService : IDeviceService
{
    private readonly AppDbContext _context;

    public DeviceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Create(Device device)
    {
        await _context.Devices.AddAsync(device);
        var created = await _context.SaveChangesAsync();
        return created > 0;
    }

    public async Task<Device?> ReadOne(int id)
    {
        return await _context.Devices
            .Include(d => d.Type)
            .Include(d => d.Producer)
            .Include(d => d.Loans)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<List<Device>> ReadAll()
    {
        return await _context.Devices
            .Include(d => d.Type)
            .Include(d => d.Producer)
            .Include(d => d.Loans)
            .ToListAsync();
    }

    public async Task<bool> Delete(int id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null) return false;

        _context.Devices.Remove(device);
        var deleted = await _context.SaveChangesAsync();
        return deleted > 0;
    }

    public async Task<bool> Update(int id, Device updatedDevice)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null) return false;
        
        device.TypeId = updatedDevice.TypeId;
        device.ProducerId = updatedDevice.ProducerId;
        
        device.SerialNumber = updatedDevice.SerialNumber;
        device.Available = updatedDevice.Available;

        _context.Devices.Update(device);
        var updated = await _context.SaveChangesAsync();
        return updated > 0;
    }
}