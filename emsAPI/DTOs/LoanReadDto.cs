namespace DTOs;

public class LoanReadDto
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = null!;
    public string EmployeeLastName { get; set; } = null!;
    public string DeviceSerialNumber { get; set; } = null!;
    public DateTime LoanDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public bool Returned { get; set; }
}