using AutoMapper;
using ProjectManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using static ProjectManagement.Domain.Common.BaseEntity;

namespace ProjectManagement.Application.Common.Mappings
{
    public class MiscTypeMasterProfile : Profile
    {
        public MiscTypeMasterProfile()
        {
            CreateMap<ProjectManagement.Domain.Entities.MiscTypeMaster,GetMiscTypeMasterDto>();
            
            CreateMap<ProjectManagement.Domain.Entities.MiscTypeMaster, GetMiscTypeMasterAutocompleteDto>();

            CreateMap<CreateMiscTypeMasterCommand, ProjectManagement.Domain.Entities.MiscTypeMaster>()
              .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
              .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateMiscTypeMasterCommand, ProjectManagement.Domain.Entities.MiscTypeMaster>()
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));
               
            CreateMap<DeleteMiscTypeMasterCommand, ProjectManagement.Domain.Entities.MiscTypeMaster>()
               .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted)); 
        }

    }
}