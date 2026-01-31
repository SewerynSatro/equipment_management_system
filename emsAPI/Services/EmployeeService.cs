using Microsoft.EntityFrameworkCore;
using Models;
using Data;
using DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Services;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _context;

    public EmployeeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeReadDto?> Create(EmployeeCreateDto dto)
    {
        var employee = new Employee
        {
            Name = dto.Name,
            LastName = dto.LastName,
            Email = dto.Email,
            BranchId = dto.BranchId
        };

        await _context.Employees.AddAsync(employee);
        var created = await _context.SaveChangesAsync();
        if (created == 0) return null;

        await _context.Entry(employee).Reference(e => e.Branch).LoadAsync();

        return new EmployeeReadDto
        {
            Name = employee.Name,
            LastName = employee.LastName,
            Email = employee.Email,
            BranchName = employee.Branch?.Name ?? string.Empty
        };
    }

    public async Task<EmployeeReadDto?> ReadOne(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.Branch)
            .Include(e => e.Loans)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null) return null;

        return new EmployeeReadDto
        {
            Name = employee.Name,
            LastName = employee.LastName,
            Email = employee.Email,
            BranchName = employee.Branch?.Name ?? string.Empty
        };
    }

    public async Task<List<EmployeeReadDto>> ReadAll()
    {
        var employees = await _context.Employees
            .Include(e => e.Branch)
            .Include(e => e.Loans)
            .ToListAsync();

        return employees.Select(e => new EmployeeReadDto
        {
            Name = e.Name,
            LastName = e.LastName,
            Email = e.Email,
            BranchName = e.Branch?.Name ?? string.Empty
        }).ToList();
    }

    public async Task<bool> Delete(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return false;

        _context.Employees.Remove(employee);
        var deleted = await _context.SaveChangesAsync();
        return deleted > 0;
    }

    public async Task<bool> Update(int id, EmployeeUpdateDto dto)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return false;

        employee.Name = dto.Name;
        employee.LastName = dto.LastName;
        employee.Email = dto.Email;
        employee.BranchId = dto.BranchId;

        _context.Employees.Update(employee);
        var updated = await _context.SaveChangesAsync();
        return updated > 0;
    }
}