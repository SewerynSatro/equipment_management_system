using Microsoft.EntityFrameworkCore;
using Data;
using Models;

namespace Services;

public class LoanService : ILoanService
{
    private readonly AppDbContext _context;

    public LoanService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Create(Loan loan)
    {
        var device = await _context.Devices.FindAsync(loan.DeviceId);
        if (device == null)
            return false;

        if (!device.Available)
            return false;

        var employee = await _context.Employees.FindAsync(loan.EmployeeId);
        if (employee == null)
            return false;

        device.Available = false;

        await _context.Loans.AddAsync(loan);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Loan?> ReadOne(int id)
    {
        return await _context.Loans
            .Include(l => l.Employee)
            .Include(l => l.Device)
            .ThenInclude(d => d.Type)
            .Include(l => l.Device)
            .ThenInclude(d => d.Producer)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<List<Loan>> ReadAll()
    {
        return await _context.Loans
            .Include(l => l.Employee)
            .Include(l => l.Device)
            .ThenInclude(d => d.Type)
            .Include(l => l.Device)
            .ThenInclude(d => d.Producer)
            .ToListAsync();
    }

    public async Task<bool> Update(int id, Loan updatedLoan)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan == null)
            return false;

        loan.ReturnDate = updatedLoan.ReturnDate;
        loan.Returned = updatedLoan.Returned;

        if (updatedLoan.Returned && loan.Device != null)
        {
            var device = await _context.Devices.FindAsync(loan.DeviceId);
            if (device != null)
                device.Available = true;
        }

        _context.Loans.Update(loan);
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
