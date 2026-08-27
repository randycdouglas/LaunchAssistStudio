using System.ComponentModel.DataAnnotations;

namespace LaunchAssistStudio.Web.Data.Entities;

/// <summary>Audit trail of lead status changes (supports conversion tracking).</summary>
public class LeadStatusHistory
{
    public int Id { get; set; }
    public int LeadId { get; set; }
    public Lead Lead { get; set; } = null!;

    [MaxLength(50)] public string? FromStatus { get; set; }
    [MaxLength(50)] public string ToStatus { get; set; } = "";
    public DateTime ChangedAtUtc { get; set; }
    [MaxLength(200)] public string? ChangedBy { get; set; }
}
