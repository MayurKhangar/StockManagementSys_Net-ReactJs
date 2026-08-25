using SmartStock.Domain.Enums;

namespace SmartStock.Domain.Entities;

public class AuditLog : BaseEntity
{
    public int? UserId { get; set; }
    public User? User { get; set; }

    public AuditAction Action { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
}
