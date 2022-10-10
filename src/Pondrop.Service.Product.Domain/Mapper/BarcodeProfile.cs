using AutoMapper;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Domain.Mapper;

public class BarcodeProfile : Profile
{
    public BarcodeProfile()
    {
        CreateMap<BarcodeEntity, BarcodeRecord>();
    }
}
