using AutoMapper;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Domain.Mapper;

public class BrandProfile : Profile
{
    public BrandProfile()
    {
        CreateMap<BrandEntity, BrandRecord>();
    }
}
