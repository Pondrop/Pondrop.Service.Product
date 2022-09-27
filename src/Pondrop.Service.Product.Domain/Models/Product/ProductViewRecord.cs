namespace Pondrop.Service.Product.Domain.Models.Product;

public record ProductViewRecord(
    Guid Id,
    string Name,
    string ShortDescription)
{
    public ProductViewRecord() : this(
        Guid.Empty,
        string.Empty,
        string.Empty)
    {
    }
}