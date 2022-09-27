using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Domain.Models.Product;

namespace Pondrop.Service.Product.Domain.Models.Category;

public record CategoryWithProductsViewRecord(
        Guid Id,
        string Name,
        List<ProductViewRecord> Products)
{
    public CategoryWithProductsViewRecord() : this(
        Guid.Empty,
        string.Empty,
        new List<ProductViewRecord>())
    {
    }
}