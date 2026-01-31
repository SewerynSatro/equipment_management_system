using DTOs;
using Models;

namespace Services;

public interface ILoanService
{
    public Task<LoanReadDto?> Create(LoanCreateDto dto);
    public Task<LoanReadDto?> ReadOne(int id);
    public Task<List<LoanReadDto>> ReadAll();
    public Task<bool> Delete(int id);
    public Task<bool> Update(int id, LoanUpdateDto dto);
    public Task<List<LoanReadDto>?> ShowUserActiveLoans(int userId);
    public Task<List<LoanReadDto>?> ShowUserHistory(int userId);
    public Task<List<LoanReadDto>> ShowActiveLoans();
    public Task<bool> Return(int id);
}