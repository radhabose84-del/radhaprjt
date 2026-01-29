using AutoMapper;
using FAM.Application.AssetSubGroup.Command.CreateAssetSubGroup;
using FAM.Application.AssetSubGroup.Command.DeleteAssetSubGroup;
using FAM.Application.AssetSubGroup.Command.UpdateAssetSubGroup;
using FAM.Application.AssetSubGroup.Queries.GetAssetSubGroup;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.Common.Mappings
{
    public class AssetSubGroupProfile : Profile
    {
        public AssetSubGroupProfile()
        {
            CreateMap<FAM.Domain.Entities.AssetSubGroup,AssetSubGroupDto>();
            CreateMap<CreateAssetSubGroupCommand, FAM.Domain.Entities.AssetSubGroup>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.SubGroupName, opt => opt.MapFrom(src => src.SubGroupName))
                .ForMember(dest => dest.SortOrder, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateAssetSubGroupCommand, FAM.Domain.Entities.AssetSubGroup>()
                .ForMember(dest => dest.Code, opt => opt.Ignore())
                .ForMember(dest => dest.SubGroupName, opt => opt.MapFrom(src => src.SubGroupName))
                .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder))
                .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.GroupId))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteAssetSubGroupCommand, FAM.Domain.Entities.AssetSubGroup>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) 
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));               

            CreateMap<FAM.Domain.Entities.AssetSubGroup, AssetSubGroupAutoCompleteDTO>(); 

                  
        }
    }
}
