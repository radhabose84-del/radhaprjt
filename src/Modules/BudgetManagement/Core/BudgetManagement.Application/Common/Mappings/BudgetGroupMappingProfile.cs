using AutoMapper;
using BudgetManagement.Application.BudgetGroups.Commands.UpdateBudgetGroup;
using BudgetManagement.Application.BudgetGroups.Commands.CreateBudgetGroup;
using BudgetManagement.Application.BudgetGroup.Command.DeleteBudgetGroup;
using static BudgetManagement.Domain.Common.BaseEntity;

namespace BudgetManagement.Application.Common.Mappings
{
    public class BudgetGroupMappingProfile : Profile
    {
        public BudgetGroupMappingProfile()
        {

            CreateMap<CreateBudgetGroupCommand, Domain.Entities.BudgetGroup>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.BudgetTypeId, opt => opt.MapFrom(src => src.BudgetTypeId))
                .ForMember(dest => dest.CarryForward, opt => opt.MapFrom(src => src.CarryForward))
                .ForMember(dest => dest.EmergencyPoApplicable, opt => opt.MapFrom(src => src.EmergencyPoApplicable));

            CreateMap<UpdateBudgetGroupCommand, Domain.Entities.BudgetGroup>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.BudgetTypeId, opt => opt.MapFrom(src => src.BudgetTypeId))
                .ForMember(dest => dest.CarryForward, opt => opt.MapFrom(src => src.CarryForward))
                .ForMember(dest => dest.EmergencyPoApplicable, opt => opt.MapFrom(src => src.EmergencyPoApplicable));
                
           // Delete command mapping (soft delete)
            CreateMap<DeleteBudgetGroupCommand, Domain.Entities.BudgetGroup>()
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted)); 
        }
    }
}
