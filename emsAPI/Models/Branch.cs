using System.ComponentModel.DataAnnotations;

namespace Models;

public class Branch
{
    [Key]
    public int Id { get; set; }
    
    [Required, MaxLength(100)]
    public required string Name { get; set; }
}