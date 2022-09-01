namespace Pondrop.Service.Product.Domain.Models;

public record AuditRecord(string CreatedBy, string UpdatedBy, DateTime CreatedUtc, DateTime UpdatedUtc);