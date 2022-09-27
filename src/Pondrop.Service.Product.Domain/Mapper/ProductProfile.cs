using AutoMapper;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Domain.Models.Product;

namespace Pondrop.Service.Product.Domain.Mapper;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<ProductEntity, ProductRecord>();
        CreateMap<ProductEntity, ProductViewRecord>();
        CreateMap<ProductEntity, ProductWithCategoryViewRecord>();
    }
}
