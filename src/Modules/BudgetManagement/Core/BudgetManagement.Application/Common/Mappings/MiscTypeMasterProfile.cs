using AutoMapper;
using BudgetManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using BudgetManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using BudgetManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using BudgetManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using static BudgetManagement.Domain.Common.BaseEntity;

namespace BudgetManagement.Application.Common.Mappings
{
    public class MiscTypeMasterProfile : Profile
    {
        public MiscTypeMasterProfile()
        {
            CreateMap<BudgetManagement.Domain.Entities.MiscTypeMaster,GetMiscTypeMasterDto>();
            
            CreateMap<BudgetManagement.Domain.Entities.MiscTypeMaster, GetMiscTypeMasterAutocompleteDto>();

            CreateMap<CreateMiscTypeMasterCommand, BudgetManagement.Domain.Entities.MiscTypeMaster>()
              .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
              .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateMiscTypeMasterCommand, BudgetManagement.Domain.Entities.MiscTypeMaster>()
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));
               
            CreateMap<DeleteMiscTypeMasterCommand, BudgetManagement.Domain.Entities.MiscTypeMaster>()
               .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted)); 
        }

    }
}