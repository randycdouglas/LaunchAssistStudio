using System.ComponentModel.DataAnnotations;

namespace LaunchAssistStudio.Web.Data.Entities;

/// <summary>An internal note or follow-up record attached to a lead.</summary>
public class LeadNote
{
    public int Id { get; set; }
    public int LeadId { get; set; }
    public Lead Lead { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }
    [MaxLength(200)] public string? Author { get; set; }
    [MaxLength(8000)] public string Body { get; set; } = "";
}
