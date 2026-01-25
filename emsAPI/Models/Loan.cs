using System.ComponentModel.DataAnnotations;

namespace Models;

public class Loan
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    
    [Required]
    public int DeviceId { get; set; }
    public Device Device { get; set; } = null!;
    
    public DateTime LoanDate { get; set; } = DateTime.UtcNow;
    public DateTime? ReturnDate { get; set; }
    
    public bool Returned { get; set; }
}