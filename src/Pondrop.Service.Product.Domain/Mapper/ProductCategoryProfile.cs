using AutoMapper;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.ProductCategory.Domain.Models;

namespace Pondrop.Service.Product.Domain.Mapper;

public class ProductCategoryProfile : Profile
{
    public ProductCategoryProfile()
    {
        CreateMap<ProductCategoryEntity, ProductCategoryRecord>();
    }
}
