using Models;

namespace Services;

public interface IEmployee
{
    public Task<bool> Create(Employee employee);
    public Task<Employee?> ReadOne(int id);
    public Task<List<Employee>> ReadAll();
    public Task<bool> Delete(int id);
    public Task<bool> Update(int id, Employee uOrder);
}