using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pondrop.Service.Product.Domain.Models.ProductCategory;

public record ParentCategoryViewRecord(
        Guid Id,
        string CategoryName,
        int ProductCount)
{
    public ParentCategoryViewRecord() : this(
        Guid.Empty,
        string.Empty,
        0)
    {
    }
}
