using System.ComponentModel.DataAnnotations;

namespace Models;

public class Employee
{
    [Key] 
    public int Id { get; set; }

    [Required, MaxLength(50)] 
    public required string Name { get; set; }

    [Required, MaxLength(100)] 
    public required string LastName { get; set; }

    [Required, MaxLength(300)] 
    public required string Email { get; set; }

    [Required] 
    public int BranchId { get; set; }
    public Branch Branch { get; set; }
}