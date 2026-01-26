using Data;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Services;

public class ProducerService : IProducerService
{
    private readonly AppDbContext _context;

    public ProducerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Create(Producer producer)
    {
        if (string.IsNullOrWhiteSpace(producer.Name))
            return false;

        var normalized = producer.Name.Trim().ToLower();

        var exists = await _context.Producers
            .AnyAsync(p => p.Name.ToLower() == normalized);

        if (exists)
            return false;

        producer.Name = producer.Name.Trim();

        await _context.Producers.AddAsync(producer);
        var created = await _context.SaveChangesAsync();
        return created > 0;
    }

    public async Task<Producer?> ReadOne(int id)
    {
        return await _context.Producers
            .Include(p => p.Devices)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Producer>> ReadAll()
    {
        return await _context.Producers
            .Include(p => p.Devices)
            .ToListAsync();
    }

    public async Task<bool> Delete(int id)
    {
        var producer = await _context.Producers.FindAsync(id);
        if (producer == null) return false;

        _context.Producers.Remove(producer);
        var deleted = await _context.SaveChangesAsync();
        return deleted > 0;
    }

    public async Task<bool> Update(int id, Producer updatedProducer)
    {
        var producer = await _context.Producers.FindAsync(id);
        if (producer == null) return false;
        
        producer.Name = updatedProducer.Name;

        _context.Producers.Update(producer);
        var updated = await _context.SaveChangesAsync();
        return updated > 0;
    }
}