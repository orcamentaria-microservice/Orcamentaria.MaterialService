using AutoMapper;
using MongoDB.Bson;
using Orcamentaria.MaterialService.Domain.DTOs.Material;
using Orcamentaria.MaterialService.Domain.DTOs.MaterialType;
using Orcamentaria.MaterialService.Domain.Models;

namespace Orcamentaria.MaterialService.Domain.Mappers
{
    public class MaterialMapper : Profile
    {
        public MaterialMapper() 
        {
            CreateMap<Material, MaterialResponseDTO>()
                .ForMember(s => s.Suppliers, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<Material, MaterialInsertDTO>().ReverseMap();

            CreateMap<Material, MaterialUpdateDTO>().ReverseMap();
        }
    }
}
