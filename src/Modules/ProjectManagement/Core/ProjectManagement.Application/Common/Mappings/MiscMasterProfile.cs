using AutoMapper;
using ProjectManagement.Application.MiscMaster.Command.CreateMiscMaster;
using ProjectManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using ProjectManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using ProjectManagement.Application.MiscMaster.Queries.GetMiscMaster;
using static ProjectManagement.Domain.Common.BaseEntity;

namespace ProjectManagement.Application.Common.Mappings
{
    public class MiscMasterProfile : Profile


    {
        public MiscMasterProfile()
        {
            CreateMap<ProjectManagement.Domain.Entities.MiscMaster, GetMiscMasterDto>();

            CreateMap<ProjectManagement.Domain.Entities.MiscMaster, GetMiscMasterAutoCompleteDto>();

            CreateMap<CreateMiscMasterCommand, ProjectManagement.Domain.Entities.MiscMaster>()
           .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
           .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateMiscMasterCommand, ProjectManagement.Domain.Entities.MiscMaster>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteMiscMasterCommand, ProjectManagement.Domain.Entities.MiscMaster>()
           .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
        }
    }
}