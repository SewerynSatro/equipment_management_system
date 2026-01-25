using Microsoft.EntityFrameworkCore;
using Models;
using Data;

namespace Services;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _context;

    public EmployeeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Create(Employee employee)
    {
        await _context.Employees.AddAsync(employee);
        var created = await _context.SaveChangesAsync();
        return created > 0;
    }

    public async Task<Employee?> ReadOne(int id)
    {
        return await _context.Employees
            .Include(e => e.Branch)
            .Include(e => e.Loans)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<List<Employee>> ReadAll()
    {
        return await _context.Employees
            .Include(e => e.Branch)
            .Include(e => e.Loans)
            .ToListAsync();
    }

    public async Task<bool> Delete(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return false;

        _context.Employees.Remove(employee);
        var deleted = await _context.SaveChangesAsync();
        return deleted > 0;
    }

    public async Task<bool> Update(int id, Employee updatedEmployee)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return false;

        employee.Name = updatedEmployee.Name;
        employee.LastName = updatedEmployee.LastName;
        employee.Email = updatedEmployee.Email;
        employee.BranchId = updatedEmployee.BranchId;

        _context.Employees.Update(employee);
        var updated = await _context.SaveChangesAsync();
        return updated > 0;
    }
}