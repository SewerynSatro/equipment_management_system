using Microsoft.EntityFrameworkCore;
using Data;
using DTOs;
using Models;

namespace Services;

public class LoanService : ILoanService
{
    private readonly AppDbContext _context;

    public LoanService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LoanReadDto?> Create(LoanCreateDto dto)
    {
        var device = await _context.Devices.FindAsync(dto.DeviceId);
        if (device == null || !device.Available)
            return null;

        var employee = await _context.Employees.FindAsync(dto.EmployeeId);
        if (employee == null)
            return null;

        var loan = new Loan
        {
            EmployeeId = dto.EmployeeId,
            DeviceId = dto.DeviceId,
            LoanDate = DateTime.UtcNow,
            Returned = false
        };

        device.Available = false;

        await _context.Loans.AddAsync(loan);
        await _context.SaveChangesAsync();

        await _context.Entry(loan).Reference(l => l.Employee).LoadAsync();
        await _context.Entry(loan).Reference(l => l.Device).LoadAsync();

        return new LoanReadDto
        {
            Id = loan.Id,
            EmployeeName = loan.Employee.Name,
            EmployeeLastName = loan.Employee.LastName,
            DeviceSerialNumber = loan.Device.SerialNumber,
            LoanDate = loan.LoanDate,
            ReturnDate = loan.ReturnDate,
            Returned = loan.Returned
        };
    }

    public async Task<LoanReadDto?> ReadOne(int id)
    {
        var loan = await _context.Loans
            .Include(l => l.Employee)
            .Include(l => l.Device)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan == null)
            return null;

        return new LoanReadDto
        {
            Id = loan.Id,
            EmployeeName = loan.Employee.Name,
            EmployeeLastName = loan.Employee.LastName,
            DeviceSerialNumber = loan.Device.SerialNumber,
            LoanDate = loan.LoanDate,
            ReturnDate = loan.ReturnDate,
            Returned = loan.Returned
        };
    }

    public async Task<List<LoanReadDto>> ReadAll()
    {
        var loans = await _context.Loans
            .Include(l => l.Employee)
            .Include(l => l.Device)
            .ToListAsync();

        return loans.Select(loan => new LoanReadDto
        {
            Id = loan.Id,
            EmployeeName = loan.Employee.Name,
            EmployeeLastName = loan.Employee.LastName,
            DeviceSerialNumber = loan.Device.SerialNumber,
            LoanDate = loan.LoanDate,
            ReturnDate = loan.ReturnDate,
            Returned = loan.Returned
        }).ToList();
    }

    public async Task<bool> Update(int id, LoanUpdateDto dto)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan == null)
            return false;

        loan.Returned = dto.Returned;
        loan.ReturnDate = dto.ReturnDate;

        if (dto.Returned)
        {
            var device = await _context.Devices.FindAsync(loan.DeviceId);
            if (device != null)
                device.Available = true;
        }

        _context.Loans.Update(loan);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<LoanReadDto>?> ShowUserActiveLoans(int userId)
    {
        var userExists = await _context.Employees.AnyAsync(e => e.Id == userId);
        if (!userExists)
            return null;

        var loans = await _context.Loans
            .Include(l => l.Employee)
            .Include(l => l.Device)
            .Where(l => l.EmployeeId == userId && !l.Returned)
            .ToListAsync();

        return loans.Select(loan => new LoanReadDto
        {
            Id = loan.Id,
            EmployeeName = loan.Employee.Name,
            EmployeeLastName = loan.Employee.LastName,
            DeviceSerialNumber = loan.Device.SerialNumber,
            LoanDate = loan.LoanDate,
            ReturnDate = loan.ReturnDate,
            Returned = loan.Returned
        }).ToList();
    }

    public async Task<List<LoanReadDto>?> ShowUserHistory(int userId)
    {
        var userExists = await _context.Employees.AnyAsync(e => e.Id == userId);
        if (!userExists)
            return null;

        var loans = await _context.Loans
            .Include(l => l.Employee)
            .Include(l => l.Device)
            .Where(l => l.EmployeeId == userId && l.Returned)
            .ToListAsync();

        return loans.Select(loan => new LoanReadDto
        {
            Id = loan.Id,
            EmployeeName = loan.Employee.Name,
            EmployeeLastName = loan.Employee.LastName,
            DeviceSerialNumber = loan.Device.SerialNumber,
            LoanDate = loan.LoanDate,
            ReturnDate = loan.ReturnDate,
            Returned = loan.Returned
        }).ToList();
    }

    public async Task<List<LoanReadDto>> ShowActiveLoans()
    {
        var loans = await _context.Loans
            .Include(l => l.Employee)
            .Include(l => l.Device)
            .Where(l => !l.Returned)
            .ToListAsync();

        return loans.Select(loan => new LoanReadDto
        {
            Id = loan.Id,
            EmployeeName = loan.Employee.Name,
            EmployeeLastName = loan.Employee.LastName,
            DeviceSerialNumber = loan.Device.SerialNumber,
            LoanDate = loan.LoanDate,
            ReturnDate = loan.ReturnDate,
            Returned = loan.Returned
        }).ToList();
    }

    public async Task<bool> Return(int id)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan == null)
            return false;

        if (loan.Returned)
            return false;

        var device = await _context.Devices.FindAsync(loan.DeviceId);
        if (device == null)
            return false;

        loan.Returned = true;
        loan.ReturnDate = DateTime.UtcNow;
        device.Available = true;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Delete(int id)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan == null)
            return false;

        if (!loan.Returned)
        {
            var device = await _context.Devices.FindAsync(loan.DeviceId);
            if (device != null)
                device.Available = true;
        }

        _context.Loans.Remove(loan);
        await _context.SaveChangesAsync();
        return true;
    }
}
