using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Models;

public class Device
{
    [Key]
    public int Id { get; set; }

    public int TypeId { get; set; }
    public int ProducerId { get; set; }

    public bool Available { get; set; }

    [Required]
    [MaxLength(100)]
    public string SerialNumber { get; set; } = null!;
    
    [JsonIgnore, ValidateNever]
    public DeviceType Type { get; set; } = null!;
    
    [JsonIgnore, ValidateNever]
    public Producer Producer { get; set; } = null!;
    
    [JsonIgnore, ValidateNever]
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}