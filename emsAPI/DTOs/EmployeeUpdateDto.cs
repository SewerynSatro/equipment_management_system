using System.ComponentModel.DataAnnotations;

namespace DTOs;

public class EmployeeUpdateDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = null!;
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = null!;
    [Required]
    [MaxLength(300)]
    public string Email { get; set; } = null!;
    [Required]
    public int BranchId { get; set; }
}