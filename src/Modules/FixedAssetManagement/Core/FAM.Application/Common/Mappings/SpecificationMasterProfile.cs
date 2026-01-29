using AutoMapper;
using FAM.Application.SpecificationMaster.Commands.CreateSpecificationMaster;
using FAM.Application.SpecificationMaster.Commands.DeleteSpecificationMaster;
using FAM.Application.SpecificationMaster.Commands.UpdateSpecificationMaster;
using FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster;
using FAM.Domain.Entities;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.Common.Mappings
{
    public class SpecificationMasterProfile : Profile
    {
        public SpecificationMasterProfile()
        { 
            CreateMap<DeleteSpecificationMasterCommand, SpecificationMasters>()            
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));            
            
            CreateMap<CreateSpecificationMasterCommand, SpecificationMasters>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))            
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));             

            CreateMap<UpdateSpecificationMasterCommand, SpecificationMasters>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));    
             
            CreateMap<SpecificationMasters, SpecificationMasterAutoCompleteDTO>();   
             
            CreateMap<SpecificationMasters, SpecificationMasterDTO>();             
        }
    }
}