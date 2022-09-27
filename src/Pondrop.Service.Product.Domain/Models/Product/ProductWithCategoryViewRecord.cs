using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Domain.Models.Product;

public record ProductWithCategoryViewRecord(
    Guid Id,
    string Name,
    string ShortDescription,
    List<CategoryViewRecord> Categories
    )
{
    public ProductWithCategoryViewRecord() : this(
        Guid.Empty,
        string.Empty,
        string.Empty,
        new List<CategoryViewRecord>())
    {
    }
}