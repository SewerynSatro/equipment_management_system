namespace DTOs;

public class LoanUpdateDto
{
    public DateTime? LoanDate { get; set; } = null;
    public DateTime? ReturnDate { get; set; } = null;
    public bool Returned { get; set; } = false;
}