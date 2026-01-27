using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Models;

public class Employee
{
    [Key] 
    public int Id { get; set; }
    [MaxLength(50)]
    public string Name { get; set; } = null!;
    [MaxLength(100)]
    public string LastName { get; set; } = null!;
    [MaxLength(300)]
    public string Email { get; set; } = null!;
    [JsonIgnore, ValidateNever] 
    public int BranchId { get; set; }
    [JsonIgnore, ValidateNever] 
    public Branch Branch { get; set; } = null!;
    [JsonIgnore, ValidateNever]
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}