using AutoMapper;
using FAM.Application.DepreciationGroup.Commands.CreateDepreciationGroup;
using FAM.Application.DepreciationGroup.Commands.DeleteDepreciationGroup;
using FAM.Application.DepreciationGroup.Commands.UpdateDepreciationGroup;
using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroup;
using FAM.Domain.Entities;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.Common.Mappings
{
    public class DepreciationGroupProfile : Profile
    {
        public DepreciationGroupProfile()
        { 
            CreateMap<DeleteDepreciationGroupCommand, DepreciationGroups>()            
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));            
            
            CreateMap<CreateDepreciationGroupCommand, DepreciationGroups>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))            
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted)); 

             CreateMap<DepreciationGroups, DepreciationGroupDTO>()
                .ForMember(dest => dest.BookType, opt => opt.MapFrom(src => src.BookMiscType.Description))
                .ForMember(dest => dest.DepreciationMethod, opt => opt.MapFrom(src => src.DepMiscType.Description))
                .ForMember(dest => dest.AssetGroupName, opt => opt.MapFrom(src => src.AssetGroup.GroupName));

            CreateMap<UpdateDepreciationGroupCommand, DepreciationGroups>()            
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (Status)src.IsActive))
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());    
             
            CreateMap<DepreciationGroupDTO, DepreciationGroupAutoCompleteDTO>();    
            CreateMap<DepreciationGroups, DepreciationGroupDTO>();             
        }
    }
}