using DTOs;
using Models;

namespace Services;

public interface IEmployeeService
{
    public Task<EmployeeReadDto?> Create(EmployeeCreateDto dto);
    public Task<EmployeeReadDto?> ReadOne(int id);
    public Task<List<EmployeeReadDto>> ReadAll();
    public Task<bool> Delete(int id);
    public Task<bool> Update(int id, EmployeeUpdateDto dto);
}