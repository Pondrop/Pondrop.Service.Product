namespace Pondrop.Service.Product.Domain.Models;

public record CategoryGroupingViewRecord(
    Guid Id,
    Guid HigherLevelCategoryId,
    string ParentName,
    Guid LowerLevelCategoryId,
    string CategoryName)
{
    public CategoryGroupingViewRecord() : this(
        Guid.Empty,
        Guid.Empty,
        string.Empty,
        Guid.Empty,
        string.Empty)
    {
    }
}