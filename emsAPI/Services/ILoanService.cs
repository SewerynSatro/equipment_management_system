using Models;

namespace Services;

public interface ILoanService
{
    public Task<bool> Create(Loan loan);
    public Task<Loan?> ReadOne(int id);
    public Task<List<Loan>> ReadAll();
    public Task<bool> Delete(int id);
    public Task<bool> Update(int id, Loan updatedLoan);
    // TODO
    // methods for showing users current devices, user device history, all active loans etc...
}