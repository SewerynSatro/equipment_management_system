using System.ComponentModel.DataAnnotations;
using Models;

namespace DTOs;

public class EmployeeReadDto
{
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string BranchName { get; set; } = null!;
}