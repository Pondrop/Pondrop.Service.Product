using AutoMapper;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Domain.Models.Category;

namespace Pondrop.Service.Product.Domain.Mapper;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<CategoryEntity, CategoryRecord>();
        CreateMap<CategoryEntity, CategoryViewRecord>();
        CreateMap<CategoryEntity, CategoryWithProductsViewRecord>();
    }
}
