using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Models;

public class Loan
{
    [Key]
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    [JsonIgnore, ValidateNever]
    public Employee Employee { get; set; } = null!;
    public int DeviceId { get; set; }
    [JsonIgnore, ValidateNever]
    public Device Device { get; set; } = null!;
    public DateTime LoanDate { get; set; } = DateTime.UtcNow;
    public DateTime? ReturnDate { get; set; } = null;
    public bool Returned { get; set; } = false;
}