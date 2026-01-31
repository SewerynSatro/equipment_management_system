using Data;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Dtos.Device;

namespace Services;

public class DeviceService : IDeviceService
{
    private readonly AppDbContext _context;

    public DeviceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DeviceReadDto?> Create(DeviceCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.SerialNumber))
            return null;

        var serialNumber = dto.SerialNumber.Trim();
        var normalized = serialNumber.ToUpperInvariant();

        var exists = await _context.Devices
            .AnyAsync(d => d.SerialNumber.ToUpper() == normalized);

        if (exists)
            return null;

        try
        {
            var device = DeviceMapper.ToEntity(dto);
            device.SerialNumber = serialNumber;
            
            _context.Devices.Add(device);
            await _context.SaveChangesAsync();
            
            // Załaduj powiązane encje dla pełnego DTO
            await _context.Entry(device).Reference(d => d.Type).LoadAsync();
            await _context.Entry(device).Reference(d => d.Producer).LoadAsync();
            
            return DeviceMapper.ToReadDto(device);
        }
        catch (DbUpdateException)
        {
            return null;
        }
    }

    public async Task<DeviceReadDto?> ReadOne(int id)
    {
        var device = await _context.Devices
            .Include(d => d.Type)
            .Include(d => d.Producer)
            .Include(d => d.Loans)
            .FirstOrDefaultAsync(d => d.Id == id);
            
        return device == null ? null : DeviceMapper.ToReadDto(device);
    }

    public async Task<List<DeviceReadDto>> ReadAll()
    {
        var devices = await _context.Devices
            .Include(d => d.Type)
            .Include(d => d.Producer)
            .Include(d => d.Loans)
            .ToListAsync();
            
        return DeviceMapper.ToReadDtoList(devices);
    }

    public async Task<bool> Delete(int id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null) return false;

        _context.Devices.Remove(device);
        var deleted = await _context.SaveChangesAsync();
        return deleted > 0;
    }

    public async Task<DeviceReadDto?> Update(int id, DeviceUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.SerialNumber))
            return null;

        var serialNumber = dto.SerialNumber.Trim();
        var normalized = serialNumber.ToUpperInvariant();
        
        var device = await _context.Devices.FindAsync(id);
        if (device == null) return null;
        
        var exists = await _context.Devices
            .AnyAsync(d => d.Id != id && d.SerialNumber.ToUpper() == normalized);

        if (exists)
            return null;
        
        DeviceMapper.UpdateEntity(device, dto);
        device.SerialNumber = serialNumber;

        try
        {
            await _context.SaveChangesAsync();
            
            // Załaduj powiązane encje dla pełnego DTO
            await _context.Entry(device).Reference(d => d.Type).LoadAsync();
            await _context.Entry(device).Reference(d => d.Producer).LoadAsync();
            
            return DeviceMapper.ToReadDto(device);
        }
        catch (DbUpdateException)
        {
            return null;
        }
    }
    
    public async Task<List<DeviceReadDto>> ReadAvailable()
    {
        var devices = await _context.Devices
            .Where(d => d.Available)
            .Include(d => d.Type)
            .Include(d => d.Producer)
            .ToListAsync();
            
        return DeviceMapper.ToReadDtoList(devices);
    }
    
    public async Task<List<DeviceReadDto>> ReadByProducer(int producerId)
    {
        var devices = await _context.Devices
            .Where(d => d.ProducerId == producerId)
            .Include(d => d.Type)
            .Include(d => d.Producer)
            .ToListAsync();
            
        return DeviceMapper.ToReadDtoList(devices);
    }

    public async Task<List<DeviceReadDto>> ReadByType(int typeId)
    {
        var devices = await _context.Devices
            .Where(d => d.TypeId == typeId)
            .Include(d => d.Type)
            .Include(d => d.Producer)
            .ToListAsync();
            
        return DeviceMapper.ToReadDtoList(devices);
    }
}