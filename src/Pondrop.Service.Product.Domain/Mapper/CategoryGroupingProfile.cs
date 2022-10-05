using AutoMapper;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Domain.Models.Category;

namespace Pondrop.Service.Product.Domain.Mapper;

public class CategoryGroupingProfile : Profile
{
    public CategoryGroupingProfile()
    {
        CreateMap<CategoryGroupingEntity, CategoryGroupingRecord>();
    }
}
