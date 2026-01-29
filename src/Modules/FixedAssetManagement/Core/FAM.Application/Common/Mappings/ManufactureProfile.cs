using AutoMapper;
using FAM.Application.Manufacture.Commands.CreateManufacture;
using FAM.Application.Manufacture.Commands.DeleteManufacture;
using FAM.Application.Manufacture.Commands.UpdateManufacture;
using FAM.Application.Manufacture.Queries.GetManufacture;
using FAM.Domain.Entities;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.Common.Mappings
{
    public class ManufactureProfile : Profile
    {
         public ManufactureProfile()
        { 
            CreateMap<DeleteManufactureCommand, Manufactures>()            
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));            
            
            CreateMap<CreateManufactureCommand, Manufactures>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))            
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted)); 

            CreateMap<UpdateManufactureCommand, Manufactures>()            
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive))
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            CreateMap<ManufactureDTO, ManufactureAutoCompleteDTO>();    
            CreateMap<Manufactures, ManufactureDTO>();             
        }
    }
}