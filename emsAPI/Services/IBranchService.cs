using Models;

namespace Services;

public interface IBranchService
{
    public Task<bool> Create(Branch branch);
    public Task<Branch?> ReadOne(int id);
    public Task<List<Branch>> ReadAll();
    public Task<bool> Delete(int id);
    public Task<bool> Update(int id, Branch updatedBranch);
}