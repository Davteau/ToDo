using System.ComponentModel.DataAnnotations;

namespace Application.Common.Models;

public class ToDo
{
    public Guid Id { get; set; }

    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 100)]
    public int PercentComplete { get; set; }

    public DateTime ExpiryDate { get; set; }
}
