using Microsoft.EntityFrameworkCore;
using Models;
using Data;

namespace Services;

public class BranchService : IBranchService
{
    private readonly AppDbContext _context;

    public BranchService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Create(Branch branch)
    {
        await _context.Branches.AddAsync(branch);
        var created = await _context.SaveChangesAsync();
        return created > 0;
    }

    public async Task<Branch?> ReadOne(int id)
    {
        return await _context.Branches.FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<List<Branch>> ReadAll()
    {
        return await _context.Branches.ToListAsync();
    }

    public async Task<bool> Delete(int id)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch == null) return false;

        _context.Branches.Remove(branch);
        var deleted = await _context.SaveChangesAsync();
        return deleted > 0;
    }

    public async Task<bool> Update(int id, Branch updatedBranch)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch == null) return false;

        branch.Name = updatedBranch.Name;

        _context.Branches.Update(branch);
        var updated = await _context.SaveChangesAsync();
        return updated > 0;
    }
}