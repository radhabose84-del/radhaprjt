using AutoMapper;
using FAM.Application.AssetGroup.Command.CreateAssetGroup;
using FAM.Application.AssetGroup.Command.DeleteAssetGroup;
using FAM.Application.AssetGroup.Command.UpdateAssetGroup;
using FAM.Application.AssetGroup.Queries.GetAssetGroup;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.Common.Mappings
{
    public class AssetGroupProfile : Profile
    {
        public AssetGroupProfile()
        {
            CreateMap<FAM.Domain.Entities.AssetGroup,AssetGroupDto>();
            CreateMap<CreateAssetGroupCommand, FAM.Domain.Entities.AssetGroup>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.GroupName))
                .ForMember(dest => dest.SortOrder, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateAssetGroupCommand, FAM.Domain.Entities.AssetGroup>()
                .ForMember(dest => dest.Code, opt => opt.Ignore())
                .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.GroupName))
                .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteAssetGroupCommand, FAM.Domain.Entities.AssetGroup>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) 
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));    
           

            CreateMap<FAM.Domain.Entities.AssetGroup, AssetGroupAutoCompleteDTO>(); 

                  
        }
    }
}
