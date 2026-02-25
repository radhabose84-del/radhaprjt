using AutoMapper;
using BudgetManagement.Application.BudgetAllocation.Command.Create;
using static BudgetManagement.Domain.Common.BaseEntity;

namespace BudgetManagement.Application.Common.Mappings
{
    public class BudgetAllocationProfile : Profile
    {
        public BudgetAllocationProfile()
        {

            CreateMap<CreateBudgetAllocationDto, BudgetManagement.Domain.Entities.BudgetAllocation>()
                 .ForMember(dest => dest.Id, opt => opt.Ignore())
                 .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                 .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));   


            
        }
    }
}