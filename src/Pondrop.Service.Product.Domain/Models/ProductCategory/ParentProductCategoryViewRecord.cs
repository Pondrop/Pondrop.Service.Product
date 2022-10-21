using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pondrop.Service.Product.Domain.Models.ProductCategory;

public record ParentProductCategoryViewRecord(
        Guid Id,
        Guid? ParentCategoryId,
        string Name,
        string? BarcodeNumber,
        string CategoryNames,
        List<CategoryViewRecord>? Categories)
{
    public ParentProductCategoryViewRecord() : this(
        Guid.Empty,
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        null)
    {
    }
}
