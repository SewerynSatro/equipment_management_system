using Models;

namespace Services;

public interface ILoanService
{
    public Task<bool> Create(Loan loan);
    public Task<Loan?> ReadOne(int id);
    public Task<List<Loan>> ReadAll();
    public Task<bool> Delete(int id);
    public Task<bool> Update(int id, Loan updatedLoan);
    public Task<List<Loan>?> ShowUserActiveLoans(int userId);
    public Task<List<Loan>?> ShowUserHistory(int userId);
    public Task<List<Loan>> ShowActiveLoans();
    public Task<bool> Return(int id);
}