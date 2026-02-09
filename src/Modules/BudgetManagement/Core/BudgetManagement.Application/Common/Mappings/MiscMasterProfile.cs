using AutoMapper;
using BudgetManagement.Application.MiscMaster.Command.CreateMiscMaster;
using BudgetManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using BudgetManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using BudgetManagement.Application.MiscMaster.Queries.GetMiscMaster;
using static BudgetManagement.Domain.Common.BaseEntity;

namespace BudgetManagement.Application.Common.Mappings
{
    public class MiscMasterProfile : Profile


    {
        public MiscMasterProfile()
        {
            CreateMap<BudgetManagement.Domain.Entities.MiscMaster, GetMiscMasterDto>();

            CreateMap<BudgetManagement.Domain.Entities.MiscMaster, GetMiscMasterAutoCompleteDto>();

            CreateMap<CreateMiscMasterCommand, BudgetManagement.Domain.Entities.MiscMaster>()
           .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
           .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateMiscMasterCommand, BudgetManagement.Domain.Entities.MiscMaster>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteMiscMasterCommand, BudgetManagement.Domain.Entities.MiscMaster>()
           .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
        }
    }
}