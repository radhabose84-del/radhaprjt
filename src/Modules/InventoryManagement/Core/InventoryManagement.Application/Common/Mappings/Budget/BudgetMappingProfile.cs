using AutoMapper;
using InventoryManagement.Application.Budget.Commands.CreateBudget;
using InventoryManagement.Application.Budget.Commands.UpdateBudget;
using InventoryManagement.Domain.Entities.Budget;

namespace InventoryManagement.Application.Budget.Mapping
{
    public class BudgetMappingProfile : Profile
    {
        public BudgetMappingProfile()
        {
            // ✅ Map Create Command to BudgetMaster
            CreateMap<CreateBudgetCommand, BudgetMaster>()
                .ForMember(dest => dest.BudgetDetail, opt => opt.MapFrom(src => src.BudgetDetails));

            // ✅ Map BudgetDetailDto to BudgetDetail entity
            CreateMap<BudgetDetailDto, BudgetDetail>();

            // ✅ Map Update Command details
            CreateMap<UpdateBudgetDetailDto, BudgetDetail>()
                .ForMember(dest => dest.BudgetAmount, opt => opt.MapFrom(src => src.NewAmount));
        }
    }
}
