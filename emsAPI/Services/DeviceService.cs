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
        if (string.IsNullOrWhiteSpace(device.SerialNumber))
            return false;

        device.SerialNumber = device.SerialNumber.Trim();
        var normalized = device.SerialNumber.ToUpperInvariant();

        var exists = await _context.Devices
            .AnyAsync(d => d.SerialNumber.ToUpper() == normalized);

        if (exists)
            return false;

        try
        {
            _context.Devices.Add(device);
            var created = await _context.SaveChangesAsync();
            return created > 0;
        }
        catch (DbUpdateException)
        {
            return false;
        }
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
        if (string.IsNullOrWhiteSpace(updatedDevice.SerialNumber))
            return false;

        updatedDevice.SerialNumber = updatedDevice.SerialNumber.Trim();
        var normalized = updatedDevice.SerialNumber.ToUpperInvariant();
        
        var device = await _context.Devices.FindAsync(id);
        if (device == null) return false;
        
        var exists = await _context.Devices
            .AnyAsync(d => d.Id != id && d.SerialNumber.ToUpper() == normalized);

        if (exists)
            return false;
        
        device.TypeId = updatedDevice.TypeId;
        device.ProducerId = updatedDevice.ProducerId;
        
        device.SerialNumber = updatedDevice.SerialNumber;
        device.Available = updatedDevice.Available;

        try
        {
            var updated = await _context.SaveChangesAsync();
            return updated > 0;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }
    
    public async Task<List<Device>> ReadAvailable()
    {
        return await _context.Devices
            .Where(d => d.Available)
            .Include(d => d.Type)
            .Include(d => d.Producer)
            .ToListAsync();
    }
    
    public async Task<List<Device>> ReadByProducer(int producerId)
    {
        return await _context.Devices
            .Where(d => d.ProducerId == producerId)
            .Include(d => d.Type)
            .Include(d => d.Producer)
            .ToListAsync();
    }

    public async Task<List<Device>> ReadByType(int typeId)
    {
        return await _context.Devices
            .Where(d => d.TypeId == typeId)
            .Include(d => d.Type)
            .Include(d => d.Producer)
            .ToListAsync();
    }
}